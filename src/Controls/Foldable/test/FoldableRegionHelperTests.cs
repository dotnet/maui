using System;
using Microsoft.Maui.Foldable;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	public class FoldableRegionHelperTests
	{
		[Fact]
		public void ActiveVerticalDivisionRegionIsSelected()
		{
			var regions = new[]
			{
				new FoldableRegion(new Rect(480, 0, 40, 700), true),
			};

			var result = FoldableRegionHelper.GetActiveDivisionRegion(regions, new Rect(0, 0, 1000, 700));

			Assert.Equal(regions[0].Bounds, result);
		}

		[Fact]
		public void ActiveHorizontalDivisionRegionIsSelected()
		{
			var regions = new[]
			{
				new FoldableRegion(new Rect(0, 330, 1000, 40), true),
			};

			var result = FoldableRegionHelper.GetActiveDivisionRegion(regions, new Rect(0, 0, 1000, 700));

			Assert.Equal(regions[0].Bounds, result);
		}

		[Fact]
		public void InactiveDivisionRegionDoesNotDriveLayout()
		{
			var activeRegions = new[]
			{
				new FoldableRegion(new Rect(480, 0, 40, 700), true),
			};
			var inactiveRegions = new[]
			{
				new FoldableRegion(new Rect(480, 0, 40, 700), false),
			};
			var viewBounds = new Rect(0, 0, 1000, 700);

			var activeResult = FoldableRegionHelper.GetActiveDivisionRegion(activeRegions, viewBounds);
			var inactiveResult = FoldableRegionHelper.GetActiveDivisionRegion(inactiveRegions, viewBounds);

			Assert.Equal(activeRegions[0].Bounds, activeResult);
			Assert.Equal(Rect.Zero, inactiveResult);
		}

		[Theory]
		[InlineData(0, 0, 40, 700)]
		[InlineData(960, 0, 40, 700)]
		[InlineData(0, 0, 1000, 40)]
		[InlineData(0, 660, 1000, 40)]
		public void EdgeRegionDoesNotSplitView(double x, double y, double width, double height)
		{
			var regions = new[]
			{
				new FoldableRegion(new Rect(x, y, width, height), true),
			};

			var result = FoldableRegionHelper.GetActiveDivisionRegion(regions, new Rect(0, 0, 1000, 700));

			Assert.Equal(Rect.Zero, result);
		}

		[Theory]
		[InlineData(0, 0)]
		[InlineData(1.5707963267948966, 90)]
		[InlineData(3.141592653589793, 180)]
		public void HingeAnglesAreConvertedFromRadians(double radians, double degrees)
		{
			Assert.Equal(degrees, FoldableRegionHelper.RadiansToDegrees(radians), 10);
		}
	}
}
