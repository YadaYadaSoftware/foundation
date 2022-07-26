﻿using System.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class MigrationFunctionAttributeModelBuilder
{
    //public static AttributeModel2<IMigrationFunctionAttributeModel> Build(AttributeData att, GeneratorExecutionContext generatorExecutionContext)
    //{
    //    if (att.AttributeClass == null)
    //    {
    //        throw new NotSupportedException($"An attribute must have an attribute class. Attribute class is not found for {att}");
    //    }

    //    AttributeModel2<IMigrationFunctionAttributeModel> model = null;
    //    if (att.AttributeClass.Equals(generatorExecutionContext.Compilation.GetTypeByMetadataName(typeof(MigrationFunctionAttribute).FullName), SymbolEqualityComparer.Default))
    //    {
    //        var data = MigrationFunctionAttributeBuilder.Build(att, generatorExecutionContext);
    //        model = new AttributeModel2<IMigrationFunctionAttributeModel>
    //        {
    //            Data = data,
    //            Type = TypeModelBuilder.Build(att.AttributeClass, generatorExecutionContext)
    //        };
    //    }

    //    return model;
    //}

    public static IMigrationFunctionAttributeModel Build(AttributeSyntax receiverMigrationFunctionAttribute, GeneratorExecutionContext context)
    {

        IMigrationFunctionAttributeModel model = new MigrationFunctionAttributeModel();
        SemanticModel semanticModel = context.Compilation.GetSemanticModel(receiverMigrationFunctionAttribute.SyntaxTree);

        foreach (var attributeArgumentSyntax in receiverMigrationFunctionAttribute.ArgumentList.Arguments)
        {
            var name = attributeArgumentSyntax.NameColon.Name.ToString().ToUpperInvariant();
            switch (name)
            {
                case "MIGRATIONFUNCTION":


                    Debug.WriteLine(attributeArgumentSyntax.Expression);
                    // ^^^ outputs 'typeof(MigrationFunctions)'

                    TypeInfo t = semanticModel.GetTypeInfo(attributeArgumentSyntax.Expression);
                    Debug.WriteLine(t.Type.ToDisplayString());
                    // ^^^ outputs 'System.Type'

                    var typeOfExpression = (TypeOfExpressionSyntax)attributeArgumentSyntax.Expression;
                    var typeSyntax = typeOfExpression.Type;
                    var type = semanticModel.GetTypeInfo(typeSyntax);
                    Debug.WriteLine(type.Type.ToDisplayString());



                    model.MigrationFunction = TypeModelBuilder.Build(type.Type, context);

                    /// HOW DO I GET THE TYPE HERE, which should be "MigrationFunctions", not "System.Type"???
                    break;
                case "MIGRATIONMETHOD":
                {
                    var value = semanticModel.GetConstantValue(attributeArgumentSyntax.Expression);
                    model.MigrationMethod = value.Value.ToString();
                    break;
                }
                case "DEPENDSON":
                {
                    var value = semanticModel.GetConstantValue(attributeArgumentSyntax.Expression);
                    model.DependsOn = value.Value.ToString();
                    break;
                }
                case "MIGRATIONSASSEMBLYPATH":
                {
                    var value = semanticModel.GetConstantValue(attributeArgumentSyntax.Expression);
                    model.MigrationsAssemblyPath = value.Value.ToString();
                    break;
                }
                case "MIGRATIONFUNCTIONARN":
                {
                    var value = semanticModel.GetConstantValue(attributeArgumentSyntax.Expression);
                    model.MigrationsFunctionArn = value.Value.ToString();
                    break;
                }
                case "INITIALCATALOG":
                {
                    var value = semanticModel.GetConstantValue(attributeArgumentSyntax.Expression);
                    model.InitialCatalog = value.Value.ToString();
                    break;
                }
                case "BACKUPBUCKET":
                {
                    var value = semanticModel.GetConstantValue(attributeArgumentSyntax.Expression);
                    model.BackupBucket = value.Value.ToString();
                    break;
                }
                default: 
                    throw new ArgumentException(attributeArgumentSyntax.NameEquals.Name.Identifier.ValueText);

            }
        }
        
        return model;

    }
}