using System.Diagnostics;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public HashSet<ClassDeclarationSyntax> MigrationClasses { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        try
        {
            if (context.Node is AttributeSyntax attributeSyntax
                && context.SemanticModel.GetTypeInfo(attributeSyntax).Type.ToDisplayString().Contains("MigrationFunctionAttribute"))
            {
                this.MigrationFunctionAttribute = attributeSyntax;
            }
            else if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
            {
                if (classDeclarationSyntax.AttributeLists.Any(_ => _.Attributes.Any(__ => __.Name.ToString() == "Migration")))
                {
                    this.MigrationClasses.Add(classDeclarationSyntax);

                }
            }
        }
        catch (Exception e)
        {

            throw;
        }

    }

    public AttributeSyntax MigrationFunctionAttribute { get; set; }
}