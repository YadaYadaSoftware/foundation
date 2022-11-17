using Amazon.Lambda.Core;
using Amazon.RDS.Model;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using YadaYada.Bisque.Annotations;

namespace Foundation.Functions.Snapshot
{
    public class SnapshotFunctions : DatabaseFunctionBase
    {
        public SnapshotFunctions([NotNull][ItemNotNull] IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, ILoggerProvider loggerProvider, DatabaseBackupStatus databaseBackupStatus) : base(sqlConnectionStringBuildOptions, loggerProvider, databaseBackupStatus)
        {
        }

        [Function(customResourceType:typeof(SnapshotCustomResource), noFunction:true, doNotCreateCustomResource:true)]
        public virtual async Task<CloudFormationResponse> TakeSnapshotAsync(SnapshotRequest snapshotInfo, ILambdaContext context)
        {
            if (snapshotInfo.RequestType==CloudFormationRequest.RequestTypeEnum.Delete || !snapshotInfo.ResourceProperties.Enabled)
            {
                return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, snapshotInfo, context);
            }

            try
            {
                var rds = GetAmazonRds();

                var createDbSnapshotRequest = new CreateDBSnapshotRequest
                {
                    DBInstanceIdentifier = snapshotInfo.ResourceProperties.DbInstanceId,
                    DBSnapshotIdentifier = snapshotInfo.ResourceProperties.SnapshotName
                };

                var delay = TimeSpan.FromSeconds(5);

                do
                {
                    var instance = (await rds.DescribeDBInstancesAsync(new DescribeDBInstancesRequest {DBInstanceIdentifier = snapshotInfo.ResourceProperties.DbInstanceId})).DBInstances.Single();
                    string dbInstanceStatus = instance.DBInstanceStatus;

                    if (dbInstanceStatus.Contains("AVAILABLE", StringComparison.InvariantCultureIgnoreCase))
                    {
                        do
                        {
                            try
                            {
                                LambdaLogger.Log($"{nameof(createDbSnapshotRequest)}.{nameof(createDbSnapshotRequest.DBInstanceIdentifier)}='{createDbSnapshotRequest.DBInstanceIdentifier}', {nameof(createDbSnapshotRequest)}.{nameof(createDbSnapshotRequest.DBSnapshotIdentifier)}='{createDbSnapshotRequest.DBSnapshotIdentifier}'");
                                var x = await rds.CreateDBSnapshotAsync(createDbSnapshotRequest);
                                break;
                            }
                            catch (DBSnapshotAlreadyExistsException exists)
                            {
                                LambdaLogger.Log(exists.Message);
                                createDbSnapshotRequest.DBSnapshotIdentifier += "x";
                            }
                        } while (context.RemainingTime>TimeSpan.FromMinutes(1));
                        return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, snapshotInfo, context);

                    }

                    await Task.Delay(delay);
                    LambdaLogger.Log($"{nameof(dbInstanceStatus)}={dbInstanceStatus}");

                } while (context.RemainingTime > delay * 2);

                return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, snapshotInfo, context, $"Timed Out await for {snapshotInfo.ResourceProperties.DbInstanceId} to be 'AVAILABLE'.");


            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.ToString());
                return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, snapshotInfo, context, e.Message);
            }
        }

    }
}
