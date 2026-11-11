using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ProgressBarTests : BaseTestFixture
	{
		[Fact]
		public void TestClamp()
		{
			ProgressBar bar = new ProgressBar();

			bar.Progress = 2;
			Assert.Equal(1, bar.Progress);

			bar.Progress = -1;
			Assert.Equal(0, bar.Progress);
		}

		// Confirms ProgressProperty's coerceValue reuses the original boxed value instead of reboxing
		// when the value is already in range, and still reboxes to the clamped value when it's not.
		[Fact]
		public void ProgressCoerceValueReusesBoxWhenAlreadyInRange()
		{
			var bar = new ProgressBar();
			object boxedValue = 0.5d;

			var result = ProgressBar.ProgressProperty.CoerceValue(bar, boxedValue);

			Assert.Same(boxedValue, result);
			Assert.Equal(0.5d, (double)result);
		}

		[Fact]
		public void ProgressCoerceValueReboxesWhenOutOfRange()
		{
			var bar = new ProgressBar();
			object boxedValue = 2d;

			var result = ProgressBar.ProgressProperty.CoerceValue(bar, boxedValue);

			Assert.NotSame(boxedValue, result);
			Assert.Equal(1d, (double)result);
		}

		[Fact]
		public void TestProgressTo()
		{
			var bar = AnimationReadyHandler.Prepare(new ProgressBar());

			bar.ProgressTo(0.8, 250, Easing.Linear);

			AssertEqualWithTolerance(0.8, bar.Progress, 0.001);
		}

		static void AssertEqualWithTolerance(double a, double b, double tolerance)
		{
			var diff = Math.Abs(a - b);
			Assert.True(diff <= tolerance);
		}
	}
}