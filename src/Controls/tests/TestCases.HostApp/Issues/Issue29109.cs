using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29109, "[Android] Unable to set unselected iconImageSource color when toolbar placement is set to bottom", PlatformAffected.Android)]
public class Issue29109 : TestTabbedPage
{
	protected override void Init()
	{
		// Set the toolbar placement to bottom (android specific)
		On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
		// Set tab colors
		this.SelectedTabColor = Colors.Red;
		this.UnselectedTabColor = Colors.Gray;

		// Add tabs
		this.Children.Add(new Issue29109Tab1());
		this.Children.Add(new Issue29109Tab2());
		this.Children.Add(new Issue29109Tab3());
	}
}

public class Issue29109Tab1 : ContentPage
{
	public Issue29109Tab1()
	{
		//If no FontImageSource color is given, tab icon should be applied based on the SelectedTabColor and UnselectedTabColor.
		IconImageSource = new FontImageSource
		{
			FontFamily = "Ion",
			Glyph = "\uf47e",
			Size = 15
		};
		Button button = new Button
		{
			Text = "Change Tab Icon Color",
			BackgroundColor = Colors.Green,
			TextColor = Colors.White
		};
		button.AutomationId = "Button";
		button.Clicked += (s, e) =>
		{
			//If the FontImageSource color is given, the tab icon color should be applied solely based on the specified color, regardless of the SelectedTabColor or UnselectedTabColor.
			(this.IconImageSource as FontImageSource).Color = Colors.Orange;
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

public class Issue29109Tab2 : ContentPage
{
	public Issue29109Tab2()
	{
		//The FontImageSource color is given. So, the tab icon color should be applied based solely on the given color, regardless of the selected tab or unselected tab color.
		IconImageSource = new FontImageSource
		{
			FontFamily = "Ion",
			Glyph = "\uf47e",
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
public class Issue29109Tab3 : ContentPage
{
	public Issue29109Tab3()
	{
		//The FontImageSource color is given. So, the icon color should be applied based solely on the given color, regardless of the selected tab or unselected tab color.
		IconImageSource = new FontImageSource
		{
			FontFamily = "Ion",
			Glyph = "\uf47e",
			Color = Colors.Green,
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
