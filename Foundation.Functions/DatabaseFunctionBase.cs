﻿using System.Data;
using System.Text;
using Amazon.Lambda.Core;
using Amazon.RDS;
using Foundation.Functions.Backup;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using YadaYada.Bisque.Annotations;

namespace Foundation.Functions;

public abstract class DatabaseFunctionBase
{
    private readonly DatabaseBackupStatus _databaseBackupStatus;

    protected DatabaseFunctionBase(IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, ILoggerProvider loggerProvider, DatabaseBackupStatus databaseBackupStatus)
    {
        _databaseBackupStatus = databaseBackupStatus;
        SqlConnectionStringBuilder = sqlConnectionStringBuildOptions.Value;
        Logger = loggerProvider.CreateLogger(GetType().FullName!);
    }

    public ILogger Logger { get; }

    internal virtual IAmazonRDS GetAmazonRds()
    {
        return new AmazonRDSClient();
    }

    protected SqlConnectionStringBuilder SqlConnectionStringBuilder { get; }

    protected bool IsTaskComplete(SqlConnection sqlConnection, int task)
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

                var status = reader.GetString(5);

                Logger.LogInformation("{0}={1}", nameof(status), status);

                switch (status)
                {
                    case "SUCCESS":
                        return true;
                    case "ERROR":
                        var error = reader.GetString(6);
                        throw new InvalidOperationException(error);
                    default:
                        return false;
                }
            }
        }

        return false;

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
            var datetime = DateTime.Now.ToString("u").Replace(':', '-').Replace('/', '-').Replace('+', '-').Replace('.', '-').Replace(' ', '-');
            await BackupDatabaseImplementationAsync(info.BackupBucket,$"{SqlConnectionStringBuilder.InitialCatalog}{datetime}");
        }
        catch (Exception e)
        {
            return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, info, context, e.Message);
        }

        return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context);



    }

    internal async Task BackupDatabaseImplementationAsync(string backupBucket, string filename)
    {
        using (Logger.AddMember(nameof(BackupDatabaseImplementationAsync)))
        {

            SqlCommand? command = null;

            try
            {
                await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
                sqlConnection.Open();
                command = sqlConnection.CreateCommand();
                command.CommandText = "msdb.dbo.rds_backup_database";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("source_db_name", SqlDbType.VarChar).Value = SqlConnectionStringBuilder.InitialCatalog;
                command.Parameters.Add("s3_arn_to_backup_to", SqlDbType.VarChar).Value = $"{backupBucket}/{filename}.bak";
                command.Parameters.Add("overwrite_S3_backup_file", SqlDbType.TinyInt).Value = 1;

                while (_databaseBackupStatus.IsBusy())
                {
                    Logger.LogInformation("Waiting....");
                    await Task.Delay(TimeSpan.FromSeconds(15));
                }

                command.ExecuteNonQuery();

            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                Logger.LogTrace($"{{{nameof(command.CommandText)}}}={0}", command?.CommandText);
                Logger.LogTrace($"{{{nameof(command.CommandType)}}}={0}", command?.CommandType);
                foreach (SqlParameter commandParameter in command.Parameters)
                {
                    Logger.LogTrace($"{{{nameof(commandParameter.ParameterName)}}}={0},{{{nameof(commandParameter.Value)}}}={1}", commandParameter.ParameterName, commandParameter.Value);
                        
                }
                throw;
            }
            finally
            {
                command?.Dispose();
            }
        }
    }

    private async Task<bool> CheckDatabaseAlreadyExistsAsync(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
        await sqlConnection.OpenAsync();

        try
        {
            var command = sqlConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM [sys].[databases] WHERE [name] = @name";
            command.Parameters.AddWithValue("@name", info.DatabaseName);

            var scalarAsync = await command.ExecuteScalarAsync();

            if (scalarAsync is not { } || !int.TryParse(scalarAsync.ToString(), out var count))
            {
                throw new InvalidOperationException();
            }

            return count == 1;

        }
        catch (Exception e)
        {
            Logger.LogError(e, e.ToString());
            throw;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }

    protected async Task CreateDatabaseAsync(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        LambdaLogger.Log($"{nameof(CreateDatabaseAsync)}:{nameof(SqlConnectionStringBuilder.ConnectionString)}={SqlConnectionStringBuilder.ConnectionString}");

        if (await CheckDatabaseAlreadyExistsAsync(info, context)) return;

        var databaseToRestoreTo = info.DatabaseName;
        bool waitForRestore = true;

        if (await RestoreFromWarmAsync(info, context))
        {
            databaseToRestoreTo = info.FromBackupFile;
            waitForRestore = false;
        }


        await using (var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString))
        {


            await sqlConnection.OpenAsync();

            try
            {

                await using var restoreCommand = sqlConnection.CreateCommand();

                restoreCommand.CommandText = "msdb.dbo.rds_restore_database";
                restoreCommand.CommandType = CommandType.StoredProcedure;
                restoreCommand.Parameters.Add("restore_db_name", SqlDbType.VarChar).Value = databaseToRestoreTo;
                restoreCommand.Parameters.Add("s3_arn_to_restore_from", SqlDbType.VarChar).Value = $"{info.BackupBucket}/{info.FromBackupFile}.bak";
                restoreCommand.ExecuteNonQuery();

                if (!waitForRestore) return;


                var taskId = GetTaskId(sqlConnection, databaseToRestoreTo);

                do
                {
                    LambdaLogger.Log("Waiting for task to complete....");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    if (IsTaskComplete(sqlConnection, taskId))
                    {
                        LambdaLogger.Log("Restored...");
                        break;
                    }
                } while (true);

            }
            catch (Exception e)
            {
                LambdaLogger.Log($"{GetType().FullName}.{nameof(CreateDatabaseAsync)}:{nameof(SqlConnectionStringBuilder)}='{SqlConnectionStringBuilder.ConnectionString}':{e}");
                throw;
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }

        }

        //SqlConnectionStringBuilder.InitialCatalog = databaseToRestoreTo;

        //do
        //{
        //    await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
        //    try
        //    {
        //        LambdaLogger.Log("Connecting...");
        //        await sqlConnection.OpenAsync();
        //        LambdaLogger.Log("Connected.");
        //        break;
        //    }
        //    catch
        //    {
        //        LambdaLogger.Log("Sleeping...");
        //        await Task.Delay(TimeSpan.FromSeconds(5));
        //    }
        //    finally
        //    {
        //        await sqlConnection.CloseAsync();
        //    }
        //} while (true);
    }

    private async Task<bool> RestoreFromWarmAsync(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        await using var sqlConnection = new SqlConnection(SqlConnectionStringBuilder.ConnectionString);
        await sqlConnection.OpenAsync();

        try
        {
            var command = sqlConnection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM [sys].[databases] WHERE [name] = @name";
            command.Parameters.AddWithValue("@name", info.FromBackupFile);

            var scalarAsync = await command.ExecuteScalarAsync();

            if (scalarAsync is not { } || !int.TryParse(scalarAsync.ToString(), out var count))
            {
                throw new InvalidOperationException();
            }

            if (count != 1) return false;

            try
            {

                await using var renameCommand = sqlConnection.CreateCommand();

                renameCommand.CommandText = "rdsadmin.dbo.rds_modify_db_name";
                renameCommand.CommandType = CommandType.StoredProcedure;
                renameCommand.Parameters.Add("@old_db_name", SqlDbType.VarChar).Value = info.FromBackupFile;
                renameCommand.Parameters.Add("@new_db_name", SqlDbType.VarChar).Value = info.DatabaseName;
                renameCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                LambdaLogger.Log($"{GetType().FullName}.{nameof(CreateDatabaseAsync)}:{nameof(SqlConnectionStringBuilder)}='{SqlConnectionStringBuilder.ConnectionString}':{e}");
                throw;
            }
            finally
            {
                await sqlConnection.CloseAsync();
            }


            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, e.ToString());
            throw;
        }
        finally
        {
            await sqlConnection.CloseAsync();
        }
    }
}