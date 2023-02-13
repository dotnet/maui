using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;
using NSubstitute;

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

			var manager = new FlexLayoutManager(flexLayout);
			_ = manager.Measure(1000, 1000);

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

			var manager = new FlexLayoutManager(flexLayout);

			// Measure and arrange the layout while the first view is visible
			var measure = manager.Measure(1000, 1000);
			manager.ArrangeChildren(new Rect(Point.Zero, measure));

			// Keep track of where the second view is arranged
			var whenVisible = view2.Frame.X;

			// Change the visibility
			view.IsVisible = false;

			// Measure and arrange againg
			measure = manager.Measure(1000, 1000);
			manager.ArrangeChildren(new Rect(Point.Zero, measure));

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

		(IFlexLayout, IView) SetUpUnconstrainedTest() 
		{
			var root = new Grid(); // FlexLayout requires a parent, at least for now
			var flexLayout = new FlexLayout() as IFlexLayout;

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
	}
}
