using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Foundation.Annotations;
using Sample.Properties;

[assembly: MigrationFunction(MigrationFunction = typeof(MigrationFunctions), MigrationMethod = nameof(MigrationFunctions.MyMigrator), Branch = "@Branch", DependsOn = "Restore", MigrationsAssemblyPath = "@X", MigrationsAssembly = "X")]
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
