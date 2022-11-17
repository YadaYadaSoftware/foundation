namespace Foundation.Functions.Migrate;

public interface IMigrationInfo
{
    string MigrationName { get; set; }
    string MigrationsAssemblyPath { get; set; }
    string StackName { get; set; }
    string InitialCatalog { get; set; }
    string BackupBucket { get; set; }
    string BackupAfterApply { get; set; }
}