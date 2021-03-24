using System.Collections.Generic;
using Microsoft.Maui.Layouts;
using NSubstitute;
using Xunit;

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
			manager.ArrangeChildren(new Rectangle(Point.Zero, measuredSize));

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
			manager.ArrangeChildren(new Rectangle(Point.Zero, measuredSize));

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

			var view = LayoutTestHelpers.CreateTestView(new Size(100, viewHeight));

			var children = new List<IView>() { view }.AsReadOnly();

			stack.Children.Returns(children);
			stack.Height.Returns(stackHeight);

			var manager = new VerticalStackLayoutManager(stack);
			var measurement = manager.Measure(100, double.PositiveInfinity);
			Assert.Equal(expectedHeight, measurement.Height);
		}
	}
}
