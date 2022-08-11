using System.Data;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.RDS;
using Data.Serverless.Backup;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using YadaYada.Bisque.Annotations;

namespace Data.Serverless;

public abstract class DatabaseFunctionBase
{
    protected DatabaseFunctionBase(IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, ILoggerProvider loggerProvider)
    {
        SqlConnectionStringBuilder = sqlConnectionStringBuildOptions.Value;
        Logger = loggerProvider.CreateLogger(GetType().FullName!);
    }

    public ILogger Logger { get; }

    internal virtual IAmazonRDS GetAmazonRds()
    {
        return new AmazonRDSClient();
    }

    protected SqlConnectionStringBuilder SqlConnectionStringBuilder { get; }

    protected static bool IsTaskComplete(SqlConnection sqlConnection, int task)
    {
        try
        {
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "msdb.dbo.rds_task_status";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("task_id", SqlDbType.Int).Value = task;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.HasRows)
                {
                    var s = new StringBuilder();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        s.AppendLine($"{i}={reader[i]}");
                    }
                    //LambdaLogger.Log(s.ToString());

                    var status = reader.GetString(5);
                    return status == "SUCCESS";
                }
            }

            return false;

        }
        catch (Exception e)
        {
            //LambdaLogger.Log(e.ToString());
            throw;
        }

    }

    protected int GetTaskId(SqlConnection sqlConnection, string dbName)
    {
        try
        {
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "msdb.dbo.rds_task_status";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("db_name", SqlDbType.VarChar).Value = dbName;
            do
            {

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.HasRows)
                    {
                        var s = new StringBuilder();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            s.AppendLine($"{i}={reader[i]}");
                        }

                        //LambdaLogger.Log(s.ToString());

                        var status = reader.GetString(5);
                        var id = reader.GetInt32(0);
                        var db = reader.GetString(2);
                        if ((status == "CREATED" || status == "IN_PROGRESS") && db == dbName)
                        {
                            return id;
                        }
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            } while (true);

            throw new InvalidOperationException();

        }
        catch (Exception e)
        {
            //LambdaLogger.Log(e.ToString());
            throw;
        }
    }

    [Function(memorySize: 128, noFunction: true, doNotCreateCustomResource: true)]
    public virtual async Task<CloudFormationResponse> BackupDatabaseAsync(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        try
        {
            await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
            sqlConnection.Open();
            await using var command = sqlConnection.CreateCommand();
            command.CommandText = "msdb.dbo.rds_backup_database";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("source_db_name", SqlDbType.VarChar).Value = SqlConnectionStringBuilder.InitialCatalog;
            var datetime = DateTime.Now.ToString("u").Replace(':', '-').Replace('/', '-').Replace('+', '-').Replace('.', '-').Replace(' ', '-');
            command.Parameters.Add("s3_arn_to_backup_to", SqlDbType.VarChar).Value = $"{info.BackupBucket}/{SqlConnectionStringBuilder.InitialCatalog}{datetime}.bak";
            command.Parameters.Add("overwrite_S3_backup_file", SqlDbType.TinyInt).Value = 1;
            command.ExecuteNonQuery();

            var taskId = GetTaskId(sqlConnection, SqlConnectionStringBuilder.InitialCatalog);

            do
            {
                if (IsTaskComplete(sqlConnection, taskId))
                {
                    break;
                }
                await Task.Delay(TimeSpan.FromSeconds(15));
            } while (true);

        }
        catch (Exception e)
        {
            return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, info, context, e.Message);
        }

        return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context);



    }

    protected async Task CreateDatabaseAsync(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        SqlConnectionStringBuilder.InitialCatalog = info.DatabaseName;
        LambdaLogger.Log($"{nameof(CreateDatabaseAsync)}:{nameof(SqlConnectionStringBuilder.ConnectionString)}={SqlConnectionStringBuilder.ConnectionString}");
        await using (var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString))
        {

            try
            {

                sqlConnection.Open();
                // already present - exit
                return;

            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.ToString());
            }
        }


        // remove the catalog so we can connect to the server directly
        SqlConnectionStringBuilder.InitialCatalog = string.Empty;

        await using (var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString))
        {

            try
            {
                sqlConnection.Open();

            }
            catch (Exception e)
            {
                LambdaLogger.Log($"{GetType().FullName}.{nameof(CreateDatabaseAsync)}:{nameof(SqlConnectionStringBuilder)}='{SqlConnectionStringBuilder.ConnectionString}':{e}");
                throw;
            }
            await using var restoreCommand = sqlConnection.CreateCommand();

            restoreCommand.CommandText = "msdb.dbo.rds_restore_database";
            restoreCommand.CommandType = CommandType.StoredProcedure;
            restoreCommand.Parameters.Add("restore_db_name", SqlDbType.VarChar).Value = info.DatabaseName;
            restoreCommand.Parameters.Add("s3_arn_to_restore_from", SqlDbType.VarChar).Value = $"{info.BackupBucket}/{info.FromBackupFile}.bak";
            restoreCommand.ExecuteNonQuery();


            var taskId = GetTaskId(sqlConnection, info.DatabaseName);

            do
            {
                LambdaLogger.Log("Waiting for task to complete....");
                await Task.Delay(TimeSpan.FromSeconds(15));
                if (IsTaskComplete(sqlConnection, taskId))
                {
                    LambdaLogger.Log("Restored...");
                    break;
                }
            } while (true);

        }
        SqlConnectionStringBuilder.InitialCatalog = info.DatabaseName;

        do
        {
            await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
            try
            {
                LambdaLogger.Log("Connecting...");
                sqlConnection.Open();
                LambdaLogger.Log("Connected.");
                break;
            }
            catch
            {
                LambdaLogger.Log("Sleeping...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        } while (true);
    }
}