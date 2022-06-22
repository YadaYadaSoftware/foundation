using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public static class MigrationModelBuilder
{
    public static IMigrationModel Build(ILambdaFunctionSerializable lambdaFunctionModel, ITypeSymbol receiverMigrationClass)
    {
        if (receiverMigrationClass.GetAttributes().SingleOrDefault(_ => $"{_.AttributeClass.ContainingNamespace}.{_.AttributeClass.Name}" == "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute") is not { } migrationAttribute
            || !migrationAttribute.ConstructorArguments.Any())
        {
            throw new InvalidOperationException();
        }
        string migrationId = migrationAttribute.ConstructorArguments.FirstOrDefault().Value!.ToString();

        return new MigrationModel(lambdaFunctionModel, receiverMigrationClass.ContainingNamespace.ToString(), receiverMigrationClass.Name, migrationId, lambdaFunctionModel.Name);
    }
}