using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[TestFixture, Category("Layout")]
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

		[Test]
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

		[Test]
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
	}
}
