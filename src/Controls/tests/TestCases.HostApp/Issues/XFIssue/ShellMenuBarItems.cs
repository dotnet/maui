namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell MenuBarItems Test",
	PlatformAffected.WinRT)]
public class ShellMenuBarItems : TestShell
{
	protected override void Init()
	{
		var menuBarItem = new MenuBarItem()
		{
			Text = "Title 1"
		};

		var menuBarItem2 = new MenuBarItem()
		{
			Text = "Title 2"
		};

		menuBarItem.Add(new MenuFlyoutItem()
		{
			Text = "Item 1"
		});
		menuBarItem.Add(new MenuFlyoutItem()
		{
			Text = "Item 2"
		});
		menuBarItem.Add(new MenuFlyoutItem()
		{
			Text = "Item 3"
		});

		var contentPage = new ContentPage()
		{
			Content = new Label() { Text = "Shell MenuBarItems Test" }
		};

		contentPage.MenuBarItems.Add(menuBarItem);
		contentPage.MenuBarItems.Add(menuBarItem2);

		AddContentPage(contentPage);

		FlyoutBehavior = FlyoutBehavior.Disabled;

		// Set menu bar and menu bar item foreground color
		SetBackgroundColor(this, Colors.Purple);
		SetTitleColor(this, Colors.White);
	}
}
