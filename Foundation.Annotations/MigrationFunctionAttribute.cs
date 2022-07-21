
using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MigrationFunctionAttribute : Attribute
    {
        public MigrationFunctionAttribute(Type migrationFunction, string migrationMethod, string dependsOn, string migrationsAssemblyPath)
        {
            MigrationFunction = migrationFunction;
            MigrationMethod = migrationMethod;
            DependsOn = dependsOn;
            MigrationsAssemblyPath = migrationsAssemblyPath;
        }

        public MigrationFunctionAttribute(string dependsOn, string migrationsAssemblyPath, string migrationFunctionArn)
        {
            DependsOn = dependsOn;
            MigrationsAssemblyPath = migrationsAssemblyPath;
            MigrationFunctionArn = migrationFunctionArn;
        }

        public Type MigrationFunction { get; }

        public string MigrationMethod { get; }
        public string DependsOn { get;  }
        // ATTRIBUTE:  ADD HERE
        public string MigrationsAssemblyPath { get; }

        public string MigrationFunctionArn { get; }
    }
}