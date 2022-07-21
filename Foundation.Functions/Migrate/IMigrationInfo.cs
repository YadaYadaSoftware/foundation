namespace Data.Serverless.Migrate;

public interface IMigrationInfo
{
    string MigrationName { get; set; }
    string MigrationsAssemblyPath { get; set; }
    string StackName { get; set; }
}