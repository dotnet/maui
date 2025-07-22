using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class StackLayoutTests
	{
		[Fact]
		public void VerticalOrientationConstrainsViewsHorizontallyWhenConstrained()
		{
			var layout = new StackLayout { Orientation = StackOrientation.Vertical };
			var child = new ContentView();
			layout.Add(child);

			Assert.Equal(LayoutConstraint.None, child.Constraint);

			layout.WidthRequest = 100;
			Assert.Equal(LayoutConstraint.HorizontallyFixed, child.Constraint);

			child.HeightRequest = 50;
			Assert.Equal(LayoutConstraint.Fixed, child.Constraint);

			layout.HorizontalOptions = LayoutOptions.Center;
			layout.WidthRequest = -1;
			Assert.Equal(LayoutConstraint.VerticallyFixed, child.Constraint);
		}

		[Fact]
		public void HorizontalOrientationConstrainsViewsVerticallyWhenConstrained()
		{
			var layout = new StackLayout { Orientation = StackOrientation.Horizontal };
			var child = new ContentView();
			layout.Add(child);

			Assert.Equal(LayoutConstraint.None, child.Constraint);

			layout.HeightRequest = 100;
			Assert.Equal(LayoutConstraint.VerticallyFixed, child.Constraint);

			child.WidthRequest = 50;
			Assert.Equal(LayoutConstraint.Fixed, child.Constraint);

			layout.VerticalOptions = LayoutOptions.Center;
			layout.HeightRequest = -1;
			Assert.Equal(LayoutConstraint.HorizontallyFixed, child.Constraint);
		}
	}
}
