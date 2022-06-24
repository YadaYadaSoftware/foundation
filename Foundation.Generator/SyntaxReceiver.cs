﻿using System.Diagnostics;
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
            if (context.Node is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                // Get the symbol being declared by the class, and keep it if its annotated
                ITypeSymbol classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) as ITypeSymbol;

                if (classSymbol.GetAttributes().Any(data => data.AttributeClass.Name == "MigrationFunctionAttribute"))
                {
                    if (this.MigrationClasses.All(_ => _.classSymbol.ToDisplayString() != classSymbol.ToDisplayString()))
                    {
                        this.MigrationClasses.Add((classSymbol, classDeclarationSyntax));
                    }
                }
            }
        }
        catch (Exception e)
        {

            throw;
        }

    }

}