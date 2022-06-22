using Amazon.Lambda.Annotations.SourceGenerator;
using Amazon.Lambda.Annotations.SourceGenerator.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.FileIO;
using Amazon.Lambda.Annotations.SourceGenerator.Writers;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Text;
using Amazon.Lambda.Annotations.SourceGenerator.Extensions;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Models.Attributes;
using Amazon.Lambda.Annotations.SourceGenerator.Templates;
using Foundation.Annotations;
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
#if DEBUG
            //Debugger.Launch();
#endif
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
#if DEBUG
            //Debugger.Launch();
#endif
            var diagnosticReporter = new DiagnosticReporter(context);

            try
            {
                // retrieve the populated receiver
                if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                {
                    return;
                }

                if (receiver.MigrationFunction is null)
                {
                    return;
                }

                if (!receiver.MigrationClasses.Any())
                {
                    return;
                }

                var semanticModelProvider = new FoundationSemanticModelProvider(context);

                IMethodSymbol lambdaMethodSymbol = semanticModelProvider.GetMethodSemanticModel(receiver.MigrationFunction);
                var migrationLambdaFunctionModel = FoundationLambdaFunctionModelBuilder.Build(lambdaMethodSymbol, null, context);
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

    public class FoundationLambdaFunctionModelBuilder
    {
        public static FoundationLambdaFunctionModel Build(IMethodSymbol lambdaMethodSymbol, IMethodSymbol configureMethodSymbol, GeneratorExecutionContext context)
        {

            var attribute = FoundationAttributeModelBuilder.Build(lambdaMethodSymbol.GetAttributes().Single(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)), context);

            var lambdaFunctionModel = LambdaFunctionModelBuilder.Build(lambdaMethodSymbol, configureMethodSymbol, context);
            AttributeModel[] attributes = new AttributeModel[lambdaFunctionModel.Attributes.Count+1];
            lambdaFunctionModel.Attributes.CopyTo(attributes, 0);
            var sqlBucket = string.Empty;
            var branch = string.Empty;
            if (lambdaMethodSymbol.GetAttributes().SingleOrDefault(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)) is {} att)
            {
                var built = MigrationFunctionAttributeBuilder.Build(att, context);
                sqlBucket = built.SqlBucket;
                branch = built.Branch;

            }
            return new FoundationLambdaFunctionModel(lambdaFunctionModel, sqlBucket, branch );
        }
    }
}