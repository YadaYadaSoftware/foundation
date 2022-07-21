using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class MigrationModelBuilder
{
    public static IMigrationModel Build(AttributeData migrationAttributeData, IMigrationFunctionAttributeModel migrationFunctionModel)
    {
        var migrationId = migrationAttributeData.ConstructorArguments.SingleOrDefault().Value.ToString();
        return new MigrationModel(migrationId, migrationFunctionModel.MigrationFunction, migrationFunctionModel.MigrationMethod, migrationFunctionModel.DependsOn, migrationFunctionModel.MigrationsAssemblyPath, migrationFunctionModel.MigrationsFunctionArn, migrationFunctionModel.InitialCatalog);
    }
}