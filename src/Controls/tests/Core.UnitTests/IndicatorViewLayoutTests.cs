using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class IndicatorViewTests : BaseTestFixture
	{
		[Fact]
		public void IndicatorStackLayoutNoItems_ResetIndicators_ShouldHaveNoChildren()
		{
			// Arrange
			var indicatorView = new IndicatorView();
			var indicatorStackLayout = new IndicatorStackLayout(indicatorView);

			// Act
			indicatorStackLayout.ResetIndicators();

			// Assert
			Assert.Empty(indicatorStackLayout.Children);
		}

		[Fact]
		public void IndicatorStackLayoutWithItems_ResetIndicators_ShouldBindChildren()
		{
			// Arrange
			var indicatorView = new IndicatorView() { ItemsSource = new List<string>{"item1", "item2"} };
			var indicatorStackLayout = new IndicatorStackLayout(indicatorView);

			// Act
			indicatorStackLayout.ResetIndicators();

			// Assert
			Assert.Equal(2, indicatorStackLayout.Children.Count);
		}

		[Theory]
		[InlineData(1, 2)]
		[InlineData(0, 2)]
		[InlineData(-2, 2)]
		public void IndicatorStackLayout_ResetIndicatorCount_ShouldBindChildren(int oldCount, int expected)
		{
			// Arrange
			var indicatorView = new IndicatorView() { ItemsSource = new List<string>{"item1", "item2"} };
			var indicatorStackLayout = new IndicatorStackLayout(indicatorView);
			Assert.Empty(indicatorStackLayout.Children);

			// Act
			indicatorStackLayout.ResetIndicatorCount(oldCount);

			// Assert
			Assert.Equal(expected, indicatorStackLayout.Children.Count);
		}
	}
}
