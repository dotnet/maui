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
			bool _popScheduled;

			public PickerPage()
			{
				var picker = new Picker
				{
					Title = "Select a color",
					ItemsSource = new List<string> { "Red", "Green", "Blue", "Yellow", "Purple" },
					AutomationId = "colorPicker"
				};

				picker.Focused += OnPickerFocused;

				var instructionsLabel = new Label
				{
					Text = "Tap the picker to open the dialog. The page will automatically pop back once the dialog opens. The app should not crash.",
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
			}

			// Pop the page shortly after the picker dialog actually opens, so the
			// "page popped while the dialog is open" scenario is reproduced
			// deterministically. The previous implementation started a fixed
			// 3-second wall-clock timer in OnNavigatedTo, which raced the UI test:
			// under CI load the page could pop before the test managed to tap the
			// picker, producing StaleElementReferenceException / TimeoutException
			// instead of exercising the real scenario.
			private void OnPickerFocused(object sender, FocusEventArgs e)
			{
				if (!e.IsFocused || _popScheduled)
					return;

				_popScheduled = true;

				_ = Task.Run(async () =>
				{
					await Task.Delay(1000);
					await Dispatcher.DispatchAsync(async () =>
					{
						await Navigation.PopToRootAsync();
					});
				});
			}
		}
	}
}