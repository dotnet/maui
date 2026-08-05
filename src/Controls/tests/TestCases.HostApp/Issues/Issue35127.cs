namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35127, "[Android] Shell/TabbedPage More BottomSheet uses hard-coded M2 colors when Material3 is enabled", PlatformAffected.Android)]
public class Issue35127 : Shell
{
	public Issue35127()
	{
		var tabBar = new TabBar();

		// Create 6 tabs so the "More" overflow button appears (Android max is 5)
		for (int i = 1; i <= 6; i++)
		{
			int tabNum = i;
			var content = new ShellContent
			{
				Title = $"Tab{tabNum}",
				Route = $"M3Tab{tabNum}",
				ContentTemplate = new DataTemplate(() => new ContentPage
				{
					Title = $"Tab{tabNum}",
					Content = new VerticalStackLayout
					{
						VerticalOptions = LayoutOptions.Center,
						Children =
						{
							new Label
							{
								Text = $"Tab{tabNum} Content",
								AutomationId = $"M3Tab{tabNum}Content",
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center,
								FontSize = 24
							}
						}
					}
				})
			};

			var tab = new Tab
			{
				Title = $"Tab{tabNum}",
				AutomationId = $"M3Tab{tabNum}",
				Icon = "dotnet_bot.png"
			};
			tab.Items.Add(content);
			tabBar.Items.Add(tab);
		}

		Items.Add(tabBar);
	}
}
