using Foundation.Annotations;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class MigrationFunctionAttributeBuilder
{
    public static MigrationFunctionAttribute Build(AttributeData att, GeneratorExecutionContext context)
    {
        var data = new MigrationFunctionAttribute();

        foreach (var pair in att.NamedArguments)
        {
            if (pair.Key == nameof(data.SqlBucket) && pair.Value.Value is string value)
            {
                data.SqlBucket = value;
            }
            if (pair.Key == nameof(data.Branch) && pair.Value.Value is string branch)
            {
                data.Branch = branch;
            }

        }

        return data;
    }
}