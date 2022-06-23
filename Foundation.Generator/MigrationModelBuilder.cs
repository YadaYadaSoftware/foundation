using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class MigrationModelBuilder
{
    public static IMigrationModel Build(ITypeSymbol typeSymbol, GeneratorExecutionContext generatorExecutionContext)
    {
        if (typeSymbol.GetAttributes().SingleOrDefault(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)) is not { } migrationFunction2Attribute)
        {
            throw new InvalidOperationException();
        }
        if (typeSymbol.GetAttributes().SingleOrDefault(_ => $"{_.AttributeClass.ContainingNamespace}.{_.AttributeClass.Name}" == "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute") is not { } migrationAttribute
            || !migrationAttribute.ConstructorArguments.Any())
        {
            throw new InvalidOperationException();
        }

        var attribute = MigrationFunctionAttributeModelBuilder.Build(typeSymbol.GetAttributes().Single(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)), generatorExecutionContext);

        string migrationId = migrationAttribute.ConstructorArguments.FirstOrDefault().Value!.ToString();

        // ATTRIBUTE:  ADD HERE

        return new MigrationModel(migrationId, attribute.Data.MigrationFunction, attribute.Data.MigrationMethod, attribute.Data.DependsOn, attribute.Data.SqlScriptBucket, attribute.Data.Branch);
    }
}