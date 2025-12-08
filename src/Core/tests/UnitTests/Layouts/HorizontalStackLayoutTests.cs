using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Core, TestCategory.Layout)]
	public class HorizontalStackLayoutTests
	{
		[Fact]
		public void ConstrainsViewsVerticallyWhenConstrained()
		{
			var layout = new HorizontalStackLayout();
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
