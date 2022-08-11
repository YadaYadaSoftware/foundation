using Amazon.Lambda.Core;
using JetBrains.Annotations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using YadaYada.Bisque.Annotations;

namespace Data.Serverless.Backup;

public class BackupFunctions : DatabaseFunctionBase
{
    public BackupFunctions([NotNull][ItemNotNull] IOptions<SqlConnectionStringBuilder> sqlConnectionStringBuildOptions, ILoggerProvider loggerProvider) : base(sqlConnectionStringBuildOptions, loggerProvider)
    {
    }

    public override async Task<CloudFormationResponse> BackupDatabaseAsync(BackupRestoreDatabaseInfo info, ILambdaContext context)
    {
        if (info.RequestType == CloudFormationRequest.RequestTypeEnum.Delete)
        {
            try
            {
                await base.BackupDatabaseAsync(info, context);
            }
            catch (Exception e)
            {
                //LambdaLogger.Log(e.ToString());
                return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Failed, info, context, e.Message);
            }

        }
        return await CloudFormationResponse.CompleteCloudFormationResponse(CloudFormationResponse.StatusEnum.Success, info, context);
    }

}