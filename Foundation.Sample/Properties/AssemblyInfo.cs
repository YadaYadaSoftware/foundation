using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Foundation.Annotations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Sample.Properties;

[assembly: MigrationFunction(migrationFunction: typeof(MigrationFunctions), migrationMethod: nameof(MigrationFunctions.MyMigrator), dependsOn: "Restore", migrationsAssemblyPath: "@MigrationsAssemblyPath" )]
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
