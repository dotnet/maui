using System.Linq;
using CSharpier;
using Xunit;

namespace BindingSourceGen.UnitTests;

/// <summary>
/// Experimental snapshot assertion utility that uses CSharpier to format code before comparison.
/// This ensures that only semantically meaningful differences cause test failures,
/// regardless of formatting style.
/// </summary>
public static class CSharpierSnapshotAssert
{
	/// <summary>
	/// Asserts that two C# code strings are semantically equal after formatting with CSharpier.
	/// This normalizes all formatting differences (indentation, spacing, line breaks, etc.)
	/// and focuses on the actual code structure.
	/// </summary>
	/// <param name="expected">The expected C# code</param>
	/// <param name="actual">The actual generated C# code</param>
	public static void Equal(string expected, string actual)
	{
		// Format both strings using CSharpier
		var formattedExpected = FormatCode(expected);
		var formattedActual = FormatCode(actual);
		
		// Compare the formatted results
		Assert.Equal(formattedExpected, formattedActual);
	}

	/// <summary>
	/// Formats C# code using CSharpier with default options.
	/// </summary>
	private static string FormatCode(string code)
	{
		try
		{
			// Use CSharpier's CodeFormatter API
			var result = CodeFormatter.Format(code, new CodeFormatterOptions
			{
				// Use default options for consistent formatting
				Width = 100,
				IndentSize = 4,
				EndOfLine = EndOfLine.LF
			});

			// Check if formatting succeeded (no compilation errors means success)
			if (result.CompilationErrors?.Any() == true)
			{
				// If there are syntax errors, return the original code
				// This allows the test to still compare and show what's different
				return code;
			}

			return result.Code;
		}
		catch
		{
			// If formatting fails for any reason, return the original code
			// This ensures tests don't break due to formatter issues
			return code;
		}
	}
}
