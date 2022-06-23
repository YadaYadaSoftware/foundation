﻿using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationModel2 : IMigrationModel2
{
    public string MigrationId { get; }
    public TypeModel MigrationFunction { get; }
    public string MigrationMethod { get; }

    public MigrationModel2(string migrationId, TypeModel dataMigrationFunction, string migrationMethod)
    {
        if (string.IsNullOrEmpty(migrationMethod)) throw new ArgumentException("Value cannot be null or empty.", nameof(migrationMethod));
        if (string.IsNullOrEmpty(migrationId)) throw new ArgumentException("Value cannot be null or empty.", nameof(migrationId));
        MigrationId = migrationId;
        MigrationFunction = dataMigrationFunction ?? throw new ArgumentNullException(nameof(dataMigrationFunction));
        MigrationMethod = migrationMethod;
    }
}