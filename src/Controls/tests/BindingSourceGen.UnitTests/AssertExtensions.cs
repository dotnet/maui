using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

internal static class AssertExtensions
{
	/// <summary>
	/// Compares two code strings line by line after trimming and filtering empty lines.
	/// 
	/// NOTE: Consider using SnapshotAssert.Equal() instead for new tests.
	/// SnapshotAssert provides better diff output and is easier to use.
	/// See README_SNAPSHOT_TESTING.md for details.
	/// </summary>
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

		// Skip interceptable location
		Assert.Equal(expectedBinding.SimpleLocation, codeGeneratorResult.Binding.SimpleLocation);
		Assert.Equal(expectedBinding.SourceType, codeGeneratorResult.Binding.SourceType);
		Assert.Equal(expectedBinding.PropertyType, codeGeneratorResult.Binding.PropertyType);
		Assert.Equal(expectedBinding.Path, codeGeneratorResult.Binding.Path);
		Assert.Equal(expectedBinding.SetterOptions, codeGeneratorResult.Binding.SetterOptions);
		Assert.Equal(expectedBinding.NullableContextEnabled, codeGeneratorResult.Binding.NullableContextEnabled);
		Assert.Equal(expectedBinding.MethodType, codeGeneratorResult.Binding.MethodType);

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
