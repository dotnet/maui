using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class VerticalStackLayoutTests
	{
		[Fact]
		public void ConstrainsViewsHorizontallyWhenConstrained()
		{
			var layout = new VerticalStackLayout();
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
	}
}
