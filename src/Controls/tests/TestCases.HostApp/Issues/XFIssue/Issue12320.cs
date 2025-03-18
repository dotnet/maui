namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 12320, "[iOS] TabBarIsVisible = True/False doesn't work on Back Navigation When using BackButtonBehavior",
	PlatformAffected.iOS)]

public class Issue12320 : TestShell
{
	bool firstNavigated = true;
	protected override void Init()
	{
		var page1 = new ContentPage();
		page1.Content = new Label()
		{
			Text = "If you don't see any bottom tabs the test has failed"
		};

		AddFlyoutItem(page1, "Tab 1");
		AddBottomTab("Tab 2");
		Shell.SetTabBarIsVisible(page1, true);
	}

	protected override async void OnNavigated(ShellNavigatedEventArgs args)
	{
		base.OnNavigated(args);

		if (firstNavigated)
		{
			firstNavigated = false;
			ContentPage contentPage = new ContentPage();
			contentPage.Content = new Label()
			{
				Text = "Click the Coffee Cup in the Nav Bar",
				AutomationId = "TestReady"
			};

			Shell.SetTabBarIsVisible(contentPage, false);
			Shell.SetBackButtonBehavior(contentPage, new BackButtonBehavior
			{
				IconOverride = new FileImageSource
				{
					AutomationId = "BackButtonImage",
					File = "coffee.png"
				}
			});
			await Navigation.PushAsync(contentPage);
		}
	}
}
