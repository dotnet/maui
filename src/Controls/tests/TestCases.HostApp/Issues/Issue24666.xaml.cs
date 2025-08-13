namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24666, "Differentiate Left/Right click in PointerGestureRecognizer", PlatformAffected.All)]
public partial class Issue24666 : ContentPage
{
	private int _primaryEventCount = 0;
	private int _secondaryEventCount = 0;
	private int _combinedEventCount = 0;

	public Issue24666()
	{
		InitializeComponent();
	}

	private void OnPrimaryButtonPressed(object sender, PointerEventArgs e)
	{
		_primaryEventCount++;
		PrimaryEventLabel.Text = $"Events: {_primaryEventCount}";
		LogEvent("Primary Button Pressed");
	}

	private void OnPrimaryButtonReleased(object sender, PointerEventArgs e)
	{
		LogEvent("Primary Button Released");
	}

	private void OnSecondaryButtonPressed(object sender, PointerEventArgs e)
	{
		_secondaryEventCount++;
		SecondaryEventLabel.Text = $"Events: {_secondaryEventCount}";
		LogEvent("Secondary Button Pressed");
	}

	private void OnSecondaryButtonReleased(object sender, PointerEventArgs e)
	{
		LogEvent("Secondary Button Released");
	}

	private void OnCombinedButtonPressed(object sender, PointerEventArgs e)
	{
		_combinedEventCount++;
		CombinedEventLabel.Text = $"Events: {_combinedEventCount}";
		LogEvent($"Combined Button Pressed (Button: {e.Button})");
	}

	private void OnCombinedButtonReleased(object sender, PointerEventArgs e)
	{
		LogEvent($"Combined Button Released (Button: {e.Button})");
	}

	private void LogEvent(string eventName)
	{
		var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
		EventLogLabel.Text += $"[{timestamp}] {eventName}\n";
	}
}
