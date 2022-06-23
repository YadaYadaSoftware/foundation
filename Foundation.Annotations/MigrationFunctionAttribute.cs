﻿using System;

namespace Foundation.Annotations
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationFunctionAttribute : Attribute
    {
        public Type MigrationFunction { get; set; }

        public string MigrationMethod { get; set; }
        public string DependsOn { get; set; }
        public string SqlScriptBucket { get; set; }
        // ATTRIBUTE:  ADD HERE

    }
}