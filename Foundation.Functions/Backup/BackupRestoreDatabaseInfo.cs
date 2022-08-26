using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

public class BackupRestoreDatabaseInfo : CloudFormationRequest<BackupRestoreProperties>, IBackupRestoreDatabaseInfo
{


    public string BackupBucket => this.ResourceProperties.BackupBucket;
    public string FromBackupFile { get => this.ResourceProperties.FromBackupFile; set=> this.ResourceProperties.FromBackupFile = value; }
    public bool DropDatabase { get => this.ResourceProperties.DropDatabase; set => this.ResourceProperties.DropDatabase = value; }
    public bool BackupDatabase { get => this.ResourceProperties.BackupDatabase; set => this.ResourceProperties.BackupDatabase = value; }
    public string DatabaseName { get => this.ResourceProperties.DatabaseName; set => this.ResourceProperties.DatabaseName = value; }
}