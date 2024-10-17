namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 11869, "[Bug] ShellContent.IsVisible issue on Android",
	PlatformAffected.Android)]
public class Issue11869 : TestShell
{
	protected override void Init()
	{
		ContentPage contentPage = new ContentPage();
		var tabbar = AddContentPage<TabBar, Tab>(contentPage, title: "Tab 1");
		AddBottomTab("Tab 2");
		AddBottomTab("Tab 3");
		AddTopTab("TopTab2");
		AddTopTab("TopTab3");

		contentPage.Content =
			new StackLayout()
			{
				Children =
				{
					new Button
					{
						Text = "Hide Bottom Tab 2",
						Command = new Command(() =>
						{
							Items[0].Items[1].Items[0].IsVisible = false;
						}),
						AutomationId = "HideBottom2"
					},
					new Button
					{
						Text = "Hide Bottom Tab 3",
						Command = new Command(() =>
						{
							Items[0].Items[2].Items[0].IsVisible = false;
						}),
						AutomationId = "HideBottom3"
					},
					new Button
					{
						Text = "Hide Top Tab 2",
						Command = new Command(() =>
						{
							Items[0].Items[0].Items[1].IsVisible = false;
						}),
						AutomationId = "HideTop2"
					},
					new Button
					{
						Text = "Hide Top Tab 3",
						Command = new Command(() =>
						{
							Items[0].Items[0].Items[2].IsVisible = false;
						}),
						AutomationId = "HideTop3"
					},
					new Button
					{
						Text = "Show All Tabs",
						Command = new Command(() =>
						{
							Items[0].Items[1].Items[0].IsVisible = true;
							Items[0].Items[2].Items[0].IsVisible = true;
							Items[0].Items[0].Items[1].IsVisible = true;
							Items[0].Items[0].Items[2].IsVisible = true;
						}),
						AutomationId = "ShowAllTabs"
					}
				}
			};
	}
}
