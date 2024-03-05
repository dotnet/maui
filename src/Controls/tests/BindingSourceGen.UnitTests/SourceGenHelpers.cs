using System.Reflection;
using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.Maui.Controls.BindingSourceGen;

internal static class SourceGenHelpers
{
    internal static GeneratorDriverRunResult Run(string source)
    {
        var inputCompilation = CreateCompilation(source);
        var driver = CSharpGeneratorDriver.Create(new BindingSourceGenerator());
        return driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out _, out _).GetRunResult();
    } 

    internal static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.BindableObject).GetTypeInfo().Assembly.Location),
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));
}