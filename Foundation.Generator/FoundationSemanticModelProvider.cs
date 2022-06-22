using Amazon.Lambda.Annotations.SourceGenerator;
using Microsoft.CodeAnalysis;

namespace Foundation.Generators;

public class FoundationSemanticModelProvider : SemanticModelProvider
{
    private readonly GeneratorExecutionContext _context;

    public FoundationSemanticModelProvider(GeneratorExecutionContext context) : base(context)
    {
        _context = context;
    }
}