using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MigrationFunctionAttribute : Attribute
    {
        public Type MigrationFunction { get; set; }

        public string MigrationMethod { get; set; }
        public string DependsOn { get; set; }
        // ATTRIBUTE:  ADD HERE
        public string MigrationsAssemblyPath { get; set; }
    }
}