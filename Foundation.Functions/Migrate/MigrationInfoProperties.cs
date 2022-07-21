namespace Data.Serverless.Migrate;

public class MigrationInfoProperties : IMigrationInfo
{
    public string MigrationName { get; set; }
    public string MigrationsAssemblyPath { get; set; }
    public string StackName { get; set; }
}