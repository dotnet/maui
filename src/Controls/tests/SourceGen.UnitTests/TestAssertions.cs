using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Static helper class for common test assertions.
/// </summary>
public static class TestAssertions
{
	/// <summary>
	/// Asserts that two strings are equal after normalizing their line endings.
	/// This method should be used instead of Assert.Equal when comparing generated code
	/// to avoid issues with different line ending conventions across platforms.
	/// </summary>
	/// <param name="expected">The expected string</param>
	/// <param name="actual">The actual string</param>
	public static void AssertEqualIgnoringLineEndings(string? expected, string? actual)
	{
		Assert.Equal(expected, actual, LineEndingNormalizedEqualityComparer.Instance);
	}
}