using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35331, "Android TabbedPage inside Modal Navigation does not overlay BottomNavigationView after PushAsync", PlatformAffected.Android)]
public class Issue35331 : TestContentPage
{
	protected override void Init()
	{
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 30,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Tap the button to open a Modal TabbedPage with bottom tabs.\n\nExpected: After pushing a detail page from a tab, the bottom tab bar should be hidden.",
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Button
				{
					Text = "Open Modal TabbedPage",
					AutomationId = "OpenModalButton",
					Command = new Command(async () =>
					{
						await Navigation.PushModalAsync(new NavigationPage(new Issue35331TabbedPage()));
					})
				}
			}
		};
	}
}

public class Issue35331TabbedPage : TabbedPage
{
	public Issue35331TabbedPage()
	{
		Title = "Modal TabbedPage";
		On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
			.SetToolbarPlacement(ToolbarPlacement.Bottom);
		Children.Add(new Issue35331Tab1());
		Children.Add(new Issue35331Tab2());
	}
}

public class Issue35331Tab1 : ContentPage
{
	public Issue35331Tab1()
	{
		Title = "Tab 1";
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 30,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					AutomationId = "Tab1Label",
					Text = "Tab 1 Content",
					HorizontalOptions = LayoutOptions.Center
				},
				new Button
				{
					Text = "Push Detail Page",
					AutomationId = "PushDetailButton",
					Command = new Command(async () =>
					{
						await Navigation.PushAsync(new Issue35331DetailPage());
					})
				}
			}
		};
	}
}

public class Issue35331Tab2 : ContentPage
{
	public Issue35331Tab2()
	{
		Title = "Tab 2";
		Content = new VerticalStackLayout
		{
			Padding = 30,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					AutomationId = "Tab2Label",
					Text = "Tab 2 Content",
					HorizontalOptions = LayoutOptions.Center
				}
			}
		};
	}
}

public class Issue35331DetailPage : ContentPage
{
	public Issue35331DetailPage()
	{
		Title = "Detail Page";
		BackgroundColor = Colors.White;
		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 30,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					AutomationId = "DetailPageLabel",
					Text = "Detail Page\n\nThe bottom tab bar should NOT be visible.",
					FontSize = 24,
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center
				},
				new Button
				{
					Text = "Go Back",
					AutomationId = "GoBackButton",
					Command = new Command(async () =>
					{
						await Navigation.PopAsync();
					})
				}
			}
		};
	}
}
