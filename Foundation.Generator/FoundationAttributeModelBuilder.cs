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
            var data = MigrationFunctionAttributeBuilder.Build(att, context);
            model = new AttributeModel<MigrationFunctionAttribute>
            {
                Data = data,
                Type = TypeModelBuilder.Build(att.AttributeClass, context)
            };
        }

        return model;

    }

}