using Amazon.Lambda.Annotations.SourceGenerator.Models.Attributes;

namespace Foundation.Generators;

public class AttributeModel2<T> : AttributeModel
{
    public T Data { get; set; }
}