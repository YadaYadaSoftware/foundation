using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

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