using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

internal interface IBackupRestoreDatabaseInfo
{
    string BackupBucket { get; }
    string FromBackupFile { get; set; }
    bool DropDatabase { get; set; }
    bool BackupDatabase { get; set; }
    string DatabaseName { get; set; }


}