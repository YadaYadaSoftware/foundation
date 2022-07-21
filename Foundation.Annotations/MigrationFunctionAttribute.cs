
using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MigrationFunctionAttribute : Attribute
    {
        public MigrationFunctionAttribute(Type migrationFunction, string migrationMethod, string dependsOn, string migrationsAssemblyPath, string initialCatalog)
        {
            MigrationFunction = migrationFunction;
            MigrationMethod = migrationMethod;
            DependsOn = dependsOn;
            MigrationsAssemblyPath = migrationsAssemblyPath;
            InitialCatalog = initialCatalog;
        }

        public MigrationFunctionAttribute(string dependsOn, string migrationsAssemblyPath, string migrationFunctionArn, string initialCatalog)
        {
            DependsOn = dependsOn;
            MigrationsAssemblyPath = migrationsAssemblyPath;
            MigrationFunctionArn = migrationFunctionArn;
            InitialCatalog = initialCatalog;
        }

        public Type MigrationFunction { get; }

        public string MigrationMethod { get; }
        public string DependsOn { get;  }
        // ATTRIBUTE:  ADD HERE
        public string MigrationsAssemblyPath { get; }

        public string MigrationFunctionArn { get; }
        public string InitialCatalog { get; }
    }
}