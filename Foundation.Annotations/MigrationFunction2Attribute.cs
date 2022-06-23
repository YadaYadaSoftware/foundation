using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationFunction2Attribute : Attribute
    {
        public Type MigrationFunction { get; set; }

        public string MigrationMethod { get; set; }
    }
}