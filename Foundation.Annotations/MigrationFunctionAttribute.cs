using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class MigrationFunctionAttribute : Attribute
    {
        public Type MigrationFunction { get; set; }

        public string MigrationMethod { get; set; }
        public string DependsOn { get; set; }
        public string SqlScriptBucket { get; set; }
        // ATTRIBUTE:  ADD HERE
        public string Branch { get; set; }

        public string MigrationsAssembly { get; set; }
        public string MigrationsAssemblyPath { get; set; }

    }
}