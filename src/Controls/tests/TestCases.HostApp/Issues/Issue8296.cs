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
			internal static bool BackButtonPressedCalledReturnFalse;
			readonly Label _returnFalseStatusLabel;

			public MainPage()
			{
				Title = "HomePage";
				BackButtonPressedCalledReturnFalse = false;

				var navigateButton = new Button
				{
					Text = "Go to Second Page",
					AutomationId = "NavigateButton"
				};

				navigateButton.Clicked += async (s, e) =>
					await Navigation.PushAsync(new SecondPage());

				var navigateReturnFalseButton = new Button
				{
					Text = "Go to Return False Page",
					AutomationId = "NavigateReturnFalseButton"
				};

				navigateReturnFalseButton.Clicked += async (s, e) =>
					await Navigation.PushAsync(new ReturnFalsePage());

				_returnFalseStatusLabel = new Label
				{
					Text = "Waiting",
					AutomationId = "ReturnFalseStatusLabel"
				};

				Content = new VerticalStackLayout
				{
					Children = { navigateButton, navigateReturnFalseButton, _returnFalseStatusLabel }
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				if (BackButtonPressedCalledReturnFalse)
				{
					_returnFalseStatusLabel.Text = "OnBackButtonPressed Called And Returned False";
					BackButtonPressedCalledReturnFalse = false;
				}
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

		public class ReturnFalsePage : ContentPage
		{
			public ReturnFalsePage()
			{
				Title = "ReturnFalsePage";

				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label
						{
							Text = "Press back to test OnBackButtonPressed returning false",
							AutomationId = "ReturnFalsePageLabel"
						}
					}
				};
			}

			protected override bool OnBackButtonPressed()
			{
				MainPage.BackButtonPressedCalledReturnFalse = true;
				// Return false to allow navigation to proceed
				return false;
			}
		}
	}
}
