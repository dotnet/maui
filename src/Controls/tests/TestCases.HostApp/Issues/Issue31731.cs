namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31731, "Picker dialog causes crash when page is popped while dialog is open", PlatformAffected.Android)]
	public class Issue31731 : NavigationPage
	{
		public Issue31731() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				var button = new Button
				{
					Text = "Navigate to Picker Page",
					AutomationId = "navigateButton"
				};

				button.Clicked += OnNavigateClicked;

				var statusLabel = new Label
				{
					Text = "Status: Ready",
					AutomationId = "statusLabel"
				};

				Content = new StackLayout
				{
					Padding = new Thickness(20),
					Children = { statusLabel, button }
				};
			}

			private void OnNavigateClicked(object sender, EventArgs e)
			{
				Navigation.PushAsync(new PickerPage());
			}
		}

		public class PickerPage : ContentPage
		{
			public PickerPage()
			{
				var picker = new Picker
				{
					Title = "Select a color",
					ItemsSource = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" },
					AutomationId = "colorPicker"
				};

				var instructionsLabel = new Label
				{
					Text = "Tap the picker to open the dialog, then wait for auto navigation back (3 seconds). The app should not crash.",
					AutomationId = "instructionsLabel",
					Margin = new Thickness(0, 0, 0, 20)
				};

				var statusLabel = new Label
				{
					Text = "Status: Page loaded",
					AutomationId = "pageStatusLabel"
				};

				Content = new StackLayout
				{
					Padding = new Thickness(20),
					Children = { instructionsLabel, statusLabel, picker }
				};
			}

			protected override void OnNavigatedTo(NavigatedToEventArgs args)
			{
				base.OnNavigatedTo(args);

				// Simulate the scenario: navigate back after 3 seconds
				// This can cause a crash if the picker dialog is open
				_ = Task.Run(async () =>
				{
					await Task.Delay(3000);
					await Dispatcher.DispatchAsync(async () =>
					{
						await Navigation.PopToRootAsync();
					});
				});
			}
		}
	}
}