using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationFunction2AttributeModel : IMigrationFunction2AttributeModel
{
    public string MigrationMethod { get; set; }
    public TypeModel MigrationFunction { get; set; }
}