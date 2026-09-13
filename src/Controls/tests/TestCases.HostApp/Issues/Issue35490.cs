using TabbedPage = Microsoft.Maui.Controls.TabbedPage;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35490, "[iOS 26] TabbedPage with NavigationPage children clips content above floating glass tab bar", PlatformAffected.iOS)]
public class Issue35490 : TabbedPage
{
	public Issue35490()
	{
		var page1 = new ContentPage
		{
			Title = "Tab1",
			BackgroundColor = Colors.MediumPurple,
			Content = new Label
			{
				AutomationId = "Tab1Label",
				Text = "Tab 1 — on iOS 26+, test passes if purple background extends under the floating tab bar",
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			},
		};

		var page2 = new ContentPage
		{
			Title = "Tab2",
			BackgroundColor = Colors.DarkCyan,
			Content = new Label
			{
				AutomationId = "Tab2Label",
				Text = "Tab 2 — on iOS 26+, test passes if cyan background extends under the floating tab bar",
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			},
		};

		Children.Add(new NavigationPage(page1) { Title = "Tab1" });
		Children.Add(new NavigationPage(page2) { Title = "Tab2" });
	}
}
