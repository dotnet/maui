using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.Maui.Controls.BindingSourceGen;
using System.Runtime.Loader;
using Xunit;

internal static class SourceGenHelpers
{
    internal static CodeWriterBinding GetBinding(string source)
    {
        var results = Run(source).Results.Single();
        var steps = results.TrackedSteps;

        Assert.Empty(results.Diagnostics);

        return (CodeWriterBinding)steps["Bindings"][0].Outputs[0].Value;
    }

    internal static GeneratorDriverRunResult Run(string source)
    {
        var inputCompilation = CreateCompilation(source);

        var compilerErrors = inputCompilation.GetDiagnostics().Where(i => i.Severity == DiagnosticSeverity.Error);

        if (compilerErrors.Any())
        {
            var errorMessages = compilerErrors.Select(error => error.ToString());
            throw new Exception("Compilation errors: " + string.Join("\n", errorMessages));
        }

        var generator = new BindingSourceGenerator();
        var sourceGenerator = generator.AsSourceGenerator();
        var driver = CSharpGeneratorDriver.Create([sourceGenerator], driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true));
        return driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out _, out _).GetRunResult();
    }
    internal static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source)],
            [
                MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.BindableObject).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("System.Runtime")).Location),
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
            .WithNullableContextOptions(NullableContextOptions.Enable));
}