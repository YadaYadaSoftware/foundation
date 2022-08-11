using Data.Serverless.Backup;
using Newtonsoft.Json;
using YadaYada.Bisque.Annotations;
using YadaYada.Bisque.Aws.CloudFormation;

namespace Data.Serverless.Restore;

public class RestoreDatabaseCustomResource : CustomResource, IBackupRestoreDatabaseInfo
{
    public RestoreDatabaseCustomResource(string key) : base(key)
    {
                
    }

    [JsonProperty(PropertyName = "Properties." + nameof(BackupBucket))]
    public CloudVariant BackupBucket { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(FromBackupFile))]
    public CloudVariant FromBackupFile { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(DropDatabase))]
    public CloudVariant DropDatabase { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(BackupDatabase))]
    public CloudVariant BackupDatabase { get; set; }
}