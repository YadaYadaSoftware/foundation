﻿using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationModel : IMigrationModel
{
    public string MigrationId { get; }
    public TypeModel MigrationFunction { get; }
    public string MigrationMethod { get; }
    public string DependsOn { get; }
    public string SqlScriptsBucket { get; }
    public string Branch { get; }
    public string MigrationsAssembly { get; set; }
    public string MigrationsAssemblyPath { get; set; }

    public MigrationModel(string migrationId, TypeModel dataMigrationFunction, string migrationMethod, string dependsOn, string sqlScriptsBucket, string branch, string migrationAssembly, string migrationsAssemblyPath)
    {
        if (string.IsNullOrEmpty(migrationMethod)) throw new ArgumentException("Value cannot be null or empty.", nameof(migrationMethod));
        if (string.IsNullOrEmpty(migrationId)) throw new ArgumentException("Value cannot be null or empty.", nameof(migrationId));
        MigrationId = migrationId;
        MigrationFunction = dataMigrationFunction ?? throw new ArgumentNullException(nameof(dataMigrationFunction));
        MigrationMethod = migrationMethod;
        DependsOn = dependsOn;
        SqlScriptsBucket = sqlScriptsBucket;
        Branch = branch;
        // ATTRIBUTE:  ADD HERE
        MigrationsAssembly = migrationAssembly;
        MigrationsAssemblyPath = migrationsAssemblyPath;
    }
}