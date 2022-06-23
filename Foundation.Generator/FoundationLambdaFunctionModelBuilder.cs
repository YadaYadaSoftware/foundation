using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Amazon.Lambda.Annotations.SourceGenerator.Models.Attributes;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class FoundationLambdaFunctionModelBuilder
{
    public static FoundationLambdaFunctionModel Build(IMethodSymbol lambdaMethodSymbol, IMethodSymbol configureMethodSymbol, GeneratorExecutionContext context)
    {

        var attribute = FoundationAttributeModelBuilder.Build(lambdaMethodSymbol.GetAttributes().Single(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)), context);

        var lambdaFunctionModel = LambdaFunctionModelBuilder.Build(lambdaMethodSymbol, configureMethodSymbol, context);
        AttributeModel[] attributes = new AttributeModel[lambdaFunctionModel.Attributes.Count+1];
        lambdaFunctionModel.Attributes.CopyTo(attributes, 0);
        var sqlBucket = string.Empty;
        var branch = string.Empty;
        if (lambdaMethodSymbol.GetAttributes().SingleOrDefault(_ => _.AttributeClass.Name == nameof(MigrationFunctionAttribute)) is {} att)
        {
            var built = MigrationFunctionAttributeBuilder.Build(att, context);
            sqlBucket = built.SqlBucket;
            branch = built.Branch;

        }
        return new FoundationLambdaFunctionModel(lambdaFunctionModel, sqlBucket, branch );
    }
}