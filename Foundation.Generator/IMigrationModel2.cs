using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public interface IMigrationModel2
{
    public string MigrationId { get; }
    public TypeModel DataMigrationFunction { get; }
}