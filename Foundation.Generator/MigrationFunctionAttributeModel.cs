using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationFunctionAttributeModel : IMigrationFunctionAttributeModel
{
    public string MigrationMethod { get; set; }
    public string DependsOn { get; set; }
    public string SqlScriptBucket { get; set; }
    public TypeModel MigrationFunction { get; set; }
    public string Branch { get; set; }

    // ATTRIBUTE:  ADD HERE
    public string MigrationsAssemblyPath { get; set; }
}