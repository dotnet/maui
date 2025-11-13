using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// EqualityComparer that normalizes line endings before comparing strings.
/// This ensures that tests pass regardless of the platform's line ending conventions.
/// 
/// NOTE: Consider using SnapshotAssert.Equal() or SnapshotAssert.EqualIgnoringLineEndings() 
/// instead for new tests. SnapshotAssert provides better diff output and additional features.
/// See README_SNAPSHOT_TESTING.md for details.
/// </summary>
public class LineEndingNormalizedEqualityComparer : IEqualityComparer<string?>
{
	private static readonly Regex LineEndingRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);
	
	public static LineEndingNormalizedEqualityComparer Instance { get; } = new LineEndingNormalizedEqualityComparer();

	public bool Equals(string? x, string? y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x is null || y is null) return false;
		
		var normalizedX = NormalizeLineEndings(x);
		var normalizedY = NormalizeLineEndings(y);
		
		return string.Equals(normalizedX, normalizedY, StringComparison.Ordinal);
	}

	public int GetHashCode(string? obj)
	{
		if (obj is null) return 0;
		return NormalizeLineEndings(obj).GetHashCode(StringComparison.Ordinal);
	}

	private static string NormalizeLineEndings(string input)
	{
		return LineEndingRegex.Replace(input, "\n");
	}
}