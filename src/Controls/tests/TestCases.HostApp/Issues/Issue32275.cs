namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32275, "[NET10] SafeAreaEdges cannot be set for Shell and the flyout menu collides with display notch and status bar in landscape mode", PlatformAffected.Android)]
public class Issue32275 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		for (int i = 0; i < 6; i++)
		{
			Items.Add(new FlyoutItem
			{
				Title = "Issue 32275",
				Items =
				{
					new ShellContent
					{
						Content = new Issue32275ContentPage(),
					}
				}
			});
		}
	}

	public class Issue32275ContentPage : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 32275";
			Content = new StackLayout
			{
				Children =
				{
					new Label { AutomationId = "Issue32275Label", Text = "Open the flyout menu in landscape mode on a device with a display notch or status bar. The flyout menu should not collide with the notch or status bar." }
				}
			};
		}
	}
}


