using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class GridTests
	{
		[Fact]
		public void GridConstrainsViewsVerticallyWhenPossible()
		{
			// While testing vertical constraints, make sure to avoid setting the horizontal one
			var grid = new Grid { HorizontalOptions = LayoutOptions.Center };

			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });

			var f50 = grid.Add(0, 0);
			var f50star = grid.Add(0, 0, rowSpan: 2);
			var star = grid.Add(1, 0);
			var starAuto = grid.Add(1, 0, rowSpan: 2);
			var auto = grid.Add(2, 0);
			var autof50 = grid.Add(2, 0, rowSpan: 2);

			Assert.Equal(LayoutConstraint.VerticallyFixed, f50.Constraint);
			Assert.Equal(LayoutConstraint.None, f50star.Constraint);
			Assert.Equal(LayoutConstraint.None, star.Constraint);
			Assert.Equal(LayoutConstraint.None, starAuto.Constraint);
			Assert.Equal(LayoutConstraint.None, auto.Constraint);
			Assert.Equal(LayoutConstraint.None, autof50.Constraint);

			// Now set a fixed height on the grid
			grid.HeightRequest = 100;

			Assert.Equal(LayoutConstraint.VerticallyFixed, f50.Constraint);
			Assert.Equal(LayoutConstraint.VerticallyFixed, f50star.Constraint);
			Assert.Equal(LayoutConstraint.VerticallyFixed, star.Constraint);
			Assert.Equal(LayoutConstraint.None, starAuto.Constraint);
			Assert.Equal(LayoutConstraint.None, auto.Constraint);
			Assert.Equal(LayoutConstraint.None, autof50.Constraint);

			// Now change children vertical options to center so we can see `VerticallyFixed` is not applied anymore
			f50.VerticalOptions = LayoutOptions.Center;
			f50star.VerticalOptions = LayoutOptions.Center;
			star.VerticalOptions = LayoutOptions.Center;
			starAuto.VerticalOptions = LayoutOptions.Center;
			auto.VerticalOptions = LayoutOptions.Center;
			autof50.VerticalOptions = LayoutOptions.Center;

			Assert.Equal(LayoutConstraint.None, f50.Constraint);
			Assert.Equal(LayoutConstraint.None, f50star.Constraint);
			Assert.Equal(LayoutConstraint.None, star.Constraint);
			Assert.Equal(LayoutConstraint.None, starAuto.Constraint);
			Assert.Equal(LayoutConstraint.None, auto.Constraint);
			Assert.Equal(LayoutConstraint.None, autof50.Constraint);
		}

		[Fact]
		public void GridConstrainsViewsHorizontallyWhenPossible()
		{
			// While testing horizontal constraints, make sure to avoid setting the vertical one
			var grid = new Grid { VerticalOptions = LayoutOptions.Center };

			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

			var f50 = grid.Add(0, 0);
			var f50star = grid.Add(0, 0, columnSpan: 2);
			var star = grid.Add(0, 1);
			var starAuto = grid.Add(0, 1, columnSpan: 2);
			var auto = grid.Add(0, 2);
			var autof50 = grid.Add(0, 2, columnSpan: 2);

			Assert.Equal(LayoutConstraint.HorizontallyFixed, f50.Constraint);
			Assert.Equal(LayoutConstraint.None, f50star.Constraint);
			Assert.Equal(LayoutConstraint.None, star.Constraint);
			Assert.Equal(LayoutConstraint.None, starAuto.Constraint);
			Assert.Equal(LayoutConstraint.None, auto.Constraint);
			Assert.Equal(LayoutConstraint.None, autof50.Constraint);

			// Now set a fixed width on the grid
			grid.WidthRequest = 100;

			Assert.Equal(LayoutConstraint.HorizontallyFixed, f50.Constraint);
			Assert.Equal(LayoutConstraint.HorizontallyFixed, f50star.Constraint);
			Assert.Equal(LayoutConstraint.HorizontallyFixed, star.Constraint);
			Assert.Equal(LayoutConstraint.None, starAuto.Constraint);
			Assert.Equal(LayoutConstraint.None, auto.Constraint);
			Assert.Equal(LayoutConstraint.None, autof50.Constraint);

			// Now change children horizontal options to center so we can see `HorizontallyFixed` is not applied anymore
			f50.HorizontalOptions = LayoutOptions.Center;
			f50star.HorizontalOptions = LayoutOptions.Center;
			star.HorizontalOptions = LayoutOptions.Center;
			starAuto.HorizontalOptions = LayoutOptions.Center;
			auto.HorizontalOptions = LayoutOptions.Center;
			autof50.HorizontalOptions = LayoutOptions.Center;

			Assert.Equal(LayoutConstraint.None, f50.Constraint);
			Assert.Equal(LayoutConstraint.None, f50star.Constraint);
			Assert.Equal(LayoutConstraint.None, star.Constraint);
			Assert.Equal(LayoutConstraint.None, starAuto.Constraint);
			Assert.Equal(LayoutConstraint.None, auto.Constraint);
		}

		[Fact]
		public void GridCompletelyConstrainsViewsWhenPossible()
		{
			var grid = new Grid { WidthRequest = 100, HeightRequest = 100 };

			var child = grid.Add(0, 0);

			Assert.Equal(LayoutConstraint.Fixed, child.Constraint);
		}
	}
}
