using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

public class BackupRestoreProperties : IBackupRestoreDatabaseInfo
{
    public string BackupBucket { get; set; }

    public string FromBackupFile { get; set; }

    public bool DropDatabase { get; set; }

    public bool BackupDatabase { get; set; }
    public string DatabaseName { get; set; }
}