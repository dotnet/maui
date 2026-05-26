namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31372, "IsPresented=true Not Working on Initial Value in FlyoutPage", PlatformAffected.UWP | PlatformAffected.macOS)]

public class Issue31372 : NavigationPage
{
	public Issue31372()
	{
		PushAsync(new Issue31372Page());
	}
}

public class Issue31372Page : FlyoutPage
{
	public Issue31372Page()
	{

		// Create Flyout Page
		var flyoutPage = new ContentPage
		{
			Title = "Flyout",
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "This is Flyout",
						AutomationId = "FlyoutLabel"
					}
				}
			}
		};

		// Create Detail Page
		var detailPage = new ContentPage
		{
			Title = "Detailpage",
			Content = new StackLayout
			{
				Padding = 16,
				Spacing = 16,
				Children =
				{
					new Label
					{
						Text = "This is Detail Page which displays additional information."
					},
				}
			}
		};

		// Assign Flyout and Detail
		Flyout = flyoutPage;
		Detail = new NavigationPage(detailPage);
		FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
		IsPresented = true;
	}
}