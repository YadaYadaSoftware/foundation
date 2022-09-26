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
    private readonly IJsonWriter _jsonWriter;
    private readonly IDiagnosticReporter _diagnosticReporter;

    public FoundationCloudFormationJsonWriter(IFileManager fileManager, IDirectoryManager directoryManager, IJsonWriter jsonWriter, IDiagnosticReporter diagnosticReporter)
    {
        _fileManager = fileManager;
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


        var processedMigrations = ProcessMigrations(foundationAnnotationReport, _jsonWriter);

        var json = _jsonWriter.GetPrettyJson();
        _fileManager.WriteAllText(report.CloudFormationTemplatePath, json);

        _diagnosticReporter.Report(Diagnostic.Create(DiagnosticDescriptors.CodeGeneration, Location.None, $"{report.CloudFormationTemplatePath}", json));
    }

    private List<string> ProcessMigrations(FoundationAnnotationReport foundationAnnotationReport, IJsonWriter jsonWriter)
    {
        var processedMigrations = new List<string>();
        var migrationsToProcess = foundationAnnotationReport.Migrations.ToList();
        migrationsToProcess.Sort(new MigrationSorter());
        string lastMigration = string.Empty;
        for (var i = 0; i < migrationsToProcess.Count; i++)
        {
            IMigrationModel? migrationModel = migrationsToProcess[i];
            migrationModel.BackupAfterApply = i == migrationsToProcess.Count - 1;
            lastMigration = ProcessMigration(migrationModel, jsonWriter, lastMigration);
            processedMigrations.Add(lastMigration);
        }

        return processedMigrations;
    }

    private string ProcessMigration(IMigrationModel migrationModel, IJsonWriter jsonWriter, string lastMigration)
    {
        var resourceName = $"Migration{migrationModel.MigrationId}".Replace("_", string.Empty);
        var customResourcePath = $"Resources.{resourceName}";
        var typePath = $"{customResourcePath}.Type";
        jsonWriter.SetToken(typePath, "AWS::CloudFormation::CustomResource");
        var metadataRootPath = GetMetadataPath(customResourcePath);
        var versionPath = $"{metadataRootPath}.Version";
        jsonWriter.SetToken(versionPath, typeof(Generator).Assembly.GetName().Version.ToString());

        var propertiesPath = $"{customResourcePath}.Properties";

        if (!string.IsNullOrEmpty(migrationModel.DependsOn))
        {
            var dependsOnPath = $"{customResourcePath}.DependsOn";
            object[] dependsOnValues = migrationModel.DependsOn.Split(',');
            if (!string.IsNullOrEmpty(lastMigration))
            {
                var redoDependsOn = new List<object>(dependsOnValues);
                redoDependsOn.Add(lastMigration);
                dependsOnValues = redoDependsOn.ToArray();
            }
            var dependsOnArray = new JArray(dependsOnValues);
            jsonWriter.SetToken(dependsOnPath,dependsOnArray);
        }


        if (!string.IsNullOrEmpty(migrationModel.MigrationMethod))
        {
            var functionResource = migrationModel.MigrationFunction.FullName.Replace(".", string.Empty) + migrationModel.MigrationMethod + "Generated";
            jsonWriter.SetToken($"{propertiesPath}.ServiceToken", new JObject(new JProperty("Fn::GetAtt", new JArray(functionResource, "Arn"))));
        }
        else
        {
            jsonWriter.SetToken($"{propertiesPath}.ServiceToken", migrationModel.MigrationsFunctionArn);
        }
        jsonWriter.SetToken($"{propertiesPath}.MigrationName", migrationModel.MigrationId);

        var migrationsAssemblyPathPath = $"{propertiesPath}.MigrationsAssemblyPath";
        if (!string.IsNullOrEmpty(migrationModel.MigrationsAssemblyPath))
        {
            jsonWriter.SetToken(migrationsAssemblyPathPath, GetValueOrRef(migrationModel.MigrationsAssemblyPath));
        }
        else
        {
            jsonWriter.RemoveToken(migrationsAssemblyPathPath);
        }

        jsonWriter.SetToken($"{propertiesPath}.StackName", new JObject(new JProperty("Ref", "AWS::StackName")));
        jsonWriter.SetToken($"{propertiesPath}.InitialCatalog", GetValueOrRef(migrationModel.InitialCatalog));
        jsonWriter.SetToken($"{propertiesPath}.BackupBucket", GetValueOrRef(migrationModel.BackupBucket));
        jsonWriter.SetToken($"{propertiesPath}.BackupAfterApply", GetValueOrRef(migrationModel.BackupAfterApply.ToString()));
        // ATTRIBUTE:  ADD HERE
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
        if (string.IsNullOrEmpty(value) || !value.StartsWith("@"))
            return value;

        var refNode = new JObject();
        refNode["Ref"] = value.Substring(1);
        return refNode;
    }


}

internal class MigrationSorter : IComparer<IMigrationModel>
{
    public int Compare(IMigrationModel x, IMigrationModel y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (ReferenceEquals(null, y)) return 1;
        if (ReferenceEquals(null, x)) return -1;
        return string.Compare(x.MigrationId, y.MigrationId, StringComparison.Ordinal);
    }
}