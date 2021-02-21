using System.Collections.Generic;
using NSubstitute;
using Xamarin.Forms;
using Xamarin.Platform.Layouts;
using Xunit;

namespace Xamarin.Platform.Handlers.UnitTests.Layouts
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
			manager.Arrange(new Rectangle(Point.Zero, measuredSize));

			var expectedRectangle = new Rectangle(0, 0, 100, 100);
			stack.Children[0].Received().Arrange(Arg.Is(expectedRectangle));
		}

		[Theory("Spacing has an effect when there's more than one item")]
		[InlineData(26), InlineData(-54)]
		public void SpacingArrangementTwoItems(int spacing)
		{
			var stack = BuildStack(2, 100, 100);
			stack.Spacing.Returns(spacing);

			var manager = new VerticalStackLayoutManager(stack);

			var measuredSize = manager.Measure(double.PositiveInfinity, 100);
			manager.Arrange(new Rectangle(Point.Zero, measuredSize));

			var expectedRectangle0 = new Rectangle(0, 0, 100, 100);
			stack.Children[0].Received().Arrange(Arg.Is(expectedRectangle0));

			var expectedRectangle1 = new Rectangle(0, 100 + spacing, 100, 100);
			stack.Children[1].Received().Arrange(Arg.Is(expectedRectangle1));
		}

		[Theory]
		[InlineData(150, 100, 100)]
		[InlineData(150, 200, 200)]
		[InlineData(1250, -1, 1250)]
		public void StackAppliesHeight(double viewHeight, double stackHeight, double expectedHeight)
		{
			var stack = CreateTestLayout();

			var view = CreateTestView(new Size(100, viewHeight));

			var children = new List<IView>() { view }.AsReadOnly();

			stack.Children.Returns(children);
			stack.Height.Returns(stackHeight);

			var manager = new VerticalStackLayoutManager(stack);
			var measurement = manager.Measure(100, double.PositiveInfinity);
			Assert.Equal(expectedHeight, measurement.Height);
		}

		[Fact]
		public void ViewsArrangedWithDesiredWidths()
		{
			var stack = CreateTestLayout();
			var manager = new VerticalStackLayoutManager(stack);

			var view1 = CreateTestView(new Size(200, 100));
			var view2 = CreateTestView(new Size(150, 100));

			var children = new List<IView>() { view1, view2 }.AsReadOnly();
			stack.Children.Returns(children);

			var measurement = manager.Measure(double.PositiveInfinity, double.PositiveInfinity);
			manager.Arrange(new Rectangle(Point.Zero, measurement));

			// The widest IView is 200, so the stack should be that wide
			Assert.Equal(200, measurement.Width);

			// We expect the first IView to be at 0,0 with a width of 200 and a height of 100
			var expectedRectangle1 = new Rectangle(0, 0, 200, 100);
			view1.Received().Arrange(Arg.Is(expectedRectangle1));

			// We expect the second IView to be at 0, 100 with a width of 150 and a height of 100
			var expectedRectangle2 = new Rectangle(0, 100, 150, 100);
			view2.Received().Arrange(Arg.Is(expectedRectangle2));
		}
	}
}
