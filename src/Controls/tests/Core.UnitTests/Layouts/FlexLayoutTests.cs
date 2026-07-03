using System;
using System.Transactions;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category("Layout")]
	public class FlexLayoutTests
	{
		class TestImage : Image
		{
			public bool Passed { get; private set; }

			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				if (double.IsPositiveInfinity(widthConstraint) && double.IsPositiveInfinity(heightConstraint))
				{
					Passed = true;
				}

				return base.MeasureOverride(widthConstraint, heightConstraint);
			}
		}

		class TestLabel : Label
		{
			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				return new Size(150, 100);
			}
		}

		class FixedSizeLabel : Label
		{
			readonly double _width;
			readonly double _height;

			public FixedSizeLabel(double width, double height)
			{
				_width = width;
				_height = height;
			}

			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				return new Size(_width, _height);
			}
		}

		[Fact]
		public void FlexLayoutMeasuresImagesUnconstrained()
		{
			var root = new Grid();
			var flexLayout = new FlexLayout() as IFlexLayout;
			var image = new TestImage();

			root.Add(flexLayout);
			flexLayout.Add(image as IView);

			flexLayout.CrossPlatformMeasure(1000, 1000);

			Assert.True(image.Passed, "Image should be measured unconstrained even if the FlexLayout is constrained.");
		}

		[Fact]
		public void FlexLayoutRecognizesVisibilityChange()
		{
			var root = new Grid();
			var flexLayout = new FlexLayout() as IFlexLayout;
			var view = new TestLabel();
			var view2 = new TestLabel();

			root.Add(flexLayout);
			flexLayout.Add(view as IView);
			flexLayout.Add(view2 as IView);

			// Measure and arrange the layout while the first view is visible
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			// Keep track of where the second view is arranged
			var whenVisible = view2.Frame.X;

			// Change the visibility
			view.IsVisible = false;

			// Measure and arrange again
			measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var whenInvisible = view2.Frame.X;

			// The location of the second view should have changed
			// now that the first view is not visible
			Assert.True(whenInvisible != whenVisible);
		}

		/*
		 * These next two tests deal with unconstrained measure of FlexLayout. Be default, FL
		 * wants to stretch children across each axis. But you can't stretch things across infinity
		 * without it getting weird. So for _measurement_ purposes, we treat infinity as zero and 
		 * just give the children their desired size in the unconstrained direction. Otherwise, FL
		 * would just set their flex frame sizes to zero, which can either cause blanks or layout cycles,
		 * depending on the target platform.
		 */

		(IFlexLayout, IView) SetUpUnconstrainedTest(Action<FlexLayout> configure = null)
		{
			var root = new Grid(); // FlexLayout requires a parent, at least for now

			var controlsFlexLayout = new FlexLayout();
			configure?.Invoke(controlsFlexLayout);

			var flexLayout = controlsFlexLayout as IFlexLayout;

			var view = Substitute.For<IView>();
			var size = new Size(100, 100);
			view.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(size);

			root.Add(flexLayout);
			flexLayout.Add(view);

			return (flexLayout, view);
		}

		[Fact]
		public void UnconstrainedHeightChildrenHaveHeight()
		{
			(var flexLayout, var view) = SetUpUnconstrainedTest();

			_ = flexLayout.CrossPlatformMeasure(400, double.PositiveInfinity);

			var flexFrame = flexLayout.GetFlexFrame(view);

			Assert.Equal(100, flexFrame.Height);
		}

		[Fact]
		public void UnconstrainedWidthChildrenHaveWidth()
		{
			(var flexLayout, var view) = SetUpUnconstrainedTest();

			_ = flexLayout.CrossPlatformMeasure(double.PositiveInfinity, 400);

			var flexFrame = flexLayout.GetFlexFrame(view);

			Assert.Equal(100, flexFrame.Width);
		}

		[Fact]
		public void FlexLayoutPaddingShouldBeAppliedCorrectly_RowDirection()
		{
			// Arrange
			var padding = 16;
			var root = new Grid();
			var flexLayout = new FlexLayout { Padding = padding };
			var view1 = new TestLabel();
			var view2 = new TestLabel();

			root.Add(flexLayout);
			flexLayout.Add(view1 as IView);
			flexLayout.Add(view2 as IView);

			// Act
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var view1Frame = flexLayout.Children[0].Frame;
			var view2Frame = flexLayout.Children[1].Frame;

			var leftPadding = view1Frame.X;
			var topPadding = view1Frame.Y;
			var rightPadding = measure.Width - view2Frame.Right;
			var expectedView1Width = measure.Width - (leftPadding + rightPadding + view2.Width);
			var expectedView2Width = measure.Width - (leftPadding + rightPadding + view1.Width);

			// Assert
			Assert.Equal(padding, leftPadding);
			Assert.Equal(padding, rightPadding);
			Assert.Equal(padding, topPadding);
			Assert.Equal(expectedView1Width, view1Frame.Width);
			Assert.Equal(expectedView2Width, view2Frame.Width);
		}

		[Fact]
		public void FlexLayoutPaddingShouldBeAppliedCorrectly_ColumnDirection()
		{
			// Arrange
			var padding = 16;
			var root = new Grid();
			var flexLayout = new FlexLayout { Padding = padding, Direction = FlexDirection.Column };
			var view1 = new TestLabel();
			var view2 = new TestLabel();

			root.Add(flexLayout);
			flexLayout.Add(view1 as IView);
			flexLayout.Add(view2 as IView);

			// Act
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var view1Frame = flexLayout.Children[0].Frame;
			var view2Frame = flexLayout.Children[1].Frame;

			var bottomPadding = measure.Height - view2Frame.Bottom;
			var topPadding = view1Frame.Y;
			var expectedView1Height = measure.Height - (topPadding + bottomPadding + view2.Height);
			var expectedView2Height = measure.Height - (topPadding + bottomPadding + view1.Height);

			// Assert
			Assert.Equal(padding, bottomPadding);
			Assert.Equal(view2.Height, view2Frame.Height);
			Assert.Equal(expectedView1Height, view1Frame.Height);
			Assert.Equal(expectedView2Height, view2Frame.Height);
		}

		[Theory]
		[InlineData(double.PositiveInfinity, 400, FlexDirection.RowReverse)]
		[InlineData(double.PositiveInfinity, 400, FlexDirection.Row)]
		[InlineData(400, double.PositiveInfinity, FlexDirection.ColumnReverse)]
		[InlineData(400, double.PositiveInfinity, FlexDirection.Column)]
		public void UnconstrainedMeasureHonorsFlexDirection(double widthConstraint, double heightConstraint,
			FlexDirection flexDirection)
		{
			(var flexLayout, var view) = SetUpUnconstrainedTest((fl) => { fl.Direction = flexDirection; });

			_ = flexLayout.CrossPlatformMeasure(widthConstraint, heightConstraint);

			var flexFrame = flexLayout.GetFlexFrame(view);

			Assert.Equal(0, flexFrame.X);
			Assert.Equal(0, flexFrame.Y);
		}

		// Test view that respects WidthRequest/HeightRequest in MeasureOverride,
		// simulating real controls like Grid, StackLayout, etc.
		class FlexTestView : View
		{
			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				var w = WidthRequest >= 0 ? Math.Min(WidthRequest, widthConstraint) : Math.Min(100, widthConstraint);
				var h = HeightRequest >= 0 ? Math.Min(HeightRequest, heightConstraint) : Math.Min(50, heightConstraint);
				return new Size(w, h);
			}
		}

		// Regression test for https://github.com/dotnet/maui/issues/31109
		// Verifies that dynamically changing WidthRequest on a FlexLayout child
		// is correctly reflected during an arrange-only pass (no preceding measure).
		[Fact]
		public void ArrangeOnlyPassUsesUpdatedWidthRequest()
		{
			var root = new Grid();
			var flexLayout = new FlexLayout() { Direction = FlexDirection.Row };
			var view = new FlexTestView { WidthRequest = 200 };

			root.Add(flexLayout);
			(flexLayout as IFlexLayout).Add(view as IView);

			// Initial measure + arrange
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var initialFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(200, initialFrame.Width);

			// Change WidthRequest without re-measuring
			view.WidthRequest = 300;

			// Arrange-only pass (simulates Android arrange without preceding measure)
			flexLayout.CrossPlatformArrange(new Rect(0, 0, 1000, 1000));

			var updatedFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(300, updatedFrame.Width);
		}

		// Regression test for https://github.com/dotnet/maui/issues/31109
		// Verifies that changing HeightRequest during arrange-only pass works correctly.
		[Fact]
		public void ArrangeOnlyPassUsesUpdatedHeightRequest()
		{
			var root = new Grid();
			var flexLayout = new FlexLayout() { Direction = FlexDirection.Column };
			var view = new FlexTestView { HeightRequest = 80 };

			root.Add(flexLayout);
			(flexLayout as IFlexLayout).Add(view as IView);

			// Initial measure + arrange
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var initialFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(80, initialFrame.Height);

			// Change HeightRequest without re-measuring
			view.HeightRequest = 120;

			// Arrange-only pass
			flexLayout.CrossPlatformArrange(new Rect(0, 0, 1000, 1000));

			var updatedFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(120, updatedFrame.Height);
		}

		// Regression test for https://github.com/dotnet/maui/issues/31109
		// Verifies that children without explicit WidthRequest still use DesiredSize during arrange.
		[Fact]
		public void ArrangeOnlyPassUsesDesiredSizeWhenNoWidthRequest()
		{
			var root = new Grid();
			var flexLayout = new FlexLayout() { Direction = FlexDirection.Row };
			var view = new TestLabel(); // No WidthRequest set, MeasureOverride returns (150, 100)

			root.Add(flexLayout);
			(flexLayout as IFlexLayout).Add(view as IView);

			// Initial measure + arrange
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var initialFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(150, initialFrame.Width);

			// Arrange-only pass (should still use DesiredSize since no WidthRequest)
			flexLayout.CrossPlatformArrange(new Rect(0, 0, 1000, 1000));

			var afterFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(150, afterFrame.Width);
		}

		// Regression test for https://github.com/dotnet/maui/issues/31109
		// Verifies that clearing WidthRequest (setting to -1) falls back to DesiredSize during arrange.
		[Fact]
		public void ArrangeOnlyPassFallsBackToDesiredSizeWhenWidthRequestCleared()
		{
			var root = new Grid();
			var flexLayout = new FlexLayout() { Direction = FlexDirection.Row };
			var view = new FlexTestView { WidthRequest = 200 };

			root.Add(flexLayout);
			(flexLayout as IFlexLayout).Add(view as IView);

			// Initial measure + arrange with explicit WidthRequest
			var measure = flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(Point.Zero, measure));

			var initialFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(200, initialFrame.Width);

			// Clear WidthRequest and re-measure so DesiredSize reflects auto-sizing
			view.WidthRequest = -1;
			flexLayout.CrossPlatformMeasure(1000, 1000);
			flexLayout.CrossPlatformArrange(new Rect(0, 0, 1000, 1000));

			// Should fall back to the auto-sized DesiredSize (100 from FlexTestView default)
			var clearedFrame = (flexLayout as IFlexLayout).GetFlexFrame(view as IView);
			Assert.Equal(100, clearedFrame.Width);
		}
    
    [Fact]
		public void GrowItemsPreserveNaturalSizeAndDistributeFreeSpaceEqually_Issue34464()
		{
			// Items with different natural widths but equal Grow values should each receive
			// an equal share of the available free space added on top of their natural width.
			// Before the fix, the natural size was zeroed and the inflated flex_dim was
			// distributed proportionally, causing items with larger natural sizes to receive
			// less growth than smaller items (violating the flex-grow spec).
			var root = new Grid();
			var controlsFlexLayout = new FlexLayout();
			var flexLayout = controlsFlexLayout as IFlexLayout;

			// item1 is narrower (50px), item2 is wider (100px); both have equal Grow
			var item1 = new FixedSizeLabel(50, 50);
			var item2 = new FixedSizeLabel(100, 50);

			FlexLayout.SetGrow(item1, 1);
			FlexLayout.SetShrink(item1, 0);
			FlexLayout.SetGrow(item2, 1);
			FlexLayout.SetShrink(item2, 0);

			root.Add(controlsFlexLayout);
			flexLayout.Add(item1 as IView);
			flexLayout.Add(item2 as IView);

			// Container = 300px; total natural width = 150px; free space = 150px.
			// With Grow=1 on both items each should receive 75px of extra space:
			//   item1 expected: 50 + 75 = 125
			//   item2 expected: 100 + 75 = 175
			_ = flexLayout.CrossPlatformMeasure(300, 200);

			var frame1 = flexLayout.GetFlexFrame(item1 as IView);
			var frame2 = flexLayout.GetFlexFrame(item2 as IView);

			Assert.Equal(125, frame1.Width);
			Assert.Equal(175, frame2.Width);
		}
	}
}
