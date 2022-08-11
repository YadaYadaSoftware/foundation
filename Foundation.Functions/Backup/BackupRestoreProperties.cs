using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

public class BackupRestoreProperties : IBackupRestoreDatabaseInfo
{
    public CloudVariant BackupBucket { get; set; }

    public CloudVariant FromBackupFile { get; set; }

    public CloudVariant DropDatabase { get; set; }

    public CloudVariant BackupDatabase { get; set; }
}