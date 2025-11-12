namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16119, "MenuFlyoutItem with FileImageSource displays icon in monochrome instead of original colors", PlatformAffected.UWP)]
public class Issue16119 : TestShell
{
	protected override void Init()
	{
		MenuBarItem menuBarItem = new MenuBarItem {  Text = "Menu Flyout Item" };

		MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem
		{
			Text = "Hello World",
			IconImageSource = new FileImageSource { File = "small_dotnet_bot.png" }
		};
		menuBarItem.Add(menuFlyoutItem);

		MenuBarItems.Add(menuBarItem);

		VerticalStackLayout layout = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					AutomationId = "Issue16119DescriptionLabel",
					Text = "The test passes if the MenuFlyoutItem's icon is in original color instead of monochrome",
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 12
				},
			}
		};

		AddContentPage(new ContentPage { Content = layout });
		FlyoutBehavior = FlyoutBehavior.Disabled;
	}
}