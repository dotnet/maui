namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35416, "[Android] Shell.FlyoutHeader background is incorrect", PlatformAffected.Android)]
public class Issue35416 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
		FlyoutBackground = Colors.Violet;

		FlyoutHeader = new Label
		{
			AutomationId = "Issue35416FlyoutHeader",
			Text = "Flyout Header",
			HeightRequest = 80,
			VerticalTextAlignment = TextAlignment.Center,
			HorizontalTextAlignment = TextAlignment.Center,
			TextColor = Colors.White,
		};

		ContentPage page = new ContentPage
		{
			Title = "Home",
			Content = new Label
			{
				AutomationId = "Issue35416Label",
				Text = "Issue35416 Home",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			},
		};

		Items.Add(new FlyoutItem { Title = "Home", Route = "home", Items = { new ShellContent { Content = page } } });
		Items.Add(new FlyoutItem { Title = "About", Route = "about", Items = { new ShellContent { Content = new ContentPage { Title = "About" } } } });
	}
}
