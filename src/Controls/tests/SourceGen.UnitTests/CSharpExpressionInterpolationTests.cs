using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

// Unit tests for CSharpExpressionHelpers.GetExpressionCode covering string
// interpolation holes, in particular ternary expressions that must be
// parenthesized so their ':' is not consumed as a format specifier (issue #36155).
public class CSharpExpressionInterpolationTests
{
	[Theory]
	// Issue #36155 repro: ternary directly inside an interpolation hole must be parenthesized.
	[InlineData(
		"{$'You clicked {Count} {Count == 1 ? 'time' : 'times'}.'}",
		"$\"You clicked {Count} {(Count == 1 ? \"time\" : \"times\")}.\"")]
	// Ternary using a comparison operator inside a hole.
	[InlineData(
		"{$'{Count > 0 ? 'some' : 'none'}'}",
		"$\"{(Count > 0 ? \"some\" : \"none\")}\"")]
	// CDATA-style interpolation already using double quotes is parenthesized too.
	[InlineData(
		"{$\"You clicked {Count} {Count == 1 ? \"time\" : \"times\"}.\"}",
		"$\"You clicked {Count} {(Count == 1 ? \"time\" : \"times\")}.\"")]
	public void TernaryInsideInterpolationHole_IsParenthesized(string input, string expected)
	{
		Assert.Equal(expected, CSharpExpressionHelpers.GetExpressionCode(input));
	}

	[Theory]
	// Simple holes are untouched.
	[InlineData("{$'Hello {FirstName}'}", "$\"Hello {FirstName}\"")]
	[InlineData("{$'{FirstName} {LastName}'}", "$\"{FirstName} {LastName}\"")]
	// Format specifiers must be preserved (the ':' is NOT a ternary).
	[InlineData("{$'{Price:C2}'}", "$\"{Price:C2}\"")]
	[InlineData("{$'{Quantity}x at {Price:C2}'}", "$\"{Quantity}x at {Price:C2}\"")]
	// Null-coalescing inside a hole must not be mistaken for a ternary.
	[InlineData("{$'{Name ?? 'Unknown'}'}", "$\"{Name ?? \"Unknown\"}\"")]
	// Null-conditional inside a hole must not be mistaken for a ternary.
	[InlineData("{$'{User?.Name}'}", "$\"{User?.Name}\"")]
	public void NonTernaryHoles_AreNotParenthesized(string input, string expected)
	{
		Assert.Equal(expected, CSharpExpressionHelpers.GetExpressionCode(input));
	}

	[Theory]
	// A ternary at the top level (NOT inside an interpolation hole) is left as-is;
	// only quote translation applies.
	[InlineData("{IsVip ? 'Gold' : 'Standard'}", "IsVip ? \"Gold\" : \"Standard\"")]
	[InlineData("{Value ?? 'Default'}", "Value ?? \"Default\"")]
	public void TopLevelTernary_IsNotParenthesized(string input, string expected)
	{
		Assert.Equal(expected, CSharpExpressionHelpers.GetExpressionCode(input));
	}

	[Fact]
	// An already-parenthesized ternary with an explicit format specifier is preserved
	// (no double wrapping, format specifier kept).
	public void ParenthesizedTernaryWithFormat_IsPreserved()
	{
		var input = "{$'{(Count == 1 ? 1 : 2):D2}'}";
		var expected = "$\"{(Count == 1 ? 1 : 2):D2}\"";
		Assert.Equal(expected, CSharpExpressionHelpers.GetExpressionCode(input));
	}
}
