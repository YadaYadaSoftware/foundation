using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Newtonsoft.Json.Linq;

namespace Foundation.Generators;

public interface IMigrationModel
{
    public string MigrationId { get; }
    public TypeModel MigrationFunction { get; }
    public string MigrationMethod { get; }
    string DependsOn { get; }

    // ATTRIBUTE:  ADD HERE
    public string MigrationsAssemblyPath { get; set; }
    public string MigrationsFunctionArn { get; set; }
    public string InitialCatalog { get; set; }
    public string BackupBucket { get; set; }
}