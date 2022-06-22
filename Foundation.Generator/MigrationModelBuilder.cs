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
            var data = MigrationFunctionAttributeBuilder.Build(att);
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
    public static MigrationFunctionAttribute Build(AttributeData att)
    {
        var data = new MigrationFunctionAttribute();

        foreach (var pair in att.NamedArguments)
        {
            if (pair.Key == nameof(data.SqlBucket) && pair.Value.Value is string value)
            {
                data.SqlBucket = value;
            }

        }

        return data;
    }
}

public static class MigrationModelBuilder
{
    public static IMigrationModel Build(ILambdaFunctionSerializable lambdaFunctionModel, ITypeSymbol receiverMigrationClass)
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



        return new MigrationModel(lambdaFunctionModel, receiverMigrationClass.ContainingNamespace.ToString(), receiverMigrationClass.Name, migrationId, lambdaFunctionModel.Name, "sqlbucket");
    }
}