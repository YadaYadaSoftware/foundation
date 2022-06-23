using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Models.Attributes;

namespace Foundation.Generators;

public class FoundationLambdaFunctionModel : ILambdaFunctionSerializable
{
    public FoundationLambdaFunctionModel(LambdaFunctionModel model, string sqlBucket, string branch)
    {
        SqlBucket = sqlBucket;
        Branch = branch;
        Handler = model.Handler;
        Name = model.Name;
        Timeout = model.Timeout;
        MemorySize = model.MemorySize;
        Role = model.Role;
        Policies = model.Policies;
        PackageType = model.PackageType;
        Attributes = model.Attributes;
    }

    public string SqlBucket { get; }
    public string Handler { get; }
    public string Name { get; }
    public uint? Timeout { get; }
    public uint? MemorySize { get; }
    public string Role { get; }
    public string Policies { get; }
    public LambdaPackageType PackageType { get; }
    public IList<AttributeModel> Attributes { get; }

    public string SourceGeneratorVersion { get; set; }

    public string Branch { get; }
}