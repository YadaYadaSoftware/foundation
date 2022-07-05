using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class MigrationModelBuilder
{
    public static IMigrationModel Build(GeneratorExecutionContext generatorExecutionContext, ClassDeclarationSyntax typeSymbol, AttributeSyntax receiverMigrationFunctionAttribute)
    {
        throw new NotImplementedException();
        //var semanticModelProvider = new SemanticModelProvider(generatorExecutionContext);

        //var migrationAttribute = typeSymbol.AttributeLists
        //string migrationId = migrationAttribute.ConstructorArguments.FirstOrDefault().Value!.ToString();

        //// ATTRIBUTE:  ADD HERE

        //return new MigrationModel(migrationId, migrationAttribute. .Data.MigrationFunction, attribute.Data.MigrationMethod, attribute.Data.DependsOn, attribute.Data.SqlScriptBucket, attribute.Data.Branch);
    }
}