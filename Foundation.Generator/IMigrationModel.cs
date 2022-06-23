using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public interface IMigrationModel
{
    public string MigrationId { get; }
    public TypeModel MigrationFunction { get; }
    public string MigrationMethod { get; }
    string DependsOn { get; }
    string SqlScriptsBucket { get; }

    // ATTRIBUTE:  ADD HERE
    string Branch { get; }

}