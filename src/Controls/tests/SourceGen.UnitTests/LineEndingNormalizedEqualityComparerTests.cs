using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class LineEndingNormalizedEqualityComparerTests
{
	[Fact]
	public void Equals_SameStringsWithDifferentLineEndings_ReturnsTrue()
	{
		var comparer = LineEndingNormalizedEqualityComparer.Instance;
		var text1 = "Hello\nWorld\nTest";
		var text2 = "Hello\r\nWorld\rTest";

		Assert.True(comparer.Equals(text1, text2));
	}

	[Fact]
	public void Equals_DifferentStrings_ReturnsFalse()
	{
		var comparer = LineEndingNormalizedEqualityComparer.Instance;
		var text1 = "Hello\nWorld";
		var text2 = "Hello\nDifferent";

		Assert.False(comparer.Equals(text1, text2));
	}

	[Fact]
	public void GetHashCode_SameStringsWithDifferentLineEndings_ReturnsSameHashCode()
	{
		var comparer = LineEndingNormalizedEqualityComparer.Instance;
		var text1 = "Hello\nWorld\nTest";
		var text2 = "Hello\r\nWorld\rTest";

		Assert.Equal(comparer.GetHashCode(text1), comparer.GetHashCode(text2));
	}
}