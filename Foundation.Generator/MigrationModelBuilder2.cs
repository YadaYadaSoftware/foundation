using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Models.Attributes;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class MigrationModelBuilder2
{
    public static IMigrationModel2 Build(ITypeSymbol typeSymbol, GeneratorExecutionContext generatorExecutionContext)
    {
        if (typeSymbol.GetAttributes().SingleOrDefault(_ => _.AttributeClass.Name == nameof(MigrationFunction2Attribute)) is not { } migrationFunction2Attribute)
        {
            throw new InvalidOperationException();
        }
        if (typeSymbol.GetAttributes().SingleOrDefault(_ => $"{_.AttributeClass.ContainingNamespace}.{_.AttributeClass.Name}" == "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute") is not { } migrationAttribute
            || !migrationAttribute.ConstructorArguments.Any())
        {
            throw new InvalidOperationException();
        }

        //var attribute = FoundationAttributeModelBuilder.Build(lambdaMethodSymbol.GetAttributes().Single(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)), context);
        var attribute = MigrationFunction2AttributeBuilder.Build(typeSymbol.GetAttributes().Single(_ => _.AttributeClass.Name == nameof(MigrationFunction2Attribute)), generatorExecutionContext);


        string migrationId = migrationAttribute.ConstructorArguments.FirstOrDefault().Value!.ToString();

        return new MigrationModel2(migrationId, attribute.Data.MigrationFunction, attribute.Data.MigrationMethod);
    }
}

public class MigrationFunction2AttributeBuilder
{
    public static AttributeModel2<IMigrationFunction2AttributeModel> Build(AttributeData att, GeneratorExecutionContext generatorExecutionContext)
    {
        if (att.AttributeClass == null)
        {
            throw new NotSupportedException($"An attribute must have an attribute class. Attribute class is not found for {att}");
        }

        AttributeModel2<IMigrationFunction2AttributeModel> model = null;
        if (att.AttributeClass.Equals(generatorExecutionContext.Compilation.GetTypeByMetadataName(typeof(MigrationFunction2Attribute).FullName), SymbolEqualityComparer.Default))
        {
            var data = MigrationFunctionAttributeBuilder2.Build(att, generatorExecutionContext);
            model = new AttributeModel2<IMigrationFunction2AttributeModel>
            {
                Data = data,
                Type = TypeModelBuilder.Build(att.AttributeClass, generatorExecutionContext)
            };
        }

        return model;
    }
}

public interface IMigrationFunction2AttributeModel
{
    TypeModel MigrationFunction { get; set; }
    string MigrationMethod { get; set; }

}

public class MigrationFunction2AttributeModel : IMigrationFunction2AttributeModel
{
    public string MigrationMethod { get; set; }
    public TypeModel MigrationFunction { get; set; }
}

public class AttributeModel2<T> : AttributeModel
{
    public T Data { get; set; }
}

public class MigrationFunctionAttributeBuilder2
{
    public static IMigrationFunction2AttributeModel Build(AttributeData att, GeneratorExecutionContext generatorExecutionContext)
    {
        var data = new MigrationFunction2AttributeModel();

        foreach (var pair in att.NamedArguments)
        {
            if (pair.Key == nameof(data.MigrationFunction) && pair.Value.Value is INamedTypeSymbol migrationFunctionType)
            {
                data.MigrationFunction = TypeModelBuilder.Build(migrationFunctionType, generatorExecutionContext);
            } else if (pair.Key == nameof(data.MigrationMethod) && pair.Value.Value is string migrationMethod)
            {
                data.MigrationMethod = migrationMethod;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        return data;

    }
}