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