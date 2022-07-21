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

    public MigrationModel(string migrationId, TypeModel dataMigrationFunction, string migrationMethod, string dependsOn, string migrationsAssemblyPath, string migrationsFunctionArn)
    {
        if (string.IsNullOrEmpty(migrationMethod) && string.IsNullOrEmpty(migrationsFunctionArn))
        {
            throw new InvalidOperationException($"Must specify {nameof(MigrationFunctionAttribute.MigrationFunctionArn)} or {nameof(MigrationFunctionAttribute.MigrationMethod)}");
        }
        if (string.IsNullOrEmpty(migrationId)) throw new ArgumentException("Value cannot be null or empty.", nameof(migrationId));
        MigrationId = migrationId;
        MigrationFunction = dataMigrationFunction ?? throw new ArgumentNullException(nameof(dataMigrationFunction));
        MigrationMethod = migrationMethod;
        DependsOn = dependsOn;
        // ATTRIBUTE:  ADD HERE
        MigrationsAssemblyPath = migrationsAssemblyPath;
        MigrationsFunctionArn = migrationsFunctionArn;
    }
}