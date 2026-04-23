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
		Assert.Single(result.GeneratedTrees);

		var generatedSource = result.GeneratedTrees[0].ToString();
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
}
