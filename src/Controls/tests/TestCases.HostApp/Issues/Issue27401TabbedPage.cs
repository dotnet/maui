
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "27401TabbedPage", "TabbedPage.PopToRootOnTabReselect", PlatformAffected.Android)]
public class Issue27401TabbedPage : TabbedPage
{
	public Issue27401TabbedPage()
	{
		Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.TabbedPage.SetToolbarPlacement(this, Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement.Bottom);
		TabbedPage.SetPopToRootOnTabReselect(this, true);
		
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
		button.Command = new Command(() =>
		{
			navPage.PushAsync(new ContentPage()
			{
				Title = "Deep 1",
				Content = new Button()
				{
					Text = "Deep 1",
					AutomationId = "Deep1Button",
					VerticalOptions = LayoutOptions.Center,
					Command = new Command(() =>
					{
						navPage.PushAsync(new ContentPage()
						{
							Title = "Deep 2",
							Content = new Button()
							{
								Text = "Deep 2",
								VerticalOptions = LayoutOptions.Center,
								AutomationId = "Deep2Label"
							}
						});
					})
				}
			});
		});

		rootPage.Content = button;
		Children.Add(navPage);
		Children.Add(new ContentPage { Title = "Ignore Me" });
	}
}
