using System;
using System.Collections.Generic;
using System.Xml.Xsl;
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
	public class AbsoluteLayoutManagerTests
	{
		IAbsoluteLayout CreateTestLayout()
		{
			var layout = Substitute.For<IAbsoluteLayout>();
			layout.Height.Returns(Dimension.Unset);
			layout.Width.Returns(Dimension.Unset);
			layout.MinimumHeight.Returns(Dimension.Minimum);
			layout.MinimumWidth.Returns(Dimension.Minimum);
			layout.MaximumHeight.Returns(Dimension.Maximum);
			layout.MaximumWidth.Returns(Dimension.Maximum);

			return layout;
		}

		void SetLayoutBounds(IAbsoluteLayout layout, IView child, Rect bounds)
		{
			layout.GetLayoutBounds(child).Returns(bounds);
		}

		void SetLayoutFlags(IAbsoluteLayout layout, IView child, AbsoluteLayoutFlags flags)
		{
			layout.GetLayoutFlags(child).Returns(flags);
		}

		Size MeasureAndArrange(IAbsoluteLayout absoluteLayout, double widthConstraint = double.PositiveInfinity, double heightConstraint = double.PositiveInfinity, double left = 0, double top = 0)
		{
			var manager = new AbsoluteLayoutManager(absoluteLayout);
			var measuredSize = manager.Measure(widthConstraint, heightConstraint);
			manager.ArrangeChildren(new Rect(new Point(left, top), measuredSize));

			return measuredSize;
		}

		static double AutoSize = -1;
		static Rect DefaultBounds = new Rect(0, 0, AutoSize, AutoSize);

		[Fact]
		public void DefaultLayoutBoundsUsesDefaultMeasure()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView(new Size(50, 75));

			// Have GetLayoutBounds return the "default" value of 0,0,-1,-1
			abs.GetLayoutBounds(child).Returns(DefaultBounds);

			var expectedRectangle = new Rect(0, 0, 50, 75);

			SubstituteChildren(abs, child);

			var measure = MeasureAndArrange(abs, double.PositiveInfinity, double.PositiveInfinity);

			// No layout bounds were specified, so plain-old measuring should be used. The view wants to be 50 by 75, and so it shall
			Assert.Equal(expectedRectangle.Size, measure);
			AssertArranged(child, expectedRectangle);
		}

		[Fact]
		public void AbsolutePositionAndSize()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();

			var expectedRectangle = new Rect(10, 15, 100, 100);

			SubstituteChildren(abs, child);
			SetLayoutBounds(abs, child, expectedRectangle);

			var measure = MeasureAndArrange(abs, double.PositiveInfinity, double.PositiveInfinity);

			// We expect that the measured size will be big enough to include the whole child rectangle at its offset
			var expectedSize = new Size(expectedRectangle.Left + expectedRectangle.Width, expectedRectangle.Top + expectedRectangle.Height);

			Assert.Equal(expectedSize, measure);
			AssertArranged(child, expectedRectangle);
		}

		[Fact]
		public void AbsoluteLayoutRespectsBounds()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();

			var childBounds = new Rect(10, 15, 100, 100);

			SubstituteChildren(abs, child);
			SetLayoutBounds(abs, child, childBounds);

			var measure = MeasureAndArrange(abs, double.PositiveInfinity, double.PositiveInfinity, left: 10, top: 10);

			// We expect that the measured size will be big enough to include the whole child rectangle at its offset
			var expectedSize = new Size(childBounds.Left + childBounds.Width, childBounds.Top + childBounds.Height);

			// We expect that the child will be arranged at a spot that respects the offsets
			var expectedRectangle = new Rect(childBounds.Left + 10, childBounds.Top + 10, childBounds.Width, childBounds.Height);

			Assert.Equal(expectedSize, measure);
			AssertArranged(child, expectedRectangle);
		}

		[Fact]
		public void AbsolutePositionRelativeSize()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);

			var childBounds = new Rect(10, 20, 0.4, 0.5);
			SetLayoutBounds(abs, child, childBounds);
			SetLayoutFlags(abs, child, AbsoluteLayoutFlags.SizeProportional);

			var manager = new AbsoluteLayoutManager(abs);

			var measure = manager.Measure(100, 100);
			manager.ArrangeChildren(new Rect(0, 0, 100, 100));

			var expectedMeasure = new Size(10 + 40, 20 + 50);

			Assert.Equal(expectedMeasure, measure);
			child.Received().Arrange(Arg.Is<Rect>(r => r.X == 10 && r.Y == 20 && r.Width == 40 && r.Height == 50));
		}

		[InlineData(30, 40, 0.2, 0.3)]
		[InlineData(35, 45, 0.5, 0.5)]
		[InlineData(35, 45, 0, 0)]
		[InlineData(35, 45, 1, 1)]
		[Theory]
		public void RelativePositionAbsoluteSize(double width, double height, double propX, double propY)
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);

			var childBounds = new Rect(propX, propY, width, height);

			SetLayoutBounds(abs, child, childBounds);
			SetLayoutFlags(abs, child, AbsoluteLayoutFlags.PositionProportional);

			var manager = new AbsoluteLayoutManager(abs);

			manager.Measure(100, 100);
			manager.ArrangeChildren(new Rect(0, 0, 100, 100));

			double expectedX = (100 - width) * propX;
			double expectedY = (100 - height) * propY;

			var expectedRectangle = new Rect(expectedX, expectedY, width, height);
			child.Received().Arrange(Arg.Is(expectedRectangle));
		}

		[InlineData(0.0, 0.0, 0.0, 0.0)]
		[InlineData(0.2, 0.2, 0.2, 0.2)]
		[InlineData(0.5, 0.5, 0.5, 0.5)]
		[InlineData(1.0, 1.0, 1.0, 1.0)]
		[Theory]
		public void RelativePositionRelativeSize(double propX, double propY, double propHeight, double propWidth)
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(propX, propY, propWidth, propHeight);
			SetLayoutBounds(abs, child, childBounds);
			SetLayoutFlags(abs, child, AbsoluteLayoutFlags.All);

			var manager = new AbsoluteLayoutManager(abs);
			manager.Measure(100, 100);
			manager.ArrangeChildren(new Rect(0, 0, 100, 100));

			double expectedWidth = 100 * propWidth;
			double expectedHeight = 100 * propHeight;
			double expectedX = (100 - expectedWidth) * propX;
			double expectedY = (100 - expectedHeight) * propY;

			var expectedRectangle = new Rect(expectedX, expectedY, expectedWidth, expectedHeight);
			child.Received().Arrange(Arg.Is(expectedRectangle));
		}

		[Theory]
		[InlineData(0, 0, 0, 0, 40, 40)]
		[InlineData(5, 5, 5, 5, 40, 40)]
		[InlineData(10, 10, 0, 0, 45, 45)]
		public void RelativePositionRespectsPadding(double left, double top,
			double right, double bottom, double expectedX, double expectedY)
		{
			double width = 20;
			double height = 20;
			double propX = 0.5;
			double propY = 0.5;

			var abs = CreateTestLayout();

			var padding = new Thickness(left, top, right, bottom);
			abs.Padding.Returns(padding);

			var child = CreateTestView();
			SubstituteChildren(abs, child);

			var childBounds = new Rect(propX, propY, width, height);

			SetLayoutBounds(abs, child, childBounds);
			SetLayoutFlags(abs, child, AbsoluteLayoutFlags.PositionProportional);

			var manager = new AbsoluteLayoutManager(abs);

			manager.Measure(100, 100);
			manager.ArrangeChildren(new Rect(0, 0, 100, 100));

			var expectedRectangle = new Rect(expectedX, expectedY, width, height);
			child.Received().Arrange(Arg.Is(expectedRectangle));
		}

		[Theory]
		[InlineData(50, 100, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(100, 50, 50)]
		[InlineData(0, 50, 0)]
		public void MeasureRespectsMaxHeight(double maxHeight, double viewHeight, double expectedHeight)
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, viewHeight);
			SetLayoutBounds(abs, child, childBounds);

			abs.MaximumHeight.Returns(maxHeight);

			var layoutManager = new AbsoluteLayoutManager(abs);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measure.Height);
		}

		[Theory]
		[InlineData(50, 100, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(100, 50, 50)]
		[InlineData(0, 50, 0)]
		public void MeasureRespectsMaxWidth(double maxWidth, double viewWidth, double expectedWidth)
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, viewWidth, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.MaximumWidth.Returns(maxWidth);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Theory]
		[InlineData(50, 10, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(10, 50, 50)]
		public void MeasureRespectsMinHeight(double minHeight, double viewHeight, double expectedHeight)
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, viewHeight);
			SetLayoutBounds(abs, child, childBounds);

			abs.MinimumHeight.Returns(minHeight);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measure.Height);
		}

		[Theory]
		[InlineData(50, 10, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(10, 50, 50)]
		public void MeasureRespectsMinWidth(double minWidth, double viewWidth, double expectedWidth)
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, viewWidth, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.MinimumWidth.Returns(minWidth);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Fact]
		public void MaxWidthDominatesWidth()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.Width.Returns(75);
			abs.MaximumWidth.Returns(50);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The maximum value beats out the explicit value
			Assert.Equal(50, measure.Width);
		}

		[Fact]
		public void MinWidthDominatesMaxWidth()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.MinimumWidth.Returns(75);
			abs.MaximumWidth.Returns(50);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The minimum value should beat out the maximum value
			Assert.Equal(75, measure.Width);
		}

		[Fact]
		public void MaxHeightDominatesHeight()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.Height.Returns(75);
			abs.MaximumHeight.Returns(50);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The maximum value beats out the explicit value
			Assert.Equal(50, measure.Height);
		}

		[Fact]
		public void MinHeightDominatesMaxHeight()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.MinimumHeight.Returns(75);
			abs.MaximumHeight.Returns(50);

			var layoutManager = new AbsoluteLayoutManager(abs);
			var measure = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The minimum value should beat out the maximum value
			Assert.Equal(75, measure.Height);
		}

		[Fact]
		public void ArrangeAccountsForFill()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, 100, 100);
			SetLayoutBounds(abs, child, childBounds);

			var layoutManager = new AbsoluteLayoutManager(abs);
			_ = layoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var arrangedWidth = 1000;
			var arrangedHeight = 1000;

			var target = new Rect(Point.Zero, new Size(arrangedWidth, arrangedHeight));

			var actual = layoutManager.ArrangeChildren(target);

			// Since we're arranging in a space larger than needed and the layout is set to Fill in both directions,
			// we expect the returned actual arrangement size to be as large as the target space
			Assert.Equal(arrangedWidth, actual.Width);
			Assert.Equal(arrangedHeight, actual.Height);
		}

		[Fact]
		public void ChildMeasureRespectsAbsoluteBounds()
		{
			double expectedWidth = 115;
			double expectedHeight = 230;

			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, expectedWidth, expectedHeight);
			SetLayoutBounds(abs, child, childBounds);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			child.Received().Measure(Arg.Is(expectedWidth), Arg.Is(expectedHeight));
		}

		[Fact]
		public void ChildMeasureRespectsProportionalBounds()
		{
			double expectedWidth = 0.5;
			double expectedHeight = 0.6;

			double widthConstraint = 200;
			double heightConstraint = 200;

			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(0, 0, expectedWidth, expectedHeight);
			SetLayoutBounds(abs, child, childBounds);
			SetLayoutFlags(abs, child, AbsoluteLayoutFlags.SizeProportional);

			var gridLayoutManager = new AbsoluteLayoutManager(abs);
			var measure = gridLayoutManager.Measure(widthConstraint, heightConstraint);

			child.Received().Measure(Arg.Is(expectedWidth * widthConstraint), Arg.Is(expectedHeight * heightConstraint));
		}

		[Fact(DisplayName = "First View in LTR Absolute Layout is on the left")]
		public void LtrShouldHaveFirstItemOnTheLeft()
		{
			var abs = CreateTestLayout();
			var child = CreateTestView();
			SubstituteChildren(abs, child);
			var childBounds = new Rect(10, 0, 100, 100);
			SetLayoutBounds(abs, child, childBounds);

			abs.FlowDirection.Returns(FlowDirection.LeftToRight);

			var manager = new AbsoluteLayoutManager(abs);
			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.ArrangeChildren(new Rect(Point.Zero, measuredSize));

			// We expect that the view should be arranged on the left
			var expectedRectangle = new Rect(10, 0, 100, 100);

			abs[0].Received().Arrange(Arg.Is(expectedRectangle));
		}
	}
}
