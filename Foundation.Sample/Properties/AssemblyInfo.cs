using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Foundation.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Sample.Properties;

[assembly: MigrationFunction(typeof(MigrationFunctions), nameof(MigrationFunctions.MyMigrator), "Restore", "@MigrationsAssemblyPath" )]
namespace Sample.Properties
{
    public class MigrationFunctions
    {
        [LambdaFunction]
        public Task MyMigrator(ILambdaContext context)
        {
            return Task.CompletedTask;
        }

    }
}
