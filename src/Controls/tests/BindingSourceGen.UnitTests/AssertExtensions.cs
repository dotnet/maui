using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

internal static class AssertExtensions
{
	internal static void CodeIsEqual(string expectedCode, string actualCode)
	{
		var expectedLines = SplitCode(expectedCode);
		var actualLines = SplitCode(actualCode);

		foreach (var (expectedLine, actualLine) in expectedLines.Zip(actualLines))
		{
			Assert.Equal(expectedLine, actualLine);
		}

		Assert.Equal(expectedLines.Count(), actualLines.Count());
	}

	internal static void BindingsAreEqual(BindingInvocationDescription expectedBinding, CodeGeneratorResult codeGeneratorResult)
	{
		AssertNoDiagnostics(codeGeneratorResult);
		Assert.NotNull(codeGeneratorResult.Binding);

		//TODO: Change arrays to custom collections implementing IEquatable
		Assert.Equal(expectedBinding.Path, codeGeneratorResult.Binding.Path);
		Assert.Equal(expectedBinding, codeGeneratorResult.Binding);
	}

	private static IEnumerable<string> SplitCode(string code)
		=> code.Split(Environment.NewLine)
			.Select(static line => line.Trim())
			.Where(static line => !string.IsNullOrWhiteSpace(line));

	internal static void AssertNoDiagnostics(CodeGeneratorResult codeGeneratorResult)
	{
		AssertNoDiagnostics(codeGeneratorResult.SourceCompilationDiagnostics, "Source compilation");
		AssertNoDiagnostics(codeGeneratorResult.SourceGeneratorDiagnostics, "Source generator");
		AssertNoDiagnostics(codeGeneratorResult.GeneratedCodeCompilationDiagnostics, "Generated code compilation");
	}

	internal static void AssertNoDiagnostics(ImmutableArray<Diagnostic> diagnostics, string name)
	{
		if (diagnostics.Any())
		{
			var errorMessages = diagnostics.Select(error => error.ToString());
			throw new Exception($"\n{name} diagnostics: {string.Join(Environment.NewLine, errorMessages)}");
		}
	}

}
