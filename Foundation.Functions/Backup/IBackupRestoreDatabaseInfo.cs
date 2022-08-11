using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

internal interface IBackupRestoreDatabaseInfo
{
    CloudVariant BackupBucket { get; }
    CloudVariant FromBackupFile { get; set; }
    CloudVariant DropDatabase { get; set; }
    CloudVariant BackupDatabase { get; set; }

}