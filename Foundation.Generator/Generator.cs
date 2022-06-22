using Amazon.Lambda.Annotations.SourceGenerator;
using Amazon.Lambda.Annotations.SourceGenerator.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.FileIO;
using Amazon.Lambda.Annotations.SourceGenerator.Writers;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;
using Amazon.Lambda.Annotations.SourceGenerator.Extensions;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Templates;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Foundation.Generators
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        private readonly IFileManager _fileManager = new FileManager();
        private readonly IDirectoryManager _directoryManager = new DirectoryManager();
        private readonly IJsonWriter _jsonWriter = new JsonWriter();

        
        public void Initialize(GeneratorInitializationContext context)
        {
            Debugger.Launch();
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Debugger.Launch();
            var diagnosticReporter = new DiagnosticReporter(context);

            try
            {
                // retrieve the populated receiver
                if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                {
                    return;
                }

                //// If there are no Lambda methods, return early
                //if (!receiver.LambdaMethods.Any())
                //{
                //    return;
                //}

                if (receiver.MigrationFunction is null)
                {
                    return;
                }

                if (!receiver.MigrationClasses.Any())
                {
                    return;
                }

                //var semanticModelProvider = new SemanticModelProvider(context);
                //if (receiver.StartupClasses.Count > 1)
                //{
                //    foreach (var startup in receiver.StartupClasses)
                //    {
                //        // If there are more than one startup class, report them as errors
                //        diagnosticReporter.Report(Diagnostic.Create(DiagnosticDescriptors.MultipleStartupNotAllowed,
                //            Location.Create(startup.SyntaxTree, startup.Span),
                //            startup.SyntaxTree.FilePath));
                //    }
                //}

                //var configureMethodModel = semanticModelProvider.GetConfigureMethodModel(receiver.StartupClasses.FirstOrDefault());

                var semanticModelProvider = new FoundationSemanticModelProvider(context);
                IMethodSymbol lambdaMethodSymbol = semanticModelProvider.GetMethodSemanticModel(receiver.MigrationFunction);
                ILambdaFunctionSerializable migrationLambdaFunctionModel = LambdaFunctionModelBuilder.Build(lambdaMethodSymbol, null, context);

                var templateFinder = new CloudFormationTemplateFinder(_fileManager, _directoryManager);
                var projectRootDirectory = templateFinder.DetermineProjectRootDirectory(receiver.MigrationFunction.SyntaxTree.FilePath);
                var annotationReport = new FoundationAnnotationReport
                {
                    MigrationFunctionModel = migrationLambdaFunctionModel,
                    CloudFormationTemplatePath = templateFinder.FindCloudFormationTemplate(projectRootDirectory),
                    ProjectRootDirectory = projectRootDirectory
                };




                foreach (var receiverMigrationClass in receiver.MigrationClasses)
                {
                    IMigrationModel migrationModel = MigrationModelBuilder.Build(migrationLambdaFunctionModel, receiverMigrationClass);
                    annotationReport.Migrations.Add(migrationModel);
                }

                var cloudFormationJsonWriter = new FoundationCloudFormationJsonWriter(_fileManager, _directoryManager, _jsonWriter, diagnosticReporter);
                cloudFormationJsonWriter.ApplyReport(annotationReport);
            }
            catch (Exception e)
            {
                // this is a generator failure, report this as error
                diagnosticReporter.Report(Diagnostic.Create(DiagnosticDescriptors.UnhandledException, Location.None, e.PrettyPrint()));
#if DEBUG
                throw;
#endif
            }
        }
    }

    public static class MigrationModelBuilder
    {
        public static IMigrationModel Build(ILambdaFunctionSerializable lambdaFunctionModel, ITypeSymbol receiverMigrationClass)
        {
            if (receiverMigrationClass.GetAttributes().SingleOrDefault(_ => $"{_.AttributeClass.ContainingNamespace}.{_.AttributeClass.Name}" == "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute") is not { } migrationAttribute
                || !migrationAttribute.ConstructorArguments.Any())
            {
                throw new InvalidOperationException();
            }
            string migrationId = migrationAttribute.ConstructorArguments.FirstOrDefault().Value!.ToString();

            return new MigrationModel(lambdaFunctionModel, receiverMigrationClass.ContainingNamespace.ToString(), receiverMigrationClass.Name, migrationId, lambdaFunctionModel.Name);
        }
    }

    public class MigrationModel : IMigrationModel
    {
        private readonly ILambdaFunctionSerializable _lambdaFunctionModel;

        public MigrationModel(ILambdaFunctionSerializable lambdaFunctionModel, string @namespace, string typeName, string migrationId, string resourceName)
        {
            _lambdaFunctionModel = lambdaFunctionModel;
            Name = typeName;
            ResourceName = resourceName;
            Namespace = @namespace;
            Id = migrationId;
        }

        public string FunctionResourceName => _lambdaFunctionModel.Name;
        public string Name { get; }
        public string ResourceName { get; }
        public string Namespace { get; }
        public string FullName => this.Namespace + "." + Name;
        public string Id { get; }
    }

    public class FoundationSemanticModelProvider : SemanticModelProvider
    {
        private readonly GeneratorExecutionContext _context;

        public FoundationSemanticModelProvider(GeneratorExecutionContext context) : base(context)
        {
            _context = context;
        }
    }
}