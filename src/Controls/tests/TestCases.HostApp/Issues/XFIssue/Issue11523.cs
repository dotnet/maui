namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 11523, "[Bug] FlyoutBehavior.Disabled removes back-button from navbar",
	PlatformAffected.iOS)]
public class Issue11523 : TestShell
{
	protected override async void Init()
	{
		ContentPage contentPage = new ContentPage()
		{
			Content =
				new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "This Page Should Have a Back Button that you should click",
							AutomationId = "PageLoaded"
						}
					}
				}
		};

		var firstPage = AddBottomTab("First Page");
		firstPage.Content =
				new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "This Page Should Have a Hamburger Menu Icon when you return to it",

						}
					}
				};

		await Task.Delay(1000);

		contentPage.Appearing += (_, __) =>
		{
			this.FlyoutBehavior = FlyoutBehavior.Disabled;
		};

		contentPage.Disappearing += (_, __) =>
		{
			this.FlyoutBehavior = FlyoutBehavior.Flyout;
		};

		await Navigation.PushAsync(contentPage);
	}
}
