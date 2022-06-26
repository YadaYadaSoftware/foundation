using System.Diagnostics;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public List<(ITypeSymbol classSymbol, ClassDeclarationSyntax classDeclarationSyntax)> MigrationClasses { get; } = new();

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
        try
        {
            if (context.Node is AttributeSyntax attributeSyntax
                && context.SemanticModel.GetTypeInfo(attributeSyntax).Type.ToDisplayString().Contains("MigrationFunctionAttribute"))
            {
                this.MigrationFunctionAttribute = attributeSyntax;
            }
        }
        catch (Exception e)
        {

            throw;
        }

    }

    public AttributeSyntax MigrationFunctionAttribute { get; set; }
}