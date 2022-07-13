using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public interface IMigrationFunctionAttributeModel
{
    TypeModel MigrationFunction { get; set; }
    string MigrationMethod { get; set; }
    string DependsOn { get; set; }

    // ATTRIBUTE:  ADD HERE
    public string MigrationsAssembly { get; set; }
    public string MigrationsAssemblyPath { get; set; }
}