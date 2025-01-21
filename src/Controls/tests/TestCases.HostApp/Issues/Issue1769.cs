namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1769, "PushAsync with Switch produces NRE", PlatformAffected.Android)]
	public class Issue1769 : NavigationPage
	{
		public Issue1769() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string GoToPageTwoButtonText = "Go To Page 2";
			const string SwitchLabelText = "Switch";
			const string SwitchAutomatedId = nameof(SwitchAutomatedId);
			const string SwitchIsNowLabelTextFormat = "Switch is now {0}";

			public MainPage()
			{
				var button = new Button()
				{
					AutomationId = GoToPageTwoButtonText,
					Text = GoToPageTwoButtonText
				};
				button.Clicked += async (sender, args) =>
				{
					await ((Button)sender).Navigation.PushAsync(new SwitchDemoPage());
				};

				Content = button;
			}

			class SwitchDemoPage : ContentPage
			{
				readonly Label _label;

				public SwitchDemoPage()
				{
					var header = new Label
					{
						Text = SwitchLabelText,
						FontSize = 50,
						HorizontalOptions = LayoutOptions.Center
					};


					var switcher = new Switch
					{
						AutomationId = SwitchAutomatedId,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					};

					switcher.Toggled += switcher_Toggled;

					_label = new Label
					{
						AutomationId = "SwitchLabel",
						Text = string.Format(SwitchIsNowLabelTextFormat, switcher.IsToggled),
						FontSize = 20,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					};

					// Accomodate iPhone status bar.
					Padding = DeviceInfo.Platform == DevicePlatform.iOS ? new Thickness(10, 20, 10, 5) : new Thickness(10, 0, 10, 5);

					// Build the page.
					Content = new StackLayout
					{
						Children =
					{
						header,
						switcher,
						_label
					}
					};
				}

				void switcher_Toggled(object sender, ToggledEventArgs e)
				{
					_label.Text = string.Format(SwitchIsNowLabelTextFormat, e.Value);
				}
			}
		}
	}
}