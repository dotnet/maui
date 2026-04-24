using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public abstract class QueryPropertyGeneratorTestBase
{
	protected static void AssertGeneratedCode(string sourceCode, string expectedOutput)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		var result = SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation, Array.Empty<SourceGeneratorDriver.AdditionalFile>(), assertNoCompilationErrors: false);

		Assert.Empty(result.Diagnostics);

		// Filter out the marker attribute definition, keep only the QueryProperty generated code
		var generatedTrees = result.GeneratedTrees
			.Where(t => !t.FilePath.Contains("QueryPropertyGeneratedAttribute", StringComparison.Ordinal))
			.ToArray();
		Assert.Single(generatedTrees);

		var generatedSource = generatedTrees[0].ToString();
		Assert.Equal(NormalizeForComparison(expectedOutput), NormalizeForComparison(generatedSource));
	}

	protected static string NormalizeForComparison(string text)
	{
		// Normalize line endings, convert tabs to spaces, trim each line,
		// remove all blank lines (compare structural content only)
		var lines = text
			.Replace("\r\n", "\n", StringComparison.Ordinal)
			.Replace("\r", "\n", StringComparison.Ordinal)
			.Replace("\t", "    ", StringComparison.Ordinal)
			.Split('\n')
			.Select(line => line.Trim())
			.Where(line => !string.IsNullOrWhiteSpace(line));

		return string.Join("\n", lines);
	}

	protected static GeneratorDriverRunResult RunQueryPropertyGenerator(string sourceCode)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));
		return SourceGeneratorDriver.RunGenerator<QueryPropertyGenerator>(compilation, Array.Empty<SourceGeneratorDriver.AdditionalFile>(), assertNoCompilationErrors: false);
	}

	/// <summary>Filters out the marker attribute definition tree, returning only QueryProperty generated code trees.</summary>
	protected static Microsoft.CodeAnalysis.SyntaxTree[] GetQueryPropertyTrees(GeneratorDriverRunResult result) =>
		result.GeneratedTrees
			.Where(t => !t.FilePath.Contains("QueryPropertyGeneratedAttribute", StringComparison.Ordinal))
			.ToArray();

	protected static GeneratorDriverRunResult RunQueryPropertyGeneratorWithGlobalOptions(string sourceCode, params (string key, string value)[] globalOptions)
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode));

		ISourceGenerator generator = new QueryPropertyGenerator().AsSourceGenerator();
		var driverOptions = new Microsoft.CodeAnalysis.GeneratorDriverOptions(
			disabledOutputs: Microsoft.CodeAnalysis.IncrementalGeneratorOutputKind.None,
			trackIncrementalGeneratorSteps: true);

		var optionsProvider = new GlobalOptionsAnalyzerConfigOptionsProvider(globalOptions);

		Microsoft.CodeAnalysis.GeneratorDriver driver = Microsoft.CodeAnalysis.CSharp.CSharpGeneratorDriver
			.Create([generator], driverOptions: driverOptions)
			.WithUpdatedAnalyzerConfigOptions(optionsProvider);

		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
		return driver.GetRunResult();
	}

	private sealed class GlobalOptionsAnalyzerConfigOptionsProvider : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptionsProvider
	{
		private readonly DictAnalyzerConfigOptions _globalOptions;

		public GlobalOptionsAnalyzerConfigOptionsProvider((string key, string value)[] options)
		{
			var dict = new System.Collections.Generic.Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var (key, value) in options)
				dict[key] = value;
			_globalOptions = new DictAnalyzerConfigOptions(dict);
		}

		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GlobalOptions => _globalOptions;
		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(Microsoft.CodeAnalysis.SyntaxTree tree) => DictAnalyzerConfigOptions.Empty;
		public override Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions GetOptions(Microsoft.CodeAnalysis.AdditionalText textFile) => DictAnalyzerConfigOptions.Empty;

		private sealed class DictAnalyzerConfigOptions : Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions
		{
			public static readonly DictAnalyzerConfigOptions Empty = new(new System.Collections.Generic.Dictionary<string, string>());
			private readonly System.Collections.Generic.Dictionary<string, string> _dict;
			public DictAnalyzerConfigOptions(System.Collections.Generic.Dictionary<string, string> dict) => _dict = dict;
			public override bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? value) => _dict.TryGetValue(key, out value);
		}
	}
}
