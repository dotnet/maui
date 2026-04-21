namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 8296,
		"ContentPage.OnBackButtonPressed is not invoked on iOS and MacCatalyst with NavigationPage",
		PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue8296 : NavigationPage
	{
		public Issue8296() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Title = "HomePage";

				var navigateButton = new Button
				{
					Text = "Go to Second Page",
					AutomationId = "NavigateButton"
				};

				navigateButton.Clicked += async (s, e) =>
					await Navigation.PushAsync(new SecondPage());

				Content = new VerticalStackLayout
				{
					Children = { navigateButton }
				};
			}
		}

		public class SecondPage : ContentPage
		{
			public SecondPage()
			{
				Title = "SecondPage";

				var statusLabel = new Label
				{
					Text = "OnBackButtonPressed Not Called",
					AutomationId = "BackButtonPressedLabel"
				};

				Content = new VerticalStackLayout
				{
					Children = { statusLabel }
				};
			}

			protected override bool OnBackButtonPressed()
			{
				var label = (Label)((VerticalStackLayout)Content).Children[0];
				label.Text = "OnBackButtonPressed Called";
				// Return true to prevent navigation so the label stays visible for test verification
				return true;
			}
		}
	}
}
