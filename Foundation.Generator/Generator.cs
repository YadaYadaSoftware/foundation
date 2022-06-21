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

                var semanticModelProvider = new SemanticModelProvider(context);
                var lambdaMethodModel = semanticModelProvider.GetMethodSemanticModel(receiver.MigrationFunction);
                var model = LambdaFunctionModelBuilder.Build(lambdaMethodModel, null, context);


                var annotationReport = new FoundationAnnotationReport();

                var templateFinder = new CloudFormationTemplateFinder(_fileManager, _directoryManager);
                var projectRootDirectory = templateFinder.DetermineProjectRootDirectory(receiver.MigrationFunction.SyntaxTree.FilePath);

                annotationReport.CloudFormationTemplatePath = templateFinder.FindCloudFormationTemplate(projectRootDirectory);
                annotationReport.ProjectRootDirectory = projectRootDirectory;
                annotationReport.MigrationFunctionModel = model;
                annotationReport.MigrationClasses = receiver.MigrationClasses;
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
}