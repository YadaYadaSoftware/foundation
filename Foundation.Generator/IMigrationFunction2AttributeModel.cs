using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public interface IMigrationFunction2AttributeModel
{
    TypeModel MigrationFunction { get; set; }
    string MigrationMethod { get; set; }

}