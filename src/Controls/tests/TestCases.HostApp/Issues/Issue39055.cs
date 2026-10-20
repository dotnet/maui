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
			ContentTemplate = new DataTemplate(() => new TestPageIssue39055("home"))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Browse",
			AutomationId = "tab_browse",
			Route = "BrowsePage",
			ContentTemplate = new DataTemplate(() => new TestPageIssue39055("browse"))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Settings",
			AutomationId = "tab_settings",
			Route = "SettingsPage",
			ContentTemplate = new DataTemplate(() => new TestPageIssue39055("settings"))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Notifications",
			AutomationId = "tab_notifications",
			Route = "NotificationsPage",
			ContentTemplate = new DataTemplate(() => new TestPageIssue39055("notifications"))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Profile",
			AutomationId = "tab_profile",
			Route = "ProfilePage",
			ContentTemplate = new DataTemplate(() => new TestPageIssue39055("profile"))
		});

		tabBar.Items.Add(new ShellContent
		{
			Title = "Account",
			AutomationId = "tab_account",
			Route = "AccountPage",
			ContentTemplate = new DataTemplate(() => new TestPageIssue39055("account"))
		});

		Items.Add(tabBar);
	}
}

public class TestPageIssue39055 : ContentPage
{
	public TestPageIssue39055(string tabName)
	{
		var pageLabel = new Label
		{
			AutomationId = $"page_{tabName}",
			Text = $"Test Page for {tabName}",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

#if WINDOWS
		var reduceWindowWidthButton = new Button
		{
			AutomationId = "ReduceWindowWidthButton",
			Text = "Reduce Window Width"
		};

		reduceWindowWidthButton.Clicked += (sender, e) =>
		{
			if (Window is not null)
				Window.Width = 400;
		};

		Content = new VerticalStackLayout
		{
			Children =
			{
				pageLabel,
				reduceWindowWidthButton
			}
		};
#else
		Content = pageLabel;
#endif
	}
}