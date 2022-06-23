using Foundation.Annotations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.Migrations
{
    [Migration("migration1")]
    [MigrationFunction2(MigrationFunction = typeof(MigrationFunctions))]
    public class Migration1 
    {
    }

    public class MigrationFunctions
    {

    }
}
