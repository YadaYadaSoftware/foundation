using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class MigrationModelBuilder
{
    public static IMigrationModel Build(GeneratorExecutionContext generatorExecutionContext, AttributeData migrationAttributeData, AttributeSyntax receiverMigrationFunctionAttribute,
        IMigrationFunctionAttributeModel migrationFunctionModel)
    {
        var migrationId = migrationAttributeData.ConstructorArguments.SingleOrDefault().Value.ToString();
        return new MigrationModel(migrationId, migrationFunctionModel.MigrationFunction, migrationFunctionModel.MigrationMethod, migrationFunctionModel.DependsOn, migrationFunctionModel.SqlScriptBucket, migrationFunctionModel.Branch, migrationFunctionModel.MigrationsAssembly, migrationFunctionModel.MigrationsAssemblyPath);
    }
}