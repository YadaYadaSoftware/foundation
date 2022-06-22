using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class FoundationAnnotationReport : AnnotationReport
{
    public ILambdaFunctionSerializable MigrationFunctionModel { get; set; }
    public List<ITypeSymbol> MigrationClasses { get; set; } = new();
}