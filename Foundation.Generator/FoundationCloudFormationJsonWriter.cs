using Amazon.Lambda.Annotations.SourceGenerator.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.FileIO;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;

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

        if (string.IsNullOrEmpty(originalContent))
        {
            throw new NotSupportedException();
        }
        else
        {
            _jsonWriter.Parse(originalContent);
        }


        var migrationFunctionResourceName = ProcessMigrationFunction(foundationAnnotationReport, _jsonWriter);
        var processedMigrations = ProcessMigrations(foundationAnnotationReport, _jsonWriter);

        var json = _jsonWriter.GetPrettyJson();
        _fileManager.WriteAllText(report.CloudFormationTemplatePath, json);

        _diagnosticReporter.Report(Diagnostic.Create(DiagnosticDescriptors.CodeGeneration, Location.None, $"{report.CloudFormationTemplatePath}", json));
    }

    private List<string> ProcessMigrations(FoundationAnnotationReport foundationAnnotationReport, IJsonWriter jsonWriter)
    {
        var processedMigrations = new List<string>();
        foreach (var migrationModel in foundationAnnotationReport.Migrations)
        {
            string migrationResourceName = ProcessMigration(migrationModel, jsonWriter);
            processedMigrations.Add(migrationResourceName);
        }

        return processedMigrations;
    }

    private string ProcessMigration(IMigrationModel migrationModel, IJsonWriter jsonWriter)
    {
        var resourceName = migrationModel.Name;
        var customResourcePath = $"Resources.{resourceName}";
        var typePath = $"{customResourcePath}.Type";
        jsonWriter.SetToken(typePath, "AWS::CloudFormation::CustomResource");
        var metadataRootPath = GetMetadataPath(customResourcePath);
        var versionPath = $"{metadataRootPath}.Version";
        jsonWriter.SetToken(versionPath, typeof(Generator).Assembly.GetName().Version.ToString());
        jsonWriter.SetToken($"{metadataRootPath}.MigrationType", migrationModel.Namespace + "." + migrationModel.Name);

        var propertiesPath = $"{customResourcePath}.Properties";

        jsonWriter.SetToken($"{propertiesPath}.StackName", new JObject(new JProperty("Ref","AWS::StackName")));
        jsonWriter.SetToken($"{propertiesPath}.ServiceToken", new JObject(new JProperty("Fn::GetAtt", new JArray(migrationModel.FunctionResourceName, "Arn"))));
        jsonWriter.SetToken($"{propertiesPath}.MigrationName", migrationModel.Id);
        jsonWriter.SetToken($"{propertiesPath}.SqlBucket", GetValueOrRef(migrationModel.SqlBucket));
        jsonWriter.SetToken($"{propertiesPath}.Branch", GetValueOrRef(migrationModel.Branch));


        /*
      "Properties": {
        "ServiceToken": {
          "Fn::GetAtt": [
            "ApplyMigration",
            "Arn"
          ]
        },
        "StackName": {
          "Ref": "AWS::StackName"
        },
        "MigrationName": "20220513195220_MigrationJobDiscriminator",
        "SqlBucket": {
          "Ref": "PipelineBucket"
        },
        "Branch": {
          "Ref": "Branch"
        }
      }
      */

        return resourceName;

    }

    private string ProcessMigrationFunction(FoundationAnnotationReport report, IJsonWriter jsonWriter)
    {
        var resourceName = report.MigrationFunctionModel.Name;
        var resourcePath = $"Resources.{resourceName}";
        var metadataRootPath = GetMetadataPath(resourcePath);
        var versionPath = $"{metadataRootPath}.Version";
        jsonWriter.SetToken(versionPath, typeof(Generator).Assembly.GetName().Version.ToString());
        return resourceName;
    }

    private static string GetMetadataPath(string resourcePath)
    {
        var fullName = typeof(Generator).FullName.Replace(".", string.Empty);
        var metadataRootPath = $"{resourcePath}.Metadata.{fullName}";
        return metadataRootPath;
    }

    private void RemoveOrphanedResources()
    {
        throw new NotImplementedException();
    }

    private JToken GetValueOrRef(string value)
    {
        if (!value.StartsWith("@"))
            return value;

        var refNode = new JObject();
        refNode["Ref"] = value.Substring(1);
        return refNode;
    }


}