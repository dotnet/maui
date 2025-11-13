using Xunit;

namespace BindingSourceGen.UnitTests;

/// <summary>
/// Tests for the SnapshotAssert utility to ensure it behaves as expected.
/// </summary>
public class SnapshotAssertTests
{
	[Fact]
	public void Equal_IgnoresLeadingWhitespace()
	{
		var expected = """
            namespace Test
            {
                class MyClass { }
            }
            """;

		var actual = """
		namespace Test
		{
		    class MyClass { }
		}
		""";

		// Should pass - leading whitespace is trimmed from each line
		SnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_IgnoresTrailingWhitespace()
	{
		var expected = "line1   \nline2\nline3  ";
		var actual = "line1\nline2\nline3";

		// Should pass - trailing whitespace on each line is trimmed
		SnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_IgnoresLineEndingDifferences()
	{
		var expected = "line1\nline2\nline3";
		var actual = "line1\r\nline2\rline3";

		// Should pass - line endings are normalized
		SnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_IgnoresInternalWhitespace()
	{
		var expected = "int    x    =    42;";
		var actual = "int x = 42;";

		// Should pass - each line is trimmed, so leading/trailing space is removed
		// However, internal whitespace within trimmed content is preserved in the comparison
		// These will fail because "int    x    =    42;" != "int x = 42;" after trimming
		Assert.Throws<Xunit.Sdk.EqualException>(() =>
			SnapshotAssert.Equal(expected, actual)
		);
	}

	[Fact]
	public void Equal_FailsOnDifferentContent()
	{
		var expected = "namespace Test { }";
		var actual = "namespace Different { }";

		// Should fail - content is different
		Assert.Throws<Xunit.Sdk.EqualException>(() =>
			SnapshotAssert.Equal(expected, actual)
		);
	}

	[Fact]
	public void Equal_ComparesMultilineCodeCorrectly()
	{
		var expected = """
            namespace Microsoft.Maui.Controls.Generated
            {
                internal static partial class GeneratedBindingInterceptors
                {
                    public static void SetBinding()
                    {
                        var binding = new TypedBinding();
                    }
                }
            }
            """;

		var actual = """
		namespace Microsoft.Maui.Controls.Generated
		{
		    internal static partial class GeneratedBindingInterceptors
		    {
		        public static void SetBinding()
		        {
		            var binding = new TypedBinding();
		        }
		    }
		}
		""";

		// Should pass - leading whitespace on each line is ignored (tabs vs spaces)
		SnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_IgnoresEmptyLines()
	{
		var expected = "line1\n\n\nline2";
		var actual = "line1\nline2";

		// Should pass - empty lines are filtered out
		SnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void EqualIgnoringLineEndings_PreservesWhitespace()
	{
		var expected = "  int x = 42;  ";
		var actual = "  int x = 42;  ";

		// Should pass - whitespace is preserved, only line endings normalized
		SnapshotAssert.EqualIgnoringLineEndings(expected, actual);
	}

	[Fact]
	public void EqualIgnoringLineEndings_FailsOnWhitespaceDifference()
	{
		var expected = "int x = 42;";
		var actual = "  int x = 42;";

		// Should fail - whitespace is preserved
		Assert.Throws<Xunit.Sdk.EqualException>(() =>
			SnapshotAssert.EqualIgnoringLineEndings(expected, actual)
		);
	}

	[Fact]
	public void EqualIgnoringLineEndings_NormalizesLineEndings()
	{
		var expected = "line1\nline2";
		var actual = "line1\r\nline2";

		// Should pass - line endings are normalized
		SnapshotAssert.EqualIgnoringLineEndings(expected, actual);
	}

	[Fact]
	public void StrictEqual_RequiresExactMatch()
	{
		var expected = "test\n";
		var actual = "test\r\n";

		// Should fail - line ending differs
		Assert.Throws<Xunit.Sdk.EqualException>(() =>
			SnapshotAssert.StrictEqual(expected, actual)
		);
	}

	[Fact]
	public void StrictEqual_PassesOnExactMatch()
	{
		var expected = "test\nline2";
		var actual = "test\nline2";

		// Should pass - exact match
		SnapshotAssert.StrictEqual(expected, actual);
	}

	[Fact]
	public void Equal_HandlesEmptyStrings()
	{
		SnapshotAssert.Equal("", "");
		SnapshotAssert.Equal("   ", "");
		SnapshotAssert.Equal("", "   ");
	}

	[Fact]
	public void Equal_HandlesStringsWithOnlyWhitespace()
	{
		var expected = "   \n   \n   ";
		var actual = "\n\n\n";

		// Lines with only whitespace are filtered out, leaving empty result
		SnapshotAssert.Equal(expected, actual);
	}
}
