namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25115, "[Windows] The color was not applied properly to the Tab Chevron icon", PlatformAffected.UWP)]
public class Issue25115 : TestShell
{
	protected override void Init()
	{
		SetValue(Shell.TabBarUnselectedColorProperty, Colors.Blue);
		SetValue(Shell.TabBarTitleColorProperty, Colors.Red);

		var tabBar = new TabBar();
		var tab1 = new Tab();
		var tab2 = new Tab();
		tab1.Title = "Tab 1";
		tab2.Title = "Tab 2";

		tab1.Items.Add(GetShellContent("Page 1", "Page 1 Content"));
		tab1.Items.Add(GetShellContent("Page 2", "Page 2 Content"));
		tab2.Items.Add(GetShellContent("Tab 2", "Tab 2 Content"));

		tabBar.Items.Add(tab1);
		tabBar.Items.Add(tab2);
		Items.Add(tabBar);
	}
	ShellContent GetShellContent(string title, string content)
	{
		return new ShellContent
		{
			Title = title,
			Content = new ContentPage
			{
				Title = title,
				Content = new Label { Text = content, AutomationId = "label" },
			}
		};
	}
}