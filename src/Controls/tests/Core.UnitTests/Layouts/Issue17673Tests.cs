using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category("Layout")]
	public class Issue17673 : BaseTestFixture
	{
		[Fact]
		public void ProportionalChildrenRetainDesiredSizeWhenUnconstrained()
		{
			const double desiredHeight = 20;
			var bottom = MockPlatformSizeService.Sub<View>();
			var top = MockPlatformSizeService.Sub<View>();
			var layout = new AbsoluteLayout
			{
				bottom,
				top
			};

			AbsoluteLayout.SetLayoutBounds(bottom, new Rect(0, 0, 1, 1));
			AbsoluteLayout.SetLayoutFlags(bottom, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(top, new Rect(0, 0, 1, 1));
			AbsoluteLayout.SetLayoutFlags(top, AbsoluteLayoutFlags.All);

			var measuredSize = ((Microsoft.Maui.ILayout)layout).CrossPlatformMeasure(
				double.PositiveInfinity,
				double.PositiveInfinity);

			Assert.True(
				measuredSize.Height >= desiredHeight,
				"AbsoluteLayout should not shrink below the desired size of proportional children.");
		}
	}
}
