using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

public class BackupRestoreDatabaseInfo : CloudFormationRequest<BackupRestoreProperties>, IBackupRestoreDatabaseInfo
{


    public CloudVariant BackupBucket => this.ResourceProperties.BackupBucket;
    public CloudVariant FromBackupFile { get => this.ResourceProperties.FromBackupFile; set=> this.ResourceProperties.FromBackupFile = value; }
    public CloudVariant DropDatabase { get => this.ResourceProperties.DropDatabase; set => this.ResourceProperties.DropDatabase = value; }
    public CloudVariant BackupDatabase { get => this.ResourceProperties.BackupDatabase; set => this.ResourceProperties.BackupDatabase = value; }
}