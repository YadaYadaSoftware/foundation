using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

internal interface IBackupRestoreDatabaseInfo
{
    string BackupBucket { get; }
    string FromBackupFile { get; set; }
    string DropDatabase { get; set; }
    string BackupDatabase { get; set; }

    string DatabaseName { get; set; }

}