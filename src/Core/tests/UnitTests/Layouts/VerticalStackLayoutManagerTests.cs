using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;
using NSubstitute;
using Xunit;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class VerticalStackLayoutManagerTests : StackLayoutManagerTests
	{
		[Theory]
		[InlineData(0, 100, 0, 0)]
		[InlineData(1, 100, 0, 100)]
		[InlineData(1, 100, 13, 100)]
		[InlineData(2, 100, 13, 213)]
		[InlineData(3, 100, 13, 326)]
		[InlineData(3, 100, -13, 274)]
		public void SpacingMeasurement(int viewCount, double viewHeight, int spacing, double expectedHeight)
		{
			var stack = BuildStack(viewCount, 100, viewHeight);
			stack.Spacing.Returns(spacing);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(100, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measuredSize.Height);
		}

		[Theory("Spacing has no effect when there's only one item")]
		[InlineData(0), InlineData(26), InlineData(-54)]
		public void SpacingArrangementOneItem(int spacing)
		{
			var stack = BuildStack(1, 100, 100);
			stack.Spacing.Returns(spacing);

			var manager = new VerticalStackLayoutManager(stack);

			var measuredSize = manager.Measure(100, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measuredSize));

			var expectedRectangle = new Rect(0, 0, 100, 100);
			stack[0].Received().Arrange(Arg.Is(expectedRectangle));
		}

		[Theory("Spacing has an effect when there's more than one item")]
		[InlineData(26), InlineData(-54)]
		public void SpacingArrangementTwoItems(int spacing)
		{
			var stack = BuildStack(2, 100, 100);
			stack.Spacing.Returns(spacing);

			var manager = new VerticalStackLayoutManager(stack);

			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.ArrangeChildren(new Rect(Point.Zero, measuredSize));

			AssertArranged(stack[0], 0, 0, 100, 100);
			AssertArranged(stack[1], 0, 100 + spacing, 100, 100);
		}

		[Theory]
		[InlineData(150, 100, 100)]
		[InlineData(150, 200, 200)]
		[InlineData(1250, Dimension.Unset, 1250)]
		public void StackAppliesHeight(double viewHeight, double stackHeight, double expectedHeight)
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, viewHeight));

			var stack = CreateTestLayout(new List<IView>() { view });

			stack.Height.Returns(stackHeight);

			var manager = new VerticalStackLayoutManager(stack);
			var measurement = manager.Measure(100, double.PositiveInfinity);
			Assert.Equal(expectedHeight, measurement.Height);
		}

		[Fact]
		public void IgnoresCollapsedViews()
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var collapsedView = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			collapsedView.Visibility.Returns(Visibility.Collapsed);

			var stack = CreateTestLayout(new List<IView>() { view, collapsedView });

			var manager = new VerticalStackLayoutManager(stack);
			var measure = manager.Measure(100, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measure));

			// View is visible, so we expect it to be measured and arranged
			view.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			view.Received().Arrange(Arg.Any<Rect>());

			// View is collapsed, so we expect it not to be measured or arranged
			collapsedView.DidNotReceive().Measure(Arg.Any<double>(), Arg.Any<double>());
			collapsedView.DidNotReceive().Arrange(Arg.Any<Rect>());
		}

		[Fact]
		public void DoesNotIgnoreHiddenViews()
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var hiddenView = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			hiddenView.Visibility.Returns(Visibility.Hidden);

			var stack = CreateTestLayout(new List<IView>() { view, hiddenView });

			var manager = new VerticalStackLayoutManager(stack);
			var measure = manager.Measure(100, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measure));

			// View is visible, so we expect it to be measured and arranged
			view.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			view.Received().Arrange(Arg.Any<Rect>());

			// View is hidden, so we expect it to be measured and arranged (since it'll need to take up space)
			hiddenView.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			hiddenView.Received().Arrange(Arg.Any<Rect>());
		}

		IStackLayout BuildPaddedStack(Thickness padding, double viewWidth, double viewHeight)
		{
			var stack = BuildStack(1, viewWidth, viewHeight);
			stack.Padding.Returns(padding);
			return stack;
		}

		[Theory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(10, 10, 10, 10)]
		[InlineData(10, 0, 10, 0)]
		[InlineData(0, 10, 0, 10)]
		[InlineData(23, 5, 3, 15)]
		public void MeasureAccountsForPadding(double left, double top, double right, double bottom)
		{
			var viewWidth = 100d;
			var viewHeight = 100d;
			var padding = new Thickness(left, top, right, bottom);

			var expectedHeight = padding.VerticalThickness + viewHeight;
			var expectedWidth = padding.HorizontalThickness + viewWidth;

			var stack = BuildPaddedStack(padding, viewWidth, viewHeight);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measuredSize.Height);
			Assert.Equal(expectedWidth, measuredSize.Width);
		}

		[Theory]
		[InlineData(0, 0, 0, 0)]
		[InlineData(10, 10, 10, 10)]
		[InlineData(10, 0, 10, 0)]
		[InlineData(0, 10, 0, 10)]
		[InlineData(23, 5, 3, 15)]
		public void ArrangeAccountsForPadding(double left, double top, double right, double bottom)
		{
			var viewWidth = 100d;
			var viewHeight = 100d;
			var padding = new Thickness(left, top, right, bottom);

			var stack = BuildPaddedStack(padding, viewWidth, viewHeight);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rect(Point.Zero, measuredSize));

			AssertArranged(stack[0], padding.Left, padding.Top, viewWidth, viewHeight);
		}

		[Fact]
		public void ArrangeRespectsBounds()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.ArrangeChildren(new Rect(new Point(10, 15), measuredSize));

			var expectedRectangle0 = new Rect(10, 15, 100, 100);

			stack[0].Received().Arrange(Arg.Is(expectedRectangle0));
		}

		[Theory]
		[InlineData(50, 100, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(100, 50, 50)]
		[InlineData(0, 50, 0)]
		public void MeasureRespectsMaxHeight(double maxHeight, double viewHeight, double expectedHeight)
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: viewHeight);
			stack.MaximumHeight.Returns(maxHeight);

			var layoutManager = new VerticalStackLayoutManager(stack);
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
			var stack = BuildStack(viewCount: 1, viewWidth: viewWidth, viewHeight: 100);

			stack.MaximumWidth.Returns(maxWidth);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Theory]
		[InlineData(50, 10, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(10, 50, 50)]
		public void MeasureRespectsMinHeight(double minHeight, double viewHeight, double expectedHeight)
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: viewHeight);

			stack.MinimumHeight.Returns(minHeight);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measure.Height);
		}

		[Theory]
		[InlineData(50, 10, 50)]
		[InlineData(100, 100, 100)]
		[InlineData(10, 50, 50)]
		public void MeasureRespectsMinWidth(double minWidth, double viewWidth, double expectedWidth)
		{
			var stack = BuildStack(viewCount: 1, viewWidth: viewWidth, viewHeight: 100);

			stack.MinimumWidth.Returns(minWidth);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Fact]
		public void MaxWidthDominatesWidth()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			stack.Width.Returns(75);
			stack.MaximumWidth.Returns(50);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The maximum value beats out the explicit value
			Assert.Equal(50, measure.Width);
		}

		[Fact]
		public void MinWidthDominatesMaxWidth()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			stack.MinimumWidth.Returns(75);
			stack.MaximumWidth.Returns(50);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The minimum value should beat out the maximum value
			Assert.Equal(75, measure.Width);
		}

		[Fact]
		public void MaxHeightDominatesHeight()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			stack.Height.Returns(75);
			stack.MaximumHeight.Returns(50);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The maximum value beats out the explicit value
			Assert.Equal(50, measure.Height);
		}

		[Fact]
		public void MinHeightDominatesMaxHeight()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			stack.MinimumHeight.Returns(75);
			stack.MaximumHeight.Returns(50);

			var gridLayoutManager = new VerticalStackLayoutManager(stack);
			var measure = gridLayoutManager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			// The minimum value should beat out the maximum value
			Assert.Equal(75, measure.Height);
		}

		[Fact]
		public void ArrangeAccountsForFill()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			stack.HorizontalLayoutAlignment.Returns(LayoutAlignment.Fill);
			stack.VerticalLayoutAlignment.Returns(LayoutAlignment.Fill);

			var layoutManager = new VerticalStackLayoutManager(stack);
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

		public static IEnumerable<object[]> ChildMeasureAccountsForPaddingTestCases()
		{
			var measureSpace = new Size(100, 100);
			var viewSize = new Size(50, 50);

			yield return new object[] { viewSize, new Thickness(0), measureSpace, new Size(100, double.PositiveInfinity) };
			yield return new object[] { viewSize, new Thickness(10), measureSpace, new Size(80, double.PositiveInfinity) };
			yield return new object[] { viewSize, new Thickness(0, 10, 0, 10), measureSpace, new Size(100, double.PositiveInfinity) };
			yield return new object[] { viewSize, new Thickness(7, 0, 14, 0), measureSpace, new Size(79, double.PositiveInfinity) };
		}

		[Theory]
		[MemberData(nameof(ChildMeasureAccountsForPaddingTestCases))]
		public void ChildMeasureAccountsForPadding(Size viewSize, Thickness padding, Size measureConstraints, Size expectedMeasureConstraint)
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(viewSize.Width, viewSize.Height));
			var stack = CreateTestLayout(new List<IView>() { view });
			stack.Padding.Returns(padding);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(measureConstraints.Width, measureConstraints.Height);

			view.Received().Measure(Arg.Is(expectedMeasureConstraint.Width), Arg.Is(expectedMeasureConstraint.Height));
		}

		[Fact]
		public void CollapsedItemsDoNotIncurSpacingInMiddle()
		{
			var viewWidth = 7;
			var viewHeight = 5;
			var spacing = 10;

			var stack = SetUpVisibilityTestStack(viewWidth, viewHeight, spacing);
			stack.Spacing.Returns(spacing);

			stack[1].Visibility.Returns(Visibility.Collapsed);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var expectedHeight = viewHeight + spacing + viewHeight;

			Assert.Equal(expectedHeight, measuredSize.Height);
		}

		[Fact]
		public void CollapsedItemsDoNotIncurSpacingAtEnd()
		{
			var viewWidth = 7;
			var viewHeight = 5;
			var spacing = 10;

			var stack = SetUpVisibilityTestStack(viewWidth, viewHeight, spacing);
			stack.Spacing.Returns(spacing);

			stack[2].Visibility.Returns(Visibility.Collapsed);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var expectedHeight = viewHeight + spacing + viewHeight;

			Assert.Equal(expectedHeight, measuredSize.Height);
		}

		[Fact]
		public void CollapsedItemsDoNotIncurSpacingAtStart()
		{
			var viewWidth = 7;
			var viewHeight = 5;
			var spacing = 10;

			var stack = SetUpVisibilityTestStack(viewWidth, viewHeight, spacing);
			stack.Spacing.Returns(spacing);

			stack[0].Visibility.Returns(Visibility.Collapsed);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var expectedHeight = viewHeight + spacing + viewHeight;

			Assert.Equal(expectedHeight, measuredSize.Height);
		}

		[Fact]
		public void LayoutWithAllCollapsedItemsHasNoSpacing()
		{
			var viewWidth = 7;
			var viewHeight = 5;
			var spacing = 10;

			var stack = SetUpVisibilityTestStack(viewWidth, viewHeight, spacing);
			stack.Spacing.Returns(spacing);

			stack[0].Visibility.Returns(Visibility.Collapsed);
			stack[1].Visibility.Returns(Visibility.Collapsed);
			stack[2].Visibility.Returns(Visibility.Collapsed);

			var manager = new VerticalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var expectedHeight = 0;

			Assert.Equal(expectedHeight, measuredSize.Height);
		}
	}
}
