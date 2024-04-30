using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.BindingSourceGen;


internal record CodeGeneratorResult(
    string GeneratedCode,
    ImmutableArray<Diagnostic> SourceCompilationDiagnostics,
    ImmutableArray<Diagnostic> SourceGeneratorDiagnostics,
    ImmutableArray<Diagnostic> GeneratedCodeCompilationDiagnostics,
    SetBindingInvocationDescription? Binding);

internal static class SourceGenHelpers
{
    private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Preview).WithFeatures(
                [new KeyValuePair<string, string>("InterceptorsPreviewNamespaces", "Microsoft.Maui.Controls.Generated")]);

    internal static List<string> StepsForComparison = [TrackingNames.Bindings, TrackingNames.BindingsWithDiagnostics];

    internal static CSharpGeneratorDriver CreateDriver()
    {
        var generator = new BindingSourceGenerator();
        var sourceGenerator = generator.AsSourceGenerator();
        return CSharpGeneratorDriver.Create(
            [sourceGenerator],
            driverOptions: new GeneratorDriverOptions(disabledOutputs: IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true),
            parseOptions: ParseOptions);
    }

    internal static CodeGeneratorResult Run(string source)
    {
        var inputCompilation = CreateCompilation(source);
        var driver = CreateDriver();

        var result = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation compilation, out _).GetRunResult().Results.Single();

        var generatedCodeDiagnostic = compilation.GetDiagnostics();
        var generatedCode = result.GeneratedSources.Length == 1 ? result.GeneratedSources.Single().SourceText.ToString() : "";

        var trackedSteps = result.TrackedSteps;

        var resultBinding = trackedSteps.TryGetValue("Bindings", out ImmutableArray<IncrementalGeneratorRunStep> value)
            ? (SetBindingInvocationDescription)value[0].Outputs[0].Value
            : null;

        return new CodeGeneratorResult(
            GeneratedCode: generatedCode,
            SourceCompilationDiagnostics: inputCompilation.GetDiagnostics(),
            SourceGeneratorDiagnostics: result.Diagnostics,
            GeneratedCodeCompilationDiagnostics: generatedCodeDiagnostic,
            Binding: resultBinding);
    }

    private static Compilation CreateCompilationFromSyntaxTrees(List<SyntaxTree> syntaxTrees)
        => CSharpCompilation.Create("compilation",
            syntaxTrees,
            [
                MetadataReference.CreateFromFile(typeof(Microsoft.Maui.Controls.BindableObject).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName("System.Runtime")).Location),
            ],
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
            .WithNullableContextOptions(NullableContextOptions.Enable));


    internal static Compilation CreateCompilation(string source)
    {
        return CreateCompilationFromSyntaxTrees([CSharpSyntaxTree.ParseText(source, ParseOptions, path: @"Path\To\Program.cs")]);
    }

    internal static Compilation CreateCompilation(List<string> sources)
    {
        var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(source, ParseOptions, path: $@"Path\To\Program{sources.IndexOf(source)}.cs")).ToList();
        return CreateCompilationFromSyntaxTrees(syntaxTrees);
    }
}
