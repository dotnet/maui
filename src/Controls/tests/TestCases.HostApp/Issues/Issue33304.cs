namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 33304, "Shell TitleView disappears when switching tabs on Android", PlatformAffected.Android)]
	public class Issue33304 : Shell
	{
		public Issue33304()
		{
			Shell.SetTitleView(this, new Label
			{
				Text = "TitleView",
				AutomationId = "HomeTitleView"
			});
			// Shell.SetFlyoutBehavior(FlyoutBehavior.Disabled);
			var tabBar = new TabBar();
			tabBar.Route = "homePage";
			// Home Tab
			var homeTab = new Tab { Title = "Home" };
			var homeContent = new ShellContent();
			var homePage = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Padding = 20,
					Spacing = 20,
					Children =
					{
						new Label
						{
							Text = "This is the Home tab",
							AutomationId = "HomeTabLabel",
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center
						},
						new Label
						{
							Text = "Switch to other tabs and back to verify TitleView persists",
							HorizontalOptions = LayoutOptions.Center
						}
					}
				}
			};
			homeContent.Content = homePage;
			homeTab.Items.Add(homeContent);
			tabBar.Items.Add(homeTab);

			// Search Tab
			var searchTab = new Tab { Title = "Search" };
			var searchContent = new ShellContent();
			var searchPage = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Padding = 20,
					Spacing = 20,
					Children =
					{
						new Label
						{
							Text = "This is the Search tab",
							AutomationId = "SearchTabLabel",
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center
						}
					}
				}
			};
			searchContent.Content = searchPage;
			searchTab.Items.Add(searchContent);
			tabBar.Items.Add(searchTab);

			// Settings Tab
			var settingsTab = new Tab { Title = "Settings" };
			var settingsContent = new ShellContent();
			var settingsPage = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Padding = 20,
					Spacing = 20,
					Children =
					{
						new Label
						{
							Text = "This is the Settings tab",
							FontSize = 18,
							HorizontalOptions = LayoutOptions.Center
						}
					}
				}
			};
			settingsContent.Content = settingsPage;
			settingsTab.Items.Add(settingsContent);
			tabBar.Items.Add(settingsTab);

			Items.Add(tabBar);
		}
	}
}
