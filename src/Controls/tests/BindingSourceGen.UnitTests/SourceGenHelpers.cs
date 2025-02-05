using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;


internal record CodeGeneratorResult(
	Dictionary<string, string> GeneratedFiles,
	ImmutableArray<Diagnostic> SourceCompilationDiagnostics,
	ImmutableArray<Diagnostic> SourceGeneratorDiagnostics,
	ImmutableArray<Diagnostic> GeneratedCodeCompilationDiagnostics,
	BindingInvocationDescription? Binding);

internal static class SourceGenHelpers
{
	private static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(LanguageVersion.Preview).WithFeatures(
				[new KeyValuePair<string, string>("InterceptorsNamespaces", "Microsoft.Maui.Controls.Generated")]);

	internal static List<string> StepsForComparison = [TrackingNames.Bindings, TrackingNames.BindingsWithDiagnostics];

	internal static CSharpGeneratorDriver CreateDriver(IEnumerable<ISourceGenerator> generators)
	{
		return CSharpGeneratorDriver.Create(
			generators: generators,
			driverOptions: new GeneratorDriverOptions(disabledOutputs: IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true),
			parseOptions: ParseOptions);
	}

	internal static CodeGeneratorResult Run(string source)
	{
		return Run(source, new[] { new BindingSourceGenerator() });
	}

	internal static CodeGeneratorResult Run(string source, IEnumerable<IIncrementalGenerator> generators)
	{
		// Function assumes the first generator in a list is BindingSourceGenerator
		Assert.NotEmpty(generators);
		Assert.IsType<BindingSourceGenerator>(generators.First());

		var inputCompilation = CreateCompilation(source);
		var driver = CreateDriver(generators.Select(g => g.AsSourceGenerator()));

		var result = driver.RunGeneratorsAndUpdateCompilation(inputCompilation, out Compilation compilation, out _).GetRunResult().Results.FirstOrDefault();
		var generatedCode = result.GeneratedSources.FirstOrDefault().SourceText.ToString();

		var trackedSteps = result.TrackedSteps;
		var resultBinding = trackedSteps.TryGetValue("Bindings", out ImmutableArray<IncrementalGeneratorRunStep> value)
			? (BindingInvocationDescription)value[0].Outputs[0].Value
			: null;

		return new CodeGeneratorResult(
			GeneratedFiles: result.GeneratedSources.ToDictionary(source => source.HintName, source => source.SourceText.ToString()),
			SourceCompilationDiagnostics: inputCompilation.GetDiagnostics(),
			SourceGeneratorDiagnostics: result.Diagnostics,
			GeneratedCodeCompilationDiagnostics: compilation.GetDiagnostics(),
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

	internal static Compilation CreateCompilation(Dictionary<string, string> sources)
	{
		var syntaxTrees = sources.Select(s => CSharpSyntaxTree.ParseText(s.Value, ParseOptions, path: s.Key)).ToList();
		return CreateCompilationFromSyntaxTrees(syntaxTrees);
	}
}
