using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Models.Attributes;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;


public static class FoundationAttributeModelBuilder
{
    public static AttributeModel Build(AttributeData att, GeneratorExecutionContext context)
    {
        if (att.AttributeClass == null)
        {
            throw new NotSupportedException($"An attribute must have an attribute class. Attribute class is not found for {att}");
        }

        AttributeModel model = null;
        if (att.AttributeClass.Equals(context.Compilation.GetTypeByMetadataName(typeof(MigrationFunctionAttribute).FullName), SymbolEqualityComparer.Default))
        {
            var data = MigrationFunctionAttributeBuilder.Build(att, context);
            model = new AttributeModel<MigrationFunctionAttribute>
            {
                Data = data,
                Type = TypeModelBuilder.Build(att.AttributeClass, context)
            };
        }

        return model;

    }

}

public class MigrationFunctionAttributeBuilder
{
    public static MigrationFunctionAttribute Build(AttributeData att, GeneratorExecutionContext context)
    {
        var data = new MigrationFunctionAttribute();

        foreach (var pair in att.NamedArguments)
        {
            if (pair.Key == nameof(data.SqlBucket) && pair.Value.Value is string value)
            {
                data.SqlBucket = value;
            }
            if (pair.Key == nameof(data.Branch) && pair.Value.Value is string branch)
            {
                data.Branch = branch;
            }

        }

        return data;
    }
}

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

public static class MigrationModelBuilder
{
    public static IMigrationModel Build(FoundationLambdaFunctionModel lambdaFunctionModel, ITypeSymbol receiverMigrationClass)
    {
        if (lambdaFunctionModel.Attributes.SingleOrDefault(_ => _.Type.FullName == typeof(MigrationFunctionAttribute).FullName) is not { } lambdaFunctionAttribute)
        {
            throw new NotSupportedException();
        }

        if (receiverMigrationClass.GetAttributes().SingleOrDefault(_ => $"{_.AttributeClass.ContainingNamespace}.{_.AttributeClass.Name}" == "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute") is not { } migrationAttribute
            || !migrationAttribute.ConstructorArguments.Any())
        {
            throw new InvalidOperationException();
        }
        string migrationId = migrationAttribute.ConstructorArguments.FirstOrDefault().Value!.ToString();



        return new MigrationModel(lambdaFunctionModel, receiverMigrationClass.ContainingNamespace.ToString(), receiverMigrationClass.Name, migrationId, lambdaFunctionModel.Name, lambdaFunctionModel.SqlBucket, lambdaFunctionModel.Branch);
    }
}