namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26864, "Shell Content Title Not Rendering in Full-Screen Mode on Mac Catalyst", PlatformAffected.macOS)]
public class Issue26864 : Shell
{
	public Issue26864()
	{
		var tabBar = new TabBar
		{
			AutomationId = "TabBar"
		};
		var tab = new Tab
		{
			Title = "Nested Tabs",
			AutomationId = "tabbar"
		};
		var homeShellContent = new ShellContent
		{
			Title = "Home",
			Content = new ContentPage
			{
				BackgroundColor = Microsoft.Maui.Graphics.Colors.White
			}
		};
		var settingsShellContent = new ShellContent
		{
			Title = "Settings",
			Content = new ContentPage
			{
				BackgroundColor = Microsoft.Maui.Graphics.Colors.White
			}
		};
		tab.Items.Add(homeShellContent);
		tab.Items.Add(settingsShellContent);
		tabBar.Items.Add(tab);
		this.Items.Add(tabBar);
	}

}