using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Platform.Handlers.Tests;
using Xamarin.Platform.Layouts;

namespace Xamarin.Platform.Handlers.UnitTests.Layouts
{
	[TestFixture(Category = TestCategory.Layout)]
	public class HorizontalStackLayoutManagerTests : StackLayoutManagerTests
	{
		[TestCase(0, 100, 0, 0, Description = "No items, width should be zero")]
		[TestCase(1, 100, 0, 100, Description = "One item, width should match the item")]
		[TestCase(1, 100, 13, 100, Description = "One item, spacing should have no effect")]
		[TestCase(2, 100, 13, 213, Description = "Two items, spacing should count once [(100 * 2) + 13 = 213]")]
		[TestCase(3, 100, 13, 326, Description = "Three items, spacing should count twice [(100 * 3) + (2 * 13) = 326]")]
		[TestCase(3, 100, -13, 274, Description = "Negative spacing overlaps items [(100 * 3) + (2 * -13) = 274]")]
		public void SpacingMeasurement(int viewCount, double viewWidth, int spacing, double expectedWidth)
		{
			var stack = BuildStack(viewCount, viewWidth, 100);
			stack.Spacing.Returns(spacing);

			var manager = new HorizontalStackLayoutManager(stack);
			var measuredSize = manager.Measure(double.PositiveInfinity, 100);

			Assert.That(measuredSize.Width, Is.EqualTo(expectedWidth));
		}

		[Test(Description = "Spacing should not affect arrangement with only one item")]
		[TestCase(0), TestCase(26), TestCase(-54)]
		public void SpacingArrangementOneItem(int spacing)
		{
			var stack = BuildStack(1, 100, 100);
			stack.Spacing.Returns(spacing);

			var manager = new HorizontalStackLayoutManager(stack);

			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.Arrange(new Rectangle(Point.Zero, measuredSize));

			var expectedRectangle = new Rectangle(0, 0, 100, 100);
			stack.Children[0].Received().Arrange(Arg.Is(expectedRectangle));
		}

		[Test(Description = "Spacing should affect arrangement with more than one item")]
		[TestCase(26), TestCase(-54)]
		public void SpacingArrangementTwoItems(int spacing)
		{
			var stack = BuildStack(2, 100, 100);
			stack.Spacing.Returns(spacing);

			var manager = new HorizontalStackLayoutManager(stack);

			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.Arrange(new Rectangle(Point.Zero, measuredSize));

			var expectedRectangle0 = new Rectangle(0, 0, 100, 100);
			stack.Children[0].Received().Arrange(Arg.Is(expectedRectangle0));

			var expectedRectangle1 = new Rectangle(100 + spacing, 0, 100, 100);
			stack.Children[1].Received().Arrange(Arg.Is(expectedRectangle1));
		}

		[TestCase(150, 100, 100, Description = "The Stack's specified width of 100 should override the child's measured width of 150")]
		[TestCase(150, 200, 200, Description = "The Stack's specified width of 200 should override the child's measured width of 150")]
		[TestCase(1250, -1, 1250, Description = "The Stack doesn't specify width, so the child determines the width")]
		public void StackAppliesWidth(double viewWidth, double stackWidth, double expectedWidth)
		{
			var stack = CreateTestLayout();

			var view = CreateTestView(new Size(viewWidth, 100));

			var children = new List<IView>() { view }.AsReadOnly();

			stack.Children.Returns(children);
			stack.Width.Returns(stackWidth);

			var manager = new HorizontalStackLayoutManager(stack);
			var measurement = manager.Measure(double.PositiveInfinity, 100);
			Assert.That(measurement.Width, Is.EqualTo(expectedWidth));
		}

		[Test]
		public void ViewsArrangedWithDesiredHeights()
		{
			var stack = CreateTestLayout();
			var manager = new HorizontalStackLayoutManager(stack);

			var view1 = CreateTestView(new Size(100, 200));
			var view2 = CreateTestView(new Size(100, 150));

			var children = new List<IView>() { view1, view2 }.AsReadOnly();
			stack.Children.Returns(children);

			var measurement = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.Arrange(new Rectangle(Point.Zero, measurement));

			// The tallest IView is 200, so the stack should be that tall
			Assert.That(measurement.Height, Is.EqualTo(200));

			// We expect the first IView to be at 0,0 with a width of 100 and a height of 200
			var expectedRectangle1 = new Rectangle(0, 0, 100, 200);
			view1.Received().Arrange(Arg.Is(expectedRectangle1));

			// We expect the second IView to be at 100, 0 with a width of 100 and a height of 150
			var expectedRectangle2 = new Rectangle(100, 0, 100, 150);
			view2.Received().Arrange(Arg.Is(expectedRectangle2));
		}
	}
}
