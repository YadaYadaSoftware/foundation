using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MigrationFunctionAttribute : Attribute
    {
        public string SqlBucket { get; set; }

        public string Branch { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationFunction2Attribute : Attribute
    {
        public Type MigrationFunction { get; set; }

        public string MigrationMethod { get; set; }
    }
}
