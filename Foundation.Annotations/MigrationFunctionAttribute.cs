using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Method)]
    public class MigrationFunctionAttribute : Attribute
    {
        public string SqlBucket { get; set; }
    }
}
