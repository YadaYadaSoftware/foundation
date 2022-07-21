using System.Text.Json.Serialization;
using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Migrate;

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
}