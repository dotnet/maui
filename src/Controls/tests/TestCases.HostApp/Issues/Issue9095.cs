namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 9095,
	"Shell toolbar back button doesn't fire Shell.OnBackButtonPressed on Android and iOS",
	PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue9095 : TestShell
{
	protected override void Init()
	{
		Routing.RegisterRoute(nameof(Issue9095SecondPage), typeof(Issue9095SecondPage));
		AddContentPage(new Issue9095RootPage());
	}

	protected override bool OnBackButtonPressed()
	{
		if (CurrentPage is Issue9095SecondPage secondPage)
		{
			secondPage.UpdateStatus("OnBackButtonPressed Called");
		}
		return true;
	}


	public class Issue9095RootPage : ContentPage
	{
		public Issue9095RootPage()
		{
			Title = "HomePage";

			var navigateButton = new Button
			{
				Text = "Go to Second Page",
				AutomationId = "NavigateButton"
			};

			navigateButton.Clicked += async (s, e) =>
				await Shell.Current.GoToAsync(nameof(Issue9095SecondPage));

			Content = new VerticalStackLayout
			{
				Children = { navigateButton }
			};
		}
	}

	public class Issue9095SecondPage : ContentPage
	{
		readonly Label _statusLabel;

		public Issue9095SecondPage()
		{
			Title = "Second Page";

			_statusLabel = new Label
			{
				Text = "OnBackButtonPressed Not Called",
				AutomationId = "BackButtonPressedLabel"
			};

			Content = new VerticalStackLayout
			{
				Children = { _statusLabel }
			};
		}

		public void UpdateStatus(string text)
		{
			_statusLabel.Text = text;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
		}

	}
}
