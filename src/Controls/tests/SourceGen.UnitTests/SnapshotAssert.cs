using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Provides improved assertion utilities for snapshot testing in source generator tests.
/// These utilities address common pain points:
/// - Avoids pedantic whitespace failures by normalizing leading/trailing whitespace
/// - Normalizes line endings automatically
/// - Provides better diff output through xUnit's built-in functionality
/// </summary>
public static class SnapshotAssert
{
	private static readonly Regex LineEndingRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);

	/// <summary>
	/// Asserts that two code strings are equal after normalizing whitespace and line endings.
	/// This normalization trims each line and removes empty lines, focusing on actual content.
	/// This is the recommended method for comparing generated code snapshots.
	/// </summary>
	/// <param name="expected">The expected code string</param>
	/// <param name="actual">The actual generated code string</param>
	public static void Equal(string expected, string actual)
	{
		var normalizedExpected = NormalizeForComparison(expected);
		var normalizedActual = NormalizeForComparison(actual);
		
		Assert.Equal(normalizedExpected, normalizedActual);
	}

	/// <summary>
	/// Asserts that two code strings are equal, ignoring only line ending differences.
	/// Whitespace within and around lines is preserved.
	/// </summary>
	/// <param name="expected">The expected code string</param>
	/// <param name="actual">The actual generated code string</param>
	public static void EqualIgnoringLineEndings(string expected, string actual)
	{
		var normalizedExpected = NormalizeLineEndings(expected);
		var normalizedActual = NormalizeLineEndings(actual);
		
		Assert.Equal(normalizedExpected, normalizedActual);
	}

	/// <summary>
	/// Asserts that two code strings are strictly equal (including whitespace and line endings).
	/// Use this only when exact formatting must be preserved.
	/// </summary>
	/// <param name="expected">The expected code string</param>
	/// <param name="actual">The actual generated code string</param>
	public static void StrictEqual(string expected, string actual)
	{
		Assert.Equal(expected, actual);
	}

	private static string NormalizeForComparison(string input)
	{
		// Normalize line endings first
		var normalized = NormalizeLineEndings(input);
		
		// Split into lines, trim each line, filter out empty lines
		var lines = normalized.Split('\n')
			.Select(line => line.Trim())
			.Where(line => !string.IsNullOrWhiteSpace(line));
		
		// Join back with consistent line endings
		return string.Join("\n", lines);
	}

	private static string NormalizeLineEndings(string input)
	{
		return LineEndingRegex.Replace(input, "\n");
	}
}
