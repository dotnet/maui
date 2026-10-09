namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39055, "TabBar AutomationId should be set", PlatformAffected.All)]
public class Issue39055 : Shell
{
	public Issue39055()
	{
		var tabBar = new TabBar
		{
			AutomationId = "main_tab_bar"
		};

		tabBar.Items.Add(new ShellContent
		{
			Title = "Home",
			AutomationId = "tab_home",
			Route = "HomePage",
			ContentTemplate = new DataTemplate(typeof(TestPageIssue39055))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Browse",
			AutomationId = "tab_browse",
			Route = "BrowsePage",
			ContentTemplate = new DataTemplate(typeof(TestPageIssue39055))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Settings",
			AutomationId = "tab_settings",
			Route = "SettingsPage",
			ContentTemplate = new DataTemplate(typeof(TestPageIssue39055))
		});

		Items.Add(tabBar);
	}
}

public class TestPageIssue39055 : ContentPage
{
	public TestPageIssue39055()
	{
		Content = new Label
		{
			Text = "Test Page for Issue 39055",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}