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

		[Fact]
		public void FlexLayoutColumnDirectionReportsCorrectDesiredHeightInScrollView()
		{
			// Arrange
			var root = new Grid();
			var flexLayout = new FlexLayout { Direction = FlexDirection.Column };
			
			var view1 = Substitute.For<IView>();
			var view2 = Substitute.For<IView>();
			
			// Child 1 wants to be 100x200
			view1.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 200));
			view1.DesiredSize.Returns(new Size(100, 200));
			
			// Child 2 wants to be 100x150
			view2.Measure(Arg.Any<double>(), Arg.Any<double>()).Returns(new Size(100, 150));
			view2.DesiredSize.Returns(new Size(100, 150));
			
			root.Add(flexLayout);
			flexLayout.Add(view1);
			flexLayout.Add(view2);
			
			// Act - Measure with constrained width but unconstrained height (like ScrollView does)
			var measure = flexLayout.CrossPlatformMeasure(400, double.PositiveInfinity);
			
			// Assert - FlexLayout should report a height equal to sum of children's heights
			// With Column direction, children are stacked vertically: 200 + 150 = 350
			Assert.True(measure.Height > 0, "FlexLayout with Direction=Column should report non-zero height");
			Assert.Equal(350, measure.Height);
			
			// Also verify that children get proper frames
			var frame1 = flexLayout.GetFlexFrame(view1);
			var frame2 = flexLayout.GetFlexFrame(view2);
			
			Assert.True(frame1.Height > 0, "First child should have non-zero height");
			Assert.True(frame2.Height > 0, "Second child should have non-zero height");
			Assert.Equal(200, frame1.Height);
			Assert.Equal(150, frame2.Height);
		}

		[Fact]
		public void FlexLayoutColumnDirectionWithGrowReportsCorrectHeight()
		{
			// This test verifies that items with Grow=1 still get their intrinsic size
			// when measuring in Column direction with unconstrained height.
			// Without the fix, the Grow property causes items to try growing into
			// zero-sized flex_dim, resulting in height=0.
			
			var root = new Grid();
			var flexLayout = new FlexLayout { Direction = FlexDirection.Column };
			
			// Create custom labels that track measurement constraints
			var view1 = new TestLabelWithTracking();
			var view2 = new TestLabelWithTracking();
			
			root.Add(flexLayout);
			flexLayout.Add(view1);
			flexLayout.Add(view2);
			
			// Set Grow property like in the issue - this is the critical part
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetGrow(view2, 1);
			
			// Act - Measure with unconstrained height
			var measure = flexLayout.CrossPlatformMeasure(400, double.PositiveInfinity);
			
			// Assert - With the fix, children should be measured with infinity (not constrained to 0)
			// This verifies the measure hack set Grow=0, allowing SelfSizing to use infinity constraints
			Assert.True(view1.WasMeasuredWithInfinityHeight, "View1 should be measured with infinity height constraint");
			Assert.True(view2.WasMeasuredWithInfinityHeight, "View2 should be measured with infinity height constraint");
			
			// FlexLayout should report non-zero height
			Assert.True(measure.Height > 0, $"FlexLayout with Direction=Column and Grow should report non-zero height, got {measure.Height}");
			Assert.Equal(200, measure.Height); // 100 + 100 from TestLabel
			
			// Verify frames have correct heights
			var frame1 = flexLayout.GetFlexFrame(view1);
			var frame2 = flexLayout.GetFlexFrame(view2);
			
			Assert.Equal(100, frame1.Height);
			Assert.Equal(100, frame2.Height);
		}

		class TestLabelWithTracking : Label
		{
			public bool WasMeasuredWithInfinityHeight { get; private set; }

			protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
			{
				if (double.IsPositiveInfinity(heightConstraint))
				{
					WasMeasuredWithInfinityHeight = true;
				}
				return new Size(150, 100);
			}
		}

		[Fact]
		public void FlexLayoutColumnDirectionWithGrowAndRelativeBasisReportsCorrectHeight()
		{
			// Reproduces #33942: FlexLayout Direction=Column inside ScrollView
			// with children that have Grow=1 and Basis=0% (relative).
			// The Basis=0% overrides self-sizing, setting child frame to 0.
			// Then with container height=0 (from infinity), flex_dim=0,
			// so Grow can't distribute any space → children collapse to height=0.
			// This is the CRITICAL test that catches the real bug.
			
			var root = new Grid();
			var flexLayout = new FlexLayout
			{
				Direction = FlexDirection.Column,
				Wrap = FlexWrap.NoWrap,  // NoWrap to ensure children stack vertically
				AlignItems = FlexAlignItems.Stretch,
				JustifyContent = FlexJustify.Start
			};

			var view1 = new TestLabelWithTracking();
			var view2 = new TestLabelWithTracking();

			root.Add(flexLayout);
			flexLayout.Add(view1);
			flexLayout.Add(view2);

			// Match the real-world scenario from issue #33942: Grow=1, Basis=0% (relative)
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetBasis(view1, new FlexBasis(0, true));
			FlexLayout.SetAlignSelf(view1, FlexAlignSelf.Stretch);

			FlexLayout.SetGrow(view2, 1);
			FlexLayout.SetBasis(view2, new FlexBasis(0, true));
			FlexLayout.SetAlignSelf(view2, FlexAlignSelf.Stretch);

			// Measure with constrained width, unconstrained height (ScrollView)
			var measure = flexLayout.CrossPlatformMeasure(400, double.PositiveInfinity);

			// Without fix: height=0 because Basis=0% + container=0 + Grow can't fire
			// With fix: children should get their intrinsic height from self-sizing
			Assert.True(measure.Height > 0,
				$"FlexLayout Column+Grow+Basis(0%) should report non-zero height, got {measure.Height}");
			Assert.Equal(200, measure.Height); // 100 + 100 from TestLabel

			var frame1 = flexLayout.GetFlexFrame(view1);
			var frame2 = flexLayout.GetFlexFrame(view2);

			Assert.True(frame1.Height > 0, "First child should have non-zero height");
			Assert.True(frame2.Height > 0, "Second child should have non-zero height");
			Assert.Equal(100, frame1.Height);
			Assert.Equal(100, frame2.Height);
		}
	}
}
