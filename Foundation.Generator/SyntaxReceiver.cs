using System.Diagnostics;
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

    public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
    {
#if DEBUG
        Debugger.Launch();
#endif
        if (context.Node is AttributeSyntax attributeSyntax)
        {
            Debug.WriteLine($"{nameof(attributeSyntax)}:{attributeSyntax.ToString()}");

            var displayString = context.SemanticModel.GetTypeInfo(attributeSyntax).Type.ToDisplayString();
            if (displayString.Contains("MigrationFunctionAttribute"))
            {
                this.MigrationFunctionAttribute = attributeSyntax;
            }
            //else if (displayString.Contains("MigrationAttribute"))
            //{
            //    this.MigrationAttributes.Add(attributeSyntaxX);
            //}
        }

        //if (context.Node is AttributeSyntax attributeSyntax
        //    && context.SemanticModel.GetTypeInfo(attributeSyntax).Type.ToDisplayString().Contains("MigrationFunctionAttribute"))
        //{
        //    this.MigrationFunctionAttribute = attributeSyntax;
        //}
        else if (context.Node is ClassDeclarationSyntax classDeclarationSyntax)
        {
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

            if (classSymbol.GetAttributes().SingleOrDefault(_ => _.AttributeClass.Name.Contains("Migration")) is { } x && x.ConstructorArguments.Any())
            {
                this.MigrationAttributes.Add(x);
                //var firstOrDefault = x.ConstructorArguments.FirstOrDefault();
                //if (firstOrDefault is { })
                //{
                //    Debug.WriteLine(firstOrDefault.Value.ToString());
                //}

            }




            //if (classDeclarationSyntax.AttributeLists.SelectMany(_ => _.Attributes).SingleOrDefault(_ => _.Name.ToString().Contains("Migration")) is { } migrationAttribute)
            //{
            //    Debug.WriteLine(migrationAttribute.Name);
            //    this.MigrationAttributes.Add(migrationAttribute);
            //}
        }
    }

}