namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33615, "[Android] Title of FlyOutPage is not updating anymore after showing a NonFlyOutPage", PlatformAffected.Android)]
public class Issue33615 : TestFlyoutPage
{
	NavigationPage _navigationPage;

	protected override void Init()
	{
		Flyout = new ContentPage
		{
			Title = "Menu",
			Content = new Label { Text = "Flyout Menu" }
		};

		var detailPage1 = new ContentPage { Title = "DetailPage1" };
		_navigationPage = new NavigationPage(detailPage1);
		Detail = _navigationPage;

		detailPage1.Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Button
				{
					Text = "Show NonFlyoutPage",
					AutomationId = "ShowNonFlyoutButton",
					Command = new Command(() =>
					{
						this.Window.Page = new Issue33615NonFlyoutPage(this);
					})
				},
				new Button
				{
					Text = "Navigate to DetailPage2",
					AutomationId = "NavigateToDetailPage2Button",
					Command = new Command(async () =>
					{
						await _navigationPage.PushAsync(new ContentPage
						{
							Title = "DetailPage2",
							Content = new Label
							{
								Text = "DetailPage2",
								AutomationId = "DetailPage2Label"
							}
						});
					})
				},
				new Label
				{
					Text = "DetailPage1",
					AutomationId = "DetailPage1Label"
				}
			}
		};
	}
}

class Issue33615NonFlyoutPage : ContentPage
{
	public Issue33615NonFlyoutPage(Page flyoutPage)
	{
		Title = "NonFlyoutPage";
		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label { Text = "NonFlyoutPage" },
				new Button
				{
					Text = "Back to FlyoutPage",
					AutomationId = "BackToFlyoutButton",
					Command = new Command(() => this.Window.Page = flyoutPage)
				}
			}
		};
	}
}
