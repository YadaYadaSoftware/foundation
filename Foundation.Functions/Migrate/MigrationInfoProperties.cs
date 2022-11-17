namespace Foundation.Functions.Migrate;

public class MigrationInfoProperties : IMigrationInfo
{
    public string MigrationName { get; set; }
    public string MigrationsAssemblyPath { get; set; }
    public string StackName { get; set; }
    public string InitialCatalog { get; set; }
    public string BackupBucket { get; set; }
    public string BackupAfterApply { get; set; }
}