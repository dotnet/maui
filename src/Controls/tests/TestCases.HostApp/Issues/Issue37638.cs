using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37638, "Screenshot.CaptureAsync deadlocks when awaited synchronously from the UI thread", PlatformAffected.Android)]
public class Issue37638 : ContentPage
{
	readonly Label _statusLabel;

	public Issue37638()
	{
		_statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = "StatusLabel",
		};

		var captureButton = new Button
		{
			Text = "Generate error",
			AutomationId = "GenerateErrorButton",
		};
		captureButton.Clicked += OnGenerateErrorClicked;

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 20,
			Children =
			{
				captureButton,
				_statusLabel,
				new Editor
				{
					Placeholder = "Check if app still responds",
					AutomationId = "ResponsivenessEditor",
				},
			},
		};
	}

	void OnGenerateErrorClicked(object sender, EventArgs e)
	{
		CaptureTestException();
	}

	void CaptureTestException()
	{
		try
		{
			throw new InvalidOperationException("Screenshot deadlock test");
		}
		catch (Exception ex)
		{
			_statusLabel.Text = $"Capturing error: {ex.Message}";
			string eventId = CaptureException(ex);
			_statusLabel.Text = $"Error captured: {eventId}";
		}
	}

	static string CaptureException(Exception exception)
	{
		_ = exception ?? throw new ArgumentNullException(nameof(exception));

		var screenshot = Screenshot.Default.CaptureAsync().GetAwaiter().GetResult();
		return screenshot is null ? string.Empty : Guid.NewGuid().ToString("N");
	}
}
