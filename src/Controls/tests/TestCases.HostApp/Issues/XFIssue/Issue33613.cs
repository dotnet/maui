namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33613, "Mac Catalyst: NavigationPage.TitleView layout shifts and adds extra spacing when window is maximized", PlatformAffected.macOS)]
public class Issue33613NavPage : NavigationPage
{
	public Issue33613NavPage() : base(new Issue33613MainPage())
	{
	}
}

public class Issue33613MainPage : ContentPage
{
	public Issue33613MainPage()
	{
		Title = "Main Page";
		
		var navigateButton = new Button
		{
			Text = "Navigate to Page with TitleView",
			AutomationId = "NavigateButton"
		};
		
		navigateButton.Clicked += async (s, e) =>
		{
			await Navigation.PushAsync(new Issue33613());
		};
		
		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Issue #33613 - TitleView Layout Test",
					FontSize = 20,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					AutomationId = "MainPageLabel"
				},
				new Label
				{
					Text = "Tap the button to navigate to a page with a custom TitleView",
					FontSize = 14,
					HorizontalTextAlignment = TextAlignment.Center
				},
				navigateButton
			}
		};
	}
}

public class Issue33613 : ContentPage
{
	public Issue33613()
	{
		Title = "Issue 33613";
		var titleViewGrid = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			HorizontalOptions = LayoutOptions.FillAndExpand,
			AutomationId = "TitleViewGrid"
		};
		
		var titleLabel = new Label
		{
			Text = "Custom TitleView",
			TextColor = Colors.Black,
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "TitleLabel"
		};
		
		titleViewGrid.Children.Add(titleLabel);
		NavigationPage.SetTitleView(this, titleViewGrid);
		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
		};
	}
}
