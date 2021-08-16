using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.UnitTests;
using Microsoft.Maui.UnitTests.Layouts;
using NSubstitute;
using Xunit;
using static Microsoft.Maui.UnitTests.Layouts.LayoutTestHelpers;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class ZStackLayoutManagerTests : StackLayoutManagerTests
	{
		[Fact]
		public void MeasureWidthEqualsWidestChild()
		{
			var expectedWidth = 200;

			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var viewNarrow = LayoutTestHelpers.CreateTestView(new Size(50, 100));
			var viewWide = LayoutTestHelpers.CreateTestView(new Size(expectedWidth, 100));

			var stack = CreateTestLayout(new List<IView>() { view, viewNarrow, viewWide });

			var manager = new ZStackLayoutManager(stack);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedWidth, measure.Width);
		}

		[Fact]
		public void MeasureHeightEqualsTallestChild()
		{
			var expectedHeight = 200;

			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var viewShort = LayoutTestHelpers.CreateTestView(new Size(100, 50));
			var viewTall = LayoutTestHelpers.CreateTestView(new Size(100, expectedHeight));

			var stack = CreateTestLayout(new List<IView>() { view, viewTall, viewShort });

			var manager = new ZStackLayoutManager(stack);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(expectedHeight, measure.Height);
		}

		[Fact]
		public void IgnoresCollapsedViews()
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var collapsedView = LayoutTestHelpers.CreateTestView(new Size(200, 200));
			collapsedView.Visibility.Returns(Visibility.Collapsed);

			var stack = CreateTestLayout(new List<IView>() { view, collapsedView });

			var manager = new ZStackLayoutManager(stack);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rectangle(Point.Zero, measure));

			// View is visible, so we expect it to be measured and arranged
			view.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			view.Received().Arrange(Arg.Any<Rectangle>());

			// View is collapsed, so we expect it not to be measured or arranged
			collapsedView.DidNotReceive().Measure(Arg.Any<double>(), Arg.Any<double>());
			collapsedView.DidNotReceive().Arrange(Arg.Any<Rectangle>());
		}

		[Fact]
		public void DoesNotIgnoreHiddenViews()
		{
			var view = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			var hiddenView = LayoutTestHelpers.CreateTestView(new Size(100, 100));
			hiddenView.Visibility.Returns(Visibility.Hidden);

			var stack = CreateTestLayout(new List<IView>() { view, hiddenView });

			var manager = new ZStackLayoutManager(stack);
			var measure = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.ArrangeChildren(new Rectangle(Point.Zero, measure));

			// View is visible, so we expect it to be measured and arranged
			view.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			view.Received().Arrange(Arg.Any<Rectangle>());

			// View is hidden, so we expect it to be measured and arranged (since it'll need to take up space)
			hiddenView.Received().Measure(Arg.Any<double>(), Arg.Any<double>());
			hiddenView.Received().Arrange(Arg.Any<Rectangle>());
		}

		[Fact]
		public void ArrangeRespectsBounds()
		{
			var stack = BuildStack(viewCount: 1, viewWidth: 100, viewHeight: 100);

			var manager = new ZStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.ArrangeChildren(new Rectangle(new Point(10, 15), measuredSize));

			var expectedRectangle0 = new Rectangle(10, 15, 100, 100);

			stack[0].Received().Arrange(Arg.Is(expectedRectangle0));
		}
	}
}
