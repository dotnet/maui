namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "15301TabbedPage", "TabbedPage.TabActiveTapped", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue15301TabbedPage : TabbedPage
{
	public Issue15301TabbedPage()
	{
		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);

		var rootPage = new ContentPage
		{
			Title = "Root Page"
		};

		var button = new Button
		{
			Text = "Deep 0",
			AutomationId = "Deep0Button",
			VerticalOptions = LayoutOptions.Center
		};

		var navPage = new NavigationPage(rootPage) { Title = "Tab 1" };
		
		TabActiveTapped += (s, e) =>
		{
			navPage.PopAsync();
		};

		button.Command = new Command(() =>
		{
			navPage.PushAsync(new ContentPage()
			{
				Title = "Deep 1",
				Content = new Button()
				{
					Text = "Hello, World!",
					AutomationId = "Deep1Label",
					VerticalOptions = LayoutOptions.Center,
				}
			});
		});

		rootPage.Content = button;
		Children.Add(navPage);
		Children.Add(new ContentPage { Title = "Ignore Me" });
	}
}
