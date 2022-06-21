using Amazon.Lambda.Annotations.SourceGenerator.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.FileIO;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Writers;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class FoundationCloudFormationJsonWriter : IAnnotationReportWriter
{
    private readonly IFileManager _fileManager;
    private readonly IDirectoryManager _directoryManager;
    private readonly IJsonWriter _jsonWriter;
    private readonly IDiagnosticReporter _diagnosticReporter;
    public const string MetadataToolPropertyNameSuffix = ".Metadata.Tool";

    public FoundationCloudFormationJsonWriter(IFileManager fileManager, IDirectoryManager directoryManager, IJsonWriter jsonWriter, IDiagnosticReporter diagnosticReporter)
    {
        _fileManager = fileManager;
        _directoryManager = directoryManager;
        _jsonWriter = jsonWriter;
        _diagnosticReporter = diagnosticReporter;
    }


    public void ApplyReport(AnnotationReport report)
    {
        if (report is not FoundationAnnotationReport foundationAnnotationReport)
        {
            throw new NotSupportedException();
        }

        var originalContent = _fileManager.ReadAllText(foundationAnnotationReport.CloudFormationTemplatePath);
        var templateDirectory = _directoryManager.GetDirectoryName(foundationAnnotationReport.CloudFormationTemplatePath);
        var relativeProjectUri = _directoryManager.GetRelativePath(templateDirectory, foundationAnnotationReport.ProjectRootDirectory);

        if (string.IsNullOrEmpty(originalContent))
        {
            throw new NotSupportedException();
        }
        else
        {
            _jsonWriter.Parse(originalContent);
        }


        var processedLambdaFunctions = new HashSet<string>();

        ProcessMigrationFunction(foundationAnnotationReport, _jsonWriter);

        //RemoveOrphanedResources();

        var json = _jsonWriter.GetPrettyJson();
        _fileManager.WriteAllText(report.CloudFormationTemplatePath, json);

        _diagnosticReporter.Report(Diagnostic.Create(DiagnosticDescriptors.CodeGeneration, Location.None, $"{report.CloudFormationTemplatePath}", json));
    }

    private void ProcessMigrationFunction(FoundationAnnotationReport report, IJsonWriter jsonWriter)
    {
        var resourcePath = $"Resources.{report.MigrationFunctionModel.Name}";
        var metadataToolPath = $"{resourcePath}.Metadata.Foundation.Tool";
        jsonWriter.SetToken($"{metadataToolPath}", typeof(Generator).FullName);
    }

    private void RemoveOrphanedResources()
    {
        throw new NotImplementedException();
    }

}