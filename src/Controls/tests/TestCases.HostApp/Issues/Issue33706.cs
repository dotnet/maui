using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33706, "MediaPicker gets stuck if LaunchMode is SingleTask", PlatformAffected.Android)]
public class Issue33706 : ContentPage
{
	private readonly ActivityIndicator _activityIndicator;
	private readonly Label _statusLabel;

	public Issue33706()
	{
		var pickButton = new Button
		{
			Text = "Pick Media",
			AutomationId = "PickMediaButton"
		};
		pickButton.Clicked += OnPickMediaClicked;

		_activityIndicator = new ActivityIndicator
		{
			AutomationId = "MediaPickerIndicator",
			IsVisible = false,
			IsRunning = false
		};

		_statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "Test MediaPicker with LaunchMode.SingleTask",
					FontSize = 18,
					HorizontalOptions = LayoutOptions.Center
				},
				pickButton,
				_activityIndicator,
				_statusLabel
			}
		};
	}

	private async void OnPickMediaClicked(object sender, EventArgs e)
	{
		try
		{
			_activityIndicator.IsRunning = true;
			_activityIndicator.IsVisible = true;
			_statusLabel.Text = "Picking...";

			var result = await MediaPicker.PickPhotoAsync();

			_activityIndicator.IsRunning = false;
			_activityIndicator.IsVisible = false;

			if (result != null)
			{
				_statusLabel.Text = "Photo picked";
			}
			else
			{
				_statusLabel.Text = "Cancelled";
			}
		}
		catch (Exception ex)
		{
			_activityIndicator.IsRunning = false;
			_activityIndicator.IsVisible = false;
			_statusLabel.Text = $"Error: {ex.Message}";
		}
	}
}
