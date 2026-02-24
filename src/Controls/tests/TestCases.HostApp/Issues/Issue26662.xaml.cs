namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26662, "Unable to dynamically set unselected IconImageSource Color on Android", PlatformAffected.All)]
	public partial class Issue26662 : TabbedPage
	{
		public Issue26662()
		{
			InitializeComponent();
		}

	}

	public class Issue26662Tab1 : ContentPage
	{
		public Issue26662Tab1()
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			//If no FontImageSource color is given, tab icon should be applied based on the SelectedTabColor and UnselectedTabColor.
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ion",
				Glyph = "\uf30c",
				Size = 15
			};
			Button button = new Button
			{
				Text = "Change Icon Color",
				BackgroundColor = Colors.Blue,
				TextColor = Colors.Red
			};
			button.AutomationId = "Button";
			button.Clicked += (s, e) =>
			{
				//If the FontImageSource color is given, the tab icon color should be applied solely based on the specified color, regardless of the SelectedTabColor or UnselectedTabColor.
				(this.IconImageSource as FontImageSource).Color = Colors.Violet;
			};
			Title = "Tab 1";
			var verticalStackLayout = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children = {
					new Label
						{
							HorizontalOptions = LayoutOptions.Center,
							Text = "Tab 1"
						},
						button
					}

			};
			Content = verticalStackLayout;
		}
	}

	public class Issue26662Tab2 : ContentPage
	{
		public Issue26662Tab2()
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			//The FontImageSource color is given. So, the tab icon color should be applied based solely on the given color, regardless of the selected tab or unselected tab color.
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ion",
				Glyph = "\uf30c",
				Color = Colors.DodgerBlue,
				Size = 15
			};

			Title = "Tab 2";
			var verticalStackLayout = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
					{
						new Label
							{
								HorizontalOptions = LayoutOptions.Center,
								Text = "Tab 2"
							},
					}
			};
			Content = verticalStackLayout;
		}
	}
	public class Issue26662Tab3 : ContentPage
	{
		public Issue26662Tab3()
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			IconImageSource = new FontImageSource
			{
				FontFamily = "Ion",
				Glyph = "\uf30c",
				Color = Colors.Red,
				Size = 15
			};

			Title = "Tab 3";
			var verticalStackLayout = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
					{
						new Label
							{
								HorizontalOptions = LayoutOptions.Center,
								Text = "Tab 3"
							},
					}
			};
			Content = verticalStackLayout;
		}
	}
}