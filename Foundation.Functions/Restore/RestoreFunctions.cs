using System.Data;
using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Data.Serverless.Backup;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using YadaYada.Bisque.Annotations;


namespace Data.Serverless.Restore;

public class RestoreFunctions : DatabaseFunctionBase
{

    public RestoreFunctions(IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, ILoggerProvider loggerProvider) : base(sqlConnectionStringBuildOptions, loggerProvider)
    {
    }

    [LambdaFunction(Role = "@LambdaExecutionRole", Timeout = 900, MemorySize = 10240)]
    public async Task<CloudFormationResponse> RestoreDatabase(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        try
        {
            switch (info.RequestType)
            {
                case CloudFormationRequest.RequestTypeEnum.Update:
                    return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context, physicalResourceId: info.FromBackupFile);

                case CloudFormationRequest.RequestTypeEnum.Create:
                    await this.CreateDatabaseAsync(info, context);
                    return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context, physicalResourceId: info.FromBackupFile);
                case CloudFormationRequest.RequestTypeEnum.Delete:
                {
                    using var cloudFormationClient = new AmazonCloudFormationClient();

                    var stack = (await cloudFormationClient.DescribeStacksAsync(new DescribeStacksRequest {StackName = info.StackId}, CancellationToken.None)).Stacks.SingleOrDefault();

                    if (stack.StackStatus.IsCleaningUp())
                    {
                        LambdaLogger.Log("Cleaning up, not deleting");
                        return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context);
                    }

                    if (info.BackupDatabase)
                    {
                        await this.BackupDatabaseAsync(info, context);
                    }
                    else
                    {
                        LambdaLogger.Log($"{this.GetType().FullName}:  Not backing up {info.BackupDatabase} == false");
                    }

                    if (info.DropDatabase)
                    {
                        await this.DeleteDatabase(info, context);
                    }
                    else
                    {
                        LambdaLogger.Log($"{this.GetType().FullName}:  Not dropping {info.DropDatabase} == false");
                    }

                    return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context);
                }
                default:
                    throw new NotSupportedException(info.ResourceType);
            }
        }
        catch (Exception e)
        {
            LambdaLogger.Log(e.ToString());
            return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, info, context, e.Message);
        }

    }

    private async Task DeleteDatabase(BackupRestoreDatabaseInfo backupRestoreDatabaseInfo, ILambdaContext context)
    {
        bool dropDatabase = backupRestoreDatabaseInfo.DropDatabase;
        if (!dropDatabase)
        {
            LambdaLogger.Log($"{this.GetType().FullName}:{nameof(dropDatabase)}={dropDatabase}");
            return;
        }

        var oldInitialCatalog = SqlConnectionStringBuilder.InitialCatalog;

        try
        {
            SqlConnectionStringBuilder.InitialCatalog = string.Empty;
            await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
            sqlConnection.Open();

            await using var dropCommand = sqlConnection.CreateCommand();

            dropCommand.CommandText = "msdb.dbo.rds_drop_database";
            dropCommand.CommandType = CommandType.StoredProcedure;
            dropCommand.Parameters.Add("db_name", SqlDbType.VarChar).Value = oldInitialCatalog;
            dropCommand.ExecuteNonQuery();

        }
        catch (SqlException ex)
        {
            LambdaLogger.Log($"{this.GetType().FullName}.{nameof(DeleteDatabase)}:{SqlConnectionStringBuilder.ConnectionString}{ex}");
            throw;
        }
        finally
        {
            SqlConnectionStringBuilder.InitialCatalog = oldInitialCatalog;
        }
    }
}

