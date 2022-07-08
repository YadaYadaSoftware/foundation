using System.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.Extensions;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using AttributeSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax;

namespace Foundation.Generators;

public class SyntaxReceiver : ISyntaxContextReceiver
{
    public HashSet<AttributeData> MigrationAttributes { get; } = new();
    public AttributeSyntax MigrationFunctionAttribute { get; set; }

    private static IEnumerable<ITypeSymbol> GetAllTypes(INamespaceSymbol root)
    {
        foreach (var namespaceOrTypeSymbol in root.GetMembers())
        {
            if (namespaceOrTypeSymbol is INamespaceSymbol @namespace) foreach (var nested in GetAllTypes(@namespace)) yield return nested;

            else if (namespaceOrTypeSymbol is ITypeSymbol type) yield return type;
        }
    }


    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
#if DEBUG
        if (!Debugger.IsAttached) Debugger.Launch();
#endif

        
        if (context.Node is AttributeSyntax attributeSyntax)
        {
            Debug.WriteLine($"{nameof(attributeSyntax)}:{attributeSyntax.ToString()}");

            var displayString = context.SemanticModel.GetTypeInfo(attributeSyntax).Type.ToDisplayString();
            if (displayString.Contains("MigrationFunctionAttribute"))
            {
                this.MigrationFunctionAttribute = attributeSyntax;
                foreach (var compilationReference in context.SemanticModel.Compilation.References)
                {
                    Debug.WriteLine(compilationReference.Display);
                }

                var types = context.SemanticModel.Compilation.SourceModule.ReferencedAssemblySymbols.SelectMany(a =>
                {
                    try
                    {
                        var main = a.Identity.Name.Split('.').Aggregate(a.GlobalNamespace, (s, c) => s.GetNamespaceMembers().Single(m => m.Name.Equals(c)));

                        return GetAllTypes(main);
                    }
                    catch
                    {
                        return Enumerable.Empty<ITypeSymbol>();
                    }
                });

                foreach (var type in types)
                {
                    Debug.WriteLine(type.Name);
                    if (type.HasAttribute(context, "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute"))
                    {
                        Debug.WriteLine(type.ToString());
                        var attributeData = type.GetAttributeData(context, "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute");
                        this.MigrationAttributes.Add(attributeData);

                    }
                }
            }
        }

        //else if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
        //{
        //    var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        //    if (classSymbol.GetAttributes().SingleOrDefault(_ => _.AttributeClass.Name.Contains("Migration")) is { } x && x.ConstructorArguments.Any())
        //    {
        //        this.MigrationAttributes.Add(x);
        //    }
        //}
    }
}

public static class SymbolHasAttributeExtension
{
    public static bool HasAttribute(
        this ISymbol symbol,
        GeneratorSyntaxContext context,
        string fullyQualifiedName)
    {
        return symbol.GetAttributes().Any<AttributeData>((Func<AttributeData, bool>)(att => att.AttributeClass != null && att.AttributeClass.Equals((ISymbol)context.SemanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedName), SymbolEqualityComparer.Default)));
    }

    public static AttributeData GetAttributeData(
        this ISymbol symbol,
        GeneratorSyntaxContext context,
        string fullyQualifiedName)
    {
        return symbol.GetAttributes().FirstOrDefault<AttributeData>((Func<AttributeData, bool>)(att => att.AttributeClass != null && att.AttributeClass.Equals((ISymbol)context.SemanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedName), SymbolEqualityComparer.Default)));
    }
}
