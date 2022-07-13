using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class MigrationFunctionAttributeBuilder
{
    public static IMigrationFunctionAttributeModel Build(AttributeData att, GeneratorExecutionContext generatorExecutionContext)
    {
        var data = new MigrationFunctionAttributeModel();

        foreach (var pair in att.NamedArguments)
        {
            if (pair.Key == nameof(data.MigrationFunction) && pair.Value.Value is INamedTypeSymbol migrationFunctionType)
            {
                data.MigrationFunction = TypeModelBuilder.Build(migrationFunctionType, generatorExecutionContext);
            }
            else if (pair.Key == nameof(data.MigrationMethod) && pair.Value.Value is string migrationMethod)
            {
                data.MigrationMethod = migrationMethod;
            }
            else if (pair.Key == nameof(data.DependsOn) && pair.Value.Value is string dependsOn)
            {
                data.DependsOn = dependsOn;
            }
            // ATTRIBUTE:  ADD HERE
            else
            {
                throw new NotSupportedException();
            }
        }

        return data;

    }
}