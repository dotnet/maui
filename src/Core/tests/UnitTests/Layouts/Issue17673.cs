using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using NSubstitute;
using Xunit;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class Issue17673
	{
		const string IssueNumber = "17673";

		[Fact]
		public void ProportionalChildrenPreserveNaturalSizeWhenUnconstrained()
		{
			if (!string.Equals(
				Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE"),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var layout = Substitute.For<IAbsoluteLayout>();
			layout.Height.Returns(Dimension.Unset);
			layout.Width.Returns(Dimension.Unset);
			layout.MinimumHeight.Returns(Dimension.Minimum);
			layout.MinimumWidth.Returns(Dimension.Minimum);
			layout.MaximumHeight.Returns(Dimension.Maximum);
			layout.MaximumWidth.Returns(Dimension.Maximum);

			var firstChildSize = new Size(120, 48);
			var secondChildSize = new Size(90, 60);
			var firstChild = CreateTestView(firstChildSize);
			var secondChild = CreateTestView(secondChildSize);
			SubstituteChildren(layout, firstChild, secondChild);

			var proportionalBounds = new Rect(0, 0, 1, 1);
			layout.GetLayoutBounds(firstChild).Returns(proportionalBounds);
			layout.GetLayoutBounds(secondChild).Returns(proportionalBounds);
			layout.GetLayoutFlags(firstChild).Returns(AbsoluteLayoutFlags.All);
			layout.GetLayoutFlags(secondChild).Returns(AbsoluteLayoutFlags.All);

			var measuredSize = new AbsoluteLayoutManager(layout).Measure(
				double.PositiveInfinity,
				double.PositiveInfinity);
			var requiredSize = new Size(
				Math.Max(firstChildSize.Width, secondChildSize.Width),
				Math.Max(firstChildSize.Height, secondChildSize.Height));

			Assert.True(
				measuredSize.Width >= requiredSize.Width && measuredSize.Height >= requiredSize.Height,
				"AbsoluteLayout should not shrink below the natural size of its proportional children.");
		}
	}
}
