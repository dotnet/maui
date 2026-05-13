using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32807, "Alert popup not displaying when dismissing modal page on iOS/MacOS", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue32807 : ContentPage
	{
		public Issue32807()
		{
			InitializeComponent();
		}

		async void OnOpenModalClicked(object sender, EventArgs e)
		{
			// Present a modal page
			var modalPage = new ModalTestPage();
			await Navigation.PushModalAsync(modalPage);

			// Wait for the modal to be dismissed
			await modalPage.DismissedTask;

			// Try to show alerts immediately after dismissal
			// Without the fix, these alerts won't appear because GetTopUIViewController
			// returns the dismissing modal view controller instead of the main view controller
			StatusLabel.Text = "Modal dismissed, showing alerts...";

			await DisplayAlert("Alert 1", "First alert after modal dismissal", "OK");
			StatusLabel.Text = "Alert 1 shown";

			await DisplayAlert("Alert 2", "Second alert after modal dismissal", "OK");
			StatusLabel.Text = "Alert 2 shown";

			await DisplayAlert("Alert 3", "Third alert after modal dismissal", "OK");
			StatusLabel.Text = "All alerts shown successfully!";
		}
	}

	public class ModalTestPage : ContentPage
	{
		private TaskCompletionSource<bool> _dismissedTcs = new TaskCompletionSource<bool>();

		public Task DismissedTask => _dismissedTcs.Task;

		public ModalTestPage()
		{
			Title = "Modal Page";

			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label { Text = "This is a modal page", FontSize = 18 },
					new Button
					{
						Text = "Dismiss",
						AutomationId = "DismissButton",
						Command = new Command(async () =>
						{
							await Navigation.PopModalAsync();
							_dismissedTcs.TrySetResult(true);
						})
					}
				}
			};
		}
	}
}
