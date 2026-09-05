namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34830, "[iOS/Mac] FlyoutPage RTL FlowDirection is not working properly", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34830 : TestFlyoutPage
{
	protected override void Init()
	{
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		FlowDirection = FlowDirection.RightToLeft;

		Flyout = new ContentPage
		{
			Title = "Flyout",
			BackgroundColor = Colors.SkyBlue,
			IconImageSource = "menu_icon",
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "If you can see me the test has passed",
						AutomationId = "CloseRootView",
						Command = new Command(() => IsPresented = false)
					}
				},
				AutomationId = "RootLayout"
			},
			Padding = new Thickness(0, 42, 0, 0)
		};

		Detail = new NavigationPage(new ContentPage
		{
			Title = "Detail",
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "The page must be with RightToLeft FlowDirection. Hamburger icon in main page must be on the right side. There should be visible text inside the Flyout View"
					},
					new Button
					{
						Text = "Set RightToLeft",
						Command = new Command(() => FlowDirection = FlowDirection.RightToLeft),
						AutomationId = "ShowRightToLeft"
					},
					new Button
					{
						Text = "Set LeftToRight",
						Command = new Command(() => FlowDirection = FlowDirection.LeftToRight),
						AutomationId = "ShowLeftToRight"
					},
					new Button
					{
						Text = "Open Flyout View",
						Command = new Command(() => IsPresented = true),
						AutomationId = "OpenRootView"
					},
					new Label
					{
						Text = DeviceInfo.Idiom.ToString(),
						AutomationId = "Idiom"
					}
				}
			}
		});
	}
}