namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40490, "Android back closes the app after switching tabs", PlatformAffected.Android)]
public class Issue40490 : TabbedPage
{
	public Issue40490()
	{
		Children.Add(CreateTab("Tab A", "Tab A root", "TabARootLabel", "PushTabADetailButton", "Tab A detail", "TabADetailLabel"));
		Children.Add(CreateTab("Tab B", "Tab B root", "TabBRootLabel", "PushTabBDetailButton", "Tab B detail", "TabBDetailLabel"));
	}

	static NavigationPage CreateTab(string tabTitle, string rootText, string rootAutomationId, string pushButtonAutomationId, string detailText, string detailAutomationId)
	{
		var rootPage = new ContentPage();
		var pushButton = new Button
		{
			Text = "Push detail page",
			AutomationId = pushButtonAutomationId,
			Command = new Command(async () => await rootPage.Navigation.PushAsync(new ContentPage
			{
				Content = new Label
				{
					Text = detailText,
					AutomationId = detailAutomationId
				}
			}))
		};

		rootPage.Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text = rootText,
					AutomationId = rootAutomationId
				},
				pushButton
			}
		};

		return new NavigationPage(rootPage)
		{
			Title = tabTitle
		};
	}
}
