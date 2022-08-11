using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Amazon.Lambda.Core;
using Amazon.S3.Transfer;
using Data.Serverless.Backup;
using Data.Serverless.Migrate;
using Foundation.Functions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


[assembly: LambdaSerializer(typeof(DatabaseSerializationContext))]

namespace Foundation.Functions;

[JsonSerializable(typeof(BackupRestoreDatabaseInfo))]
[JsonSerializable(typeof(MigrationInfoRequest))]
public partial class DatabaseSerializationContext : System.Text.Json.Serialization.JsonSerializerContext
{
}

[Amazon.Lambda.Annotations.LambdaStartup]
public class Startup
{
    /// <summary>
    /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
    ///
    /// The services can be injected into the Lambda function through the containing type's constructor or as a
    /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
    /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
    /// of the function invocation.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        var builder = new ConfigurationBuilder();

        builder.AddJsonFile("appsettings.json", true, false)
            .AddSystemsManager("/db.deploy2the.cloud");

        var configurationRoot = builder.Build();

        services.Configure<SqlConnectionStringBuilder>(configurationRoot.GetSection(nameof(SqlConnectionStringBuilder)))
            .AddLogging(loggingBuilder => loggingBuilder.AddLoggerYadaYada(configurationRoot, nameof(LambdaLoggerOptions)))
            //.AddEntityFrameworkSqlServer()
            //.AddEntityConfigurations()
            .AddAWSService<Amazon.S3.IAmazonS3>()
            .AddSingleton<ITransferUtility, TransferUtility>()
            //.AddDbContext<DbContext, DbContext>((provider, optionsBuilder) => optionsBuilder.UseSqlServer(provider.GetRequiredService<IOptions<SqlConnectionStringBuilder>>().Value.ConnectionString))
            ;
    }
}