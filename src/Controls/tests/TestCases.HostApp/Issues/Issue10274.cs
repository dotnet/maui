namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 10274, "MAUI Flyout does not work on Android when not using Shell", PlatformAffected.Android)]
	public class Issue10274 : NavigationPage
	{
		public Issue10274() : base(new MainPage())
		{

		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var button = new Button
				{
					Text = "Navigate to Flyout Page",
					AutomationId = "mainPageButton"
				};

				button.Clicked += OnNavigateToFlyoutPageClicked;

				var label = new Label
				{
					Text = "This is MainPage",
					AutomationId = "mainPageLabel"
				};

				Content = new StackLayout
				{
					Padding = new Thickness(20),
					Children = { label, button }
				};
			}
			private async void OnNavigateToFlyoutPageClicked(object sender, EventArgs e)
			{
				await Navigation.PushAsync(new CustomFlyoutPage());
			}
		}

		public class CustomFlyoutPage : FlyoutPage
		{
			public CustomFlyoutPage()
			{
				Flyout = new ContentPage
				{
					Title = "Flyout",
					Content = new StackLayout
					{
						Padding = new Thickness(20),
						Children =
						{
							new Label { Text = "This is the Flyout page." }
						}
					}
				};

				var button = new Button
				{
					Text = "Go Back",
					AutomationId = "flyoutPageButton"
				};
				button.Clicked += OnGoBackClicked;

				var label = new Label
				{
					Text = "This is FlyoutPage",
					AutomationId = "flyoutPageLabel"
				};

				var detailPage = new ContentPage
				{
					Title = "Detail",
					Content = new StackLayout
					{
						Padding = new Thickness(20),
						Children = { label, button }
					}
				};

				Detail = new NavigationPage(detailPage);
			}

			private async void OnGoBackClicked(object sender, EventArgs e)
			{
				await Navigation.PopAsync();
			}
		}
	}
}
