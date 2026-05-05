namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35279,
	"MenuBarItem named 'Edit' (or any system menu title) should merge its items into the existing system menu on Mac Catalyst",
	PlatformAffected.MacCatalyst)]
public class Issue35279 : TestShell
{
	protected override void Init()
	{
		var resultLabel = new Label
		{
			Text = "No action performed yet",
			AutomationId = "ResultLabel"
		};

		// A MenuBarItem whose title matches the system "Edit" menu.
		// On Mac Catalyst the items inside should be merged into (and visible in)
		// the existing system Edit menu, not silently dropped because a duplicate
		// menu was created.
		var editMenu = new MenuBarItem { Text = "Edit" };
		var copyItem = new MenuFlyoutItem
		{
			Text = "Copy",
			AutomationId = "CopyMenuItem",
			Command = new Command(() => resultLabel.Text = "Copy executed")
		};
		editMenu.Add(copyItem);
		MenuBarItems.Add(editMenu);

		AddContentPage(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Open the Edit menu in the Mac menu bar and tap 'Copy'.",
						HorizontalTextAlignment = TextAlignment.Center
					},
					resultLabel
				}
			}
		});

		FlyoutBehavior = FlyoutBehavior.Disabled;
	}
}
