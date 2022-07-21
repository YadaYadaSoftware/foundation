using Amazon.Lambda.Core;
using Amazon.S3.Transfer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Foundation.Functions
{
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
            var environmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
            var host = Environment.GetEnvironmentVariable("Host");

            builder.AddJsonFile("appsettings.json", true, false)
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddSystemsManager("/db.deploy2the.cloud");

            var configurationRoot = builder.Build();

            services.Configure<SqlConnectionStringBuilder>(configurationRoot.GetSection(nameof(SqlConnectionStringBuilder)))
                .AddLogging(builder => builder.AddLoggerYadaYada(configurationRoot, nameof(LambdaLoggerOptions)))
                //.AddAutoPartsContext()
                .AddEntityFrameworkSqlServer()
                .AddEntityConfigurations()
                .AddAWSService<Amazon.S3.IAmazonS3>()
                .AddSingleton<ITransferUtility, TransferUtility>()
                .AddDbContext<DbContext, DbContext>((provider, optionsBuilder) => optionsBuilder.UseSqlServer(provider.GetRequiredService<IOptions<SqlConnectionStringBuilder>>().Value.ConnectionString));
        }
    }
}
