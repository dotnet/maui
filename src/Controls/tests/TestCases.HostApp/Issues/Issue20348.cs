namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20348, "SearchBar text incorrectly copied between multiple SearchBars on Android after back navigation", PlatformAffected.Android)]
public class Issue20348 : NavigationPage
{
	public Issue20348() : base(new MainPage())
	{
	}

	public class MainPage : ContentPage
	{
		public MainPage()
		{
			SearchBar firstSearchBar = new SearchBar
			{
				AutomationId = "FirstSearchBar"
			};

			Label firstSearchBarTextLabel = new Label
			{
				AutomationId = "FirstSearchBarText",
				Text = "Pass"
			};

			firstSearchBar.TextChanged += (s, e) =>
			{
				firstSearchBarTextLabel.Text = string.IsNullOrEmpty(e.NewTextValue)
					? "Pass"
					: "Fail";
			};

			SearchBar secondSearchBar = new SearchBar
			{
				AutomationId = "SecondSearchBar"
			};

			Button navigateButton = new Button
			{
				Text = "Navigate",
				AutomationId = "NavigateButton"
			};

			navigateButton.Clicked += async (s, e) =>
			{
				await Navigation.PushAsync(new SecondPage());
			};

			Label descriptionLabel = new Label
			{
				Text = "Test passes if SecondSearchBar text is NOT applied to FirstSearchBar after typing in SecondSearchBar, navigating to page 2, and pressing back.",
				FontSize = 12,
				TextColor = Colors.Gray,
				HorizontalTextAlignment = TextAlignment.Center
			};

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children = { descriptionLabel, firstSearchBar, firstSearchBarTextLabel, secondSearchBar, navigateButton }
			};
		}
	}

	public class SecondPage : ContentPage
	{
		public SecondPage()
		{
			Title = "Second Page";
			Content = new Label
			{
				Text = "Second Page",
				AutomationId = "SecondPageLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}
