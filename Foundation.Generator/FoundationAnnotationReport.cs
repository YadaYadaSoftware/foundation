﻿using Amazon.Lambda.Annotations.SourceGenerator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Foundation.Generators;

public class FoundationAnnotationReport : AnnotationReport
{
    public List<IMigrationModel2> Migrations2 { get; set; } = new();
}