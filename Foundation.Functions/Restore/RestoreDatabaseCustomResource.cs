using Foundation.Functions.Backup;
using Newtonsoft.Json;
using YadaYada.Bisque.Aws.CloudFormation;

namespace Foundation.Functions.Restore;

public class RestoreDatabaseCustomResource : CustomResource, IBackupRestoreDatabaseInfo
{
    public RestoreDatabaseCustomResource(string key) : base(key)
    {
                
    }

    [JsonProperty(PropertyName = "Properties." + nameof(BackupBucket))]
    public string BackupBucket { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(FromBackupFile))]
    public string FromBackupFile { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(DropDatabase))]
    public string DropDatabase { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(BackupDatabase))]
    public string BackupDatabase { get; set; }

    [JsonProperty(PropertyName = "Properties." + nameof(DatabaseName))]
    public string DatabaseName { get; set; }
}