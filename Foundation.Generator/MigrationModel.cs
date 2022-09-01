using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Foundation.Annotations;

namespace Foundation.Generators;

public class MigrationModel : IMigrationModel
{
    public string MigrationId { get; }
    public TypeModel MigrationFunction { get; }
    public string MigrationMethod { get; }
    public string DependsOn { get; }
    public string MigrationsAssemblyPath { get; set; }
    public string MigrationsFunctionArn { get; set; }
    public string InitialCatalog { get; set; }
    public string BackupBucket { get; set; }

    public MigrationModel(string migrationId, TypeModel dataMigrationFunction, string migrationMethod, string dependsOn, string migrationsAssemblyPath, string migrationsFunctionArn, string initialCatalog, string backupBucket)
    {
        if (string.IsNullOrEmpty(migrationsFunctionArn))
        {
            if (string.IsNullOrEmpty(migrationMethod) || dataMigrationFunction == default)
            {
                throw new InvalidOperationException($"Must specify {nameof(MigrationFunctionAttribute.MigrationFunctionArn)} or ( {nameof(MigrationFunctionAttribute.MigrationMethod)} and {nameof(MigrationFunctionAttribute.MigrationFunction)}");

            }
        }
        if (string.IsNullOrEmpty(migrationId)) throw new ArgumentException("Value cannot be null or empty.", nameof(migrationId));
        MigrationId = migrationId;
        MigrationFunction = dataMigrationFunction;
        MigrationMethod = migrationMethod;
        DependsOn = dependsOn;
        // ATTRIBUTE:  ADD HERE
        MigrationsAssemblyPath = migrationsAssemblyPath;
        MigrationsFunctionArn = migrationsFunctionArn;
        InitialCatalog = initialCatalog;
        BackupBucket = backupBucket;
    }
}