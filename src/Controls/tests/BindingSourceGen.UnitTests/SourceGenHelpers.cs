using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.Maui.Controls.BindingSourceGen;
using System.Runtime.Loader;
using System.Collections.Immutable;

internal record CodeGeneratorResult(
    string GeneratedCode,
    ImmutableArray<Diagnostic> SourceCompilationDiagnostics,
    ImmutableArray<Diagnostic> SourceGeneratorDiagnostics,
    ImmutableArray<Diagnostic> GeneratedCodeCompilationDiagnostics,
    CodeWriterBinding? Binding);

internal static class SourceGenHelpers
{
    private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Preview).WithFeatures(
                [new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.Maui.Controls.Generated")]);

    internal static CodeGeneratorResult Run(string source)
    {
        var inputCompilation = CreateCompilation(source);
        var generator = new BindingSourceGenerator();
        var sourceGenerator = generator.AsSourceGenerator();
        var driver = CSharpGeneratorDriver.Create(
            [sourceGenerator],
            driverOptions: new GeneratorDriverOptions(default, trackIncrementalGeneratorSteps: true),
            parseOptions: ParseOptions);

        var result = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation compilation, out _).GetRunResult().Results.Single();

        var generatedCodeDiagnostic = compilation.GetDiagnostics();
        var generatedCode = result.GeneratedSources.Length == 1 ? result.GeneratedSources.Single().SourceText.ToString() : "";

        var trackedSteps = result.TrackedSteps;

        var resultBinding = trackedSteps.TryGetValue("Bindings", out ImmutableArray<IncrementalGeneratorRunStep> value)
            ? (CodeWriterBinding)value[0].Outputs[0].Value
            : null;

        return new CodeGeneratorResult(
            GeneratedCode: generatedCode,
            SourceCompilationDiagnostics: inputCompilation.GetDiagnostics(),
            SourceGeneratorDiagnostics: result.Diagnostics,
            GeneratedCodeCompilationDiagnostics: generatedCodeDiagnostic,
            Binding: resultBinding);
    }

    internal static Compilation CreateCompilation(string source)
        => CSharpCompilation.Create("compilation",
            [CSharpSyntaxTree.ParseText(source, ParseOptions, path: @"Path\To\Program.cs")],
            [
                MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.BindableObject).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("System.Runtime")).Location),
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
            .WithNullableContextOptions(NullableContextOptions.Enable));
}