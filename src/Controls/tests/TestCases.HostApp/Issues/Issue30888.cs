namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30888, "Flyout page toolbar items not rendered on iOS", PlatformAffected.iOS)]
public class Issue30888 : TestFlyoutPage
{
	protected override void Init()
	{
		var flyoutPage = new ContentPage
		{
			Title = "Flyout",
			BackgroundColor = Colors.Blue
		};

		flyoutPage.ToolbarItems.Add(new ToolbarItem
		{
			Text = "Flyout",
			IconImageSource = "dotnet_bot.png",
		});

		flyoutPage.Content = new Grid
		{
			Children = {
				new Label
				{
					Text = "Flyout Content",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
			}
		};

		var detailPage = new ContentPage
		{
			Title = "Detail",
		};

		detailPage.Content = new Grid
		{
			Children = {
				new Label
				{
					Text = "Detail Content",
					AutomationId = "DetailContent",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				},
			}
		};

		Flyout = flyoutPage;
		Detail = new NavigationPage(detailPage);
	}
}