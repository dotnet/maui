using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class RectTests
	{
		[Fact]
		public void EqualsApproximatelyReturnsTrueForIdenticalRects()
		{
			var rect = new Rect(10, 20, 30, 40);
			Assert.True(rect.EqualsApproximately(new Rect(10, 20, 30, 40), epsilon: 1e-9));
		}

		[Fact]
		public void EqualsApproximatelyAbsorbsUlpDifferences()
		{
			// Values from the dotnet/maui#35142 trace: border heights captured on consecutive
			// iOS layoutSubviews passes that differ by ~22 ULP.
			var a = new Rect(0, 0, 390, 556.00000063578295);
			var b = new Rect(0, 0, 390, 556.00000063578273);

			Assert.False(a.Equals(b)); // bit-exact equality treats them as different
			Assert.True(a.EqualsApproximately(b, epsilon: 1e-9));
		}

		[Fact]
		public void EqualsApproximatelyReturnsFalseWhenAnyComponentExceedsEpsilon()
		{
			var rect = new Rect(0, 0, 100, 100);
			const double epsilon = 1e-9;

			Assert.False(rect.EqualsApproximately(new Rect(0 + 2 * epsilon, 0, 100, 100), epsilon));
			Assert.False(rect.EqualsApproximately(new Rect(0, 0 + 2 * epsilon, 100, 100), epsilon));
			Assert.False(rect.EqualsApproximately(new Rect(0, 0, 100 + 2 * epsilon, 100), epsilon));
			Assert.False(rect.EqualsApproximately(new Rect(0, 0, 100, 100 + 2 * epsilon), epsilon));
		}

		[Fact]
		public void EqualsApproximatelyTreatsHalfEpsilonDifferenceAsEqual()
		{
			var rect = new Rect(0, 0, 100, 100);
			const double epsilon = 1e-9;

			Assert.True(rect.EqualsApproximately(new Rect(0, 0, 100, 100 + epsilon * 0.5), epsilon));
		}
	}
}
