using System.Text.Json.Serialization;
using YadaYada.Bisque.Annotations;

namespace Foundation.Functions.Migrate;

public class MigrationInfoRequest : CloudFormationRequest<MigrationInfoProperties>, IMigrationInfo
{

    [JsonIgnore]
    public string MigrationName
    {
        get => ResourceProperties.MigrationName;
        set => ResourceProperties.MigrationName = value;
    }

    [JsonIgnore]
    public string MigrationsAssemblyPath
    {
        get => ResourceProperties.MigrationsAssemblyPath;
        set => ResourceProperties.MigrationsAssemblyPath = value;
    }

    [JsonIgnore]
    public string StackName
    {
        get => ResourceProperties.StackName;
        set => ResourceProperties.StackName = value;
    }

    [JsonIgnore]
    public string InitialCatalog
    {
        get => ResourceProperties.InitialCatalog;
        set => ResourceProperties.InitialCatalog = value;
    }

    public string BackupBucket
    {
        get => ResourceProperties.BackupBucket;
        set => ResourceProperties.BackupBucket = value;
    }

    public string BackupAfterApply
    {
        get => ResourceProperties.BackupAfterApply;
        set => ResourceProperties.BackupAfterApply = value;
    }

}