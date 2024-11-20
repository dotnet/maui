namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 12126, "",
	PlatformAffected.iOS)]
public class Issue12126 : TestShell
{
	bool firstNavigated = true;
	protected override void Init()
	{
		var page1 = AddFlyoutItem("Tab 1");
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
				Text = "If you don't see any bottom tabs the test has failed"
			};
			Shell.SetTabBarIsVisible(contentPage, true);

			ContentPage contentPage2 = new ContentPage();
			contentPage2.Content =
				new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Click The Back Arrow",
							AutomationId = "TestReady"
						}
					}
				};

			Shell.SetTabBarIsVisible(contentPage2, false);
			await Navigation.PushAsync(contentPage);
			await Navigation.PushAsync(contentPage2);
		}
	}
}
