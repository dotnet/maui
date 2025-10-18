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