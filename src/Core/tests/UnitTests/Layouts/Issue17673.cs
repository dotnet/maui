using System;
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
		public void ProportionalChildPreservesIntrinsicHeightWithUnboundedConstraint()
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

			var intrinsicSize = new Size(100, 40);
			var child = CreateTestView(intrinsicSize);
			SubstituteChildren(layout, child);
			layout.GetLayoutBounds(child).Returns(new Rect(0, 0, 1, 1));
			layout.GetLayoutFlags(child).Returns(AbsoluteLayoutFlags.All);

			var measuredSize = new AbsoluteLayoutManager(layout).Measure(300, double.PositiveInfinity);

			Assert.True(
				measuredSize.Height >= intrinsicSize.Height,
				"AbsoluteLayout should preserve the child's intrinsic height when proportional sizing is measured without a finite height constraint.");
		}
	}
}
