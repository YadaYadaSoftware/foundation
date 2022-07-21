using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public interface IMigrationFunctionAttributeModel
{
    TypeModel MigrationFunction { get; set; }
    string MigrationMethod { get; set; }
    string DependsOn { get; set; }

    // ATTRIBUTE:  ADD HERE
    public string MigrationsAssemblyPath { get; set; }

    public string MigrationsFunctionArn { get; set; }
}