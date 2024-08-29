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
	}
}
