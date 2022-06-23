using Amazon.Lambda.Annotations;
using Amazon.Lambda.Core;
using Foundation.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.Migrations
{
    [Migration("migration1")]
    [MigrationFunction2(MigrationFunction = typeof(MigrationFunctions), MigrationMethod = nameof(MigrationFunctions.MyMigrator))]
    public class Migration1 
    {
    }

    public class MigrationFunctions
    {
        [LambdaFunction]
        public Task MyMigrator(ILambdaContext context)
        {
            return Task.CompletedTask;
        }

    }
}
