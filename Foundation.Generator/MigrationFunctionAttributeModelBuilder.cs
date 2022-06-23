using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class MigrationFunctionAttributeModelBuilder
{
    public static AttributeModel2<IMigrationFunctionAttributeModel> Build(AttributeData att, GeneratorExecutionContext generatorExecutionContext)
    {
        if (att.AttributeClass == null)
        {
            throw new NotSupportedException($"An attribute must have an attribute class. Attribute class is not found for {att}");
        }

        AttributeModel2<IMigrationFunctionAttributeModel> model = null;
        if (att.AttributeClass.Equals(generatorExecutionContext.Compilation.GetTypeByMetadataName(typeof(MigrationFunctionAttribute).FullName), SymbolEqualityComparer.Default))
        {
            var data = MigrationFunctionAttributeBuilder.Build(att, generatorExecutionContext);
            model = new AttributeModel2<IMigrationFunctionAttributeModel>
            {
                Data = data,
                Type = TypeModelBuilder.Build(att.AttributeClass, generatorExecutionContext)
            };
        }

        return model;
    }
}