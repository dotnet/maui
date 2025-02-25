namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 42074, "[Android] Clicking cancel on a TimePicker does not cause it to unfocus", PlatformAffected.Android)]
public class Bugzilla42074 : TestContentPage
{
	const string TimePicker = "TimePicker";

	protected override void Init()
	{
		var timePicker = new TimePicker
		{
			AutomationId = TimePicker,
			HorizontalOptions = LayoutOptions.Center
		};
		var timePickerFocusButton = new Button
		{
			Text = "Click to focus TimePicker",
			AutomationId = "focusbtn",
			Command = new Command(() => timePicker.Focus())
		};
		Content = new StackLayout
		{
			Children =
			{
				timePicker,
				timePickerFocusButton
			}
		};
	}
}
