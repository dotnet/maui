using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3413, "[iOS] Searchbar in Horizontal Stacklayout doesn't render", PlatformAffected.iOS)]
	public class Issue3413 : TestContentPage
	{
		protected override void Init()
		{
			Padding = new Thickness(20);

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Vertical
			};

			var searchBar = new SearchBar
			{
				BackgroundColor = Colors.Yellow,
				Text = "i m on a vertical stacklayout",
				AutomationId = "srb_vertical"
			};
			layout.Children.Add(new Label { Text = "Vertical" });
			layout.Children.Add(searchBar);

			var layout1 = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var searchBar1 = new SearchBar
			{
				BackgroundColor = Colors.Yellow,
				Text = "i m on a horizontal stacklayout",
				AutomationId = "srb_horizontal"
			};

			layout1.Children.Add(new Label { Text = "Horizontal" });
			layout1.Children.Add(searchBar1);

			var searchBar2 = new SearchBar
			{
				BackgroundColor = Colors.Blue,
				Text = "i m with expand",
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				AutomationId = "srb_grid"
			};

			var grid = new Grid();
			grid.Children.Add(layout);
			Grid.SetRow(layout, 0);
			grid.Children.Add(layout1);
			Grid.SetRow(layout1, 1);
			grid.Children.Add(searchBar2);
			Grid.SetRow(searchBar2, 2);
			Content = grid;
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Issue3413Test()
		{
			RunningApp.WaitForElement(q => q.Marked("srb_vertical"));
			RunningApp.WaitForElement(q => q.Marked("srb_horizontal"));
			RunningApp.Screenshot("Please verify we have 2 SearchBars. One below the label, other side by side with the label");
		}
#endif
	}
}