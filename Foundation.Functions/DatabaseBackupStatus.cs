using System.Data;
using System.Text;
using Amazon.S3.Transfer;
using Data.Serverless.Backup;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Foundation.Functions;

public class DatabaseBackupStatus
{
        
    private readonly SqlConnectionStringBuilder _sqlConnectionStringBuilder;
    private readonly ILogger _logger;

    public DatabaseBackupStatus(IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, ILoggerProvider loggerProvider)
    {
        _sqlConnectionStringBuilder = sqlConnectionStringBuildOptions.Value;
        _logger = loggerProvider.CreateLogger(typeof(DatabaseBackupStatus).FullName);
    }

    internal bool IsBusy()
    {
        using (_logger.BeginScope(nameof(IsBusy)))
        {
            using var sqlConnection = new SqlConnection(_sqlConnectionStringBuilder.ConnectionString);
            sqlConnection.Open();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "msdb.dbo.rds_task_status";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("db_name", SqlDbType.VarChar).Value = _sqlConnectionStringBuilder.InitialCatalog;
            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                _logger.LogInformation("No Read"); 
                return false;
            }

            if (!reader.HasRows)
            {
                _logger.LogInformation("No HasRows");
                return false;
            }

            var status = reader.GetString(5);

            _logger.LogInformation("{0}={1}",nameof(status), status);

            bool returnValue;

            switch (status)
            {
                case "CREATED":
                case "IN_PROGRESS":
                case "CANCEL_REQUESTED":
                    returnValue = true;
                    break;
                case "SUCCESS":
                case "ERROR":
                case "CANCELLED":
                    returnValue = false;
                    break;
                default:
                    throw new NotSupportedException(status);
            }

            _logger.LogInformation("{0}={1}", nameof(returnValue), returnValue);
            return returnValue;


        }
    }
}