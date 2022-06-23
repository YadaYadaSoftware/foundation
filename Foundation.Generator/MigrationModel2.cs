using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationModel2 : IMigrationModel2
{
    public string MigrationId { get; }
    public TypeModel DataMigrationFunction { get; }

    public MigrationModel2(string migrationId, TypeModel dataMigrationFunction)
    {
        MigrationId = migrationId;
        DataMigrationFunction = dataMigrationFunction;
    }
}