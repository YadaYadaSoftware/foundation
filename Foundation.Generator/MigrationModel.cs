using Amazon.Lambda.Annotations.SourceGenerator.Models;

namespace Foundation.Generators;

public class MigrationModel : IMigrationModel
{
    private readonly ILambdaFunctionSerializable _lambdaFunctionModel;

    public MigrationModel(ILambdaFunctionSerializable lambdaFunctionModel, string @namespace, string typeName, string migrationId, string resourceName)
    {
        _lambdaFunctionModel = lambdaFunctionModel;
        Name = typeName;
        ResourceName = resourceName;
        Namespace = @namespace;
        Id = migrationId;
    }

    public string FunctionResourceName => _lambdaFunctionModel.Name;
    public string Name { get; }
    public string ResourceName { get; }
    public string Namespace { get; }
    public string FullName => this.Namespace + "." + Name;
    public string Id { get; }
}