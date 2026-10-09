namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39155, "[iOS 27] NavigationPage.TitleView is 0pt tall (invisible) when pushed onto a NavigationPage whose bar was never laid out", PlatformAffected.iOS)]
public class Issue39155 : NavigationPage
{
	public Issue39155() : base(new Issue39155MainPage())
	{

	}
}

public partial class Issue39155MainPage : ContentPage
{
	public Issue39155MainPage()
	{
		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20, 80),
			Spacing = 12,
			Children =
			{
				new Label { Text = "Start page" },
				new Button
				{
					Text = "1. Replace Window.Page",
					AutomationId = "Replace Window.Page",
					Command = new Command(ReplaceWindowPage)
				}
			}
		};
	}

	private void ReplaceWindowPage()
	{
		if (Window is not Window window)
			return;

		window.Page = CreateFlyoutRoot();
	}

	private static Page CreateFlyoutRoot()
	{
		var home = new ContentPage { Title = "Home" };

		NavigationPage.SetHasNavigationBar(home, false);

		var flyoutPage = new FlyoutPage
		{
			Flyout = new ContentPage { Title = "Menu" },
			Detail = new NavigationPage(home)
		};

		NavigationPage.SetHasNavigationBar(flyoutPage, false);

		var outerNavigationPage = new NavigationPage(flyoutPage);

		home.Content = new VerticalStackLayout
		{
			Padding = new Thickness(20, 80),
			Spacing = 12,
			Children =
			{
				new Label { Text = "Home page (navigation bar hidden)" },
				new Button
				{
					Text = "2. Push page with TitleView",
					AutomationId = "PushTitleViewPage",
					Command = new Command(async () =>
					{
						await outerNavigationPage.PushAsync(
							new Issue39155TitleviewPage());
					})
				}
			}
		};

		return outerNavigationPage;
	}
}

public class Issue39155TitleviewPage : ContentPage
{
	private readonly Label _titleView;
	private readonly Label _heightLabel;

	public Issue39155TitleviewPage()
	{
		Title = "TitleView Test";

		_titleView = new Label
		{
			Text = "TitleView",
			BackgroundColor = Colors.Orange,
			TextColor = Colors.Black,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center
		};

		NavigationPage.SetTitleView(this, _titleView);

		_heightLabel = new Label();

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				new Label
				{
					Text = "Expected: an orange TitleView in the navigation bar."
				},
				_heightLabel
			}
		};
	}
}