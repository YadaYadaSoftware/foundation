using System.Diagnostics;
using Amazon.Lambda.Annotations.SourceGenerator.Extensions;
using Foundation.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using AttributeListSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax;
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
        var semanticModelCompilation = context.SemanticModel.Compilation;
        var compilationSourceModule = semanticModelCompilation.SourceModule;

#if DEBUG
        if (!Debugger.IsAttached) Debugger.Launch();
#endif

        if (context.Node is AttributeSyntax attributeSyntax)
        {
            var displayString = context.SemanticModel.GetTypeInfo(attributeSyntax).Type.ToDisplayString();
            if (displayString.Contains("MigrationFunctionAttribute"))
            {
                //SymbolInfo x = context.SemanticModel.GetSymbolInfo(context.Node.Parent);
                foreach (var attributeArgumentSyntax in attributeSyntax.ArgumentList.Arguments)
                {
                    //Debug.WriteLine(attributeArgumentSyntax.NameEquals.Name.ToString());
                    Debug.WriteLine(attributeArgumentSyntax.NameColon.Name.ToString());
                }
                //var z = x.Symbol.GetAttributeData(context, "Foundation.Annotations.MigrationFunctionAttribute");


                this.MigrationFunctionAttribute = attributeSyntax;
                foreach (var compilationReference in semanticModelCompilation.References)
                {
                    Debug.WriteLine(compilationReference.Display);
                }

                var types = compilationSourceModule.ReferencedAssemblySymbols.SelectMany(a =>
                {
                    Debug.WriteLine($"{nameof(a)}.{nameof(a.Name)}={a.Name}");
                    try
                    {
                        INamespaceSymbol? main = a.Identity.Name.Split('.').Aggregate(a.GlobalNamespace, (s, c) => s.GetNamespaceMembers().Single(m => m.Name.Equals(c)));

                        return GetAllTypes(main);
                    }
                    catch
                    {
                        if (a.Name.Contains("Migrations")) Debugger.Break();
                        Debug.WriteLine(a.Name);
                        return Enumerable.Empty<ITypeSymbol>();
                    }
                });

                foreach (var type in types)
                {
                    if (!type.HasAttribute(context, "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute")) continue;

                    var attributeData = type.GetAttributeData(context, "Microsoft.EntityFrameworkCore.Migrations.MigrationAttribute");
                    this.MigrationAttributes.Add(attributeData);
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
