using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

public class BackupRestoreProperties : IBackupRestoreDatabaseInfo
{
    public string BackupBucket { get; set; }

    public string FromBackupFile { get; set; }

    public string DropDatabase { get; set; }

    public string BackupDatabase { get; set; }
    public string DatabaseName { get; set; }
}