using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationFunctionAttributeModel : IMigrationFunctionAttributeModel
{
    public string MigrationMethod { get; set; }
    public string DependsOn { get; set; }
    public TypeModel MigrationFunction { get; set; }
}