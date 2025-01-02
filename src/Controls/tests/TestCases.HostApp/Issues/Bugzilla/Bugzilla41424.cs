namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 41424, "[Android] Clicking cancel on a DatePicker does not cause it to unfocus", PlatformAffected.Android)]
public class Bugzilla41424 : TestContentPage
{
	const string DatePicker = "DatePicker";

	protected override void Init()
	{
		var stepsTitleLabel = new Label() { Text = "Test steps:" };
		var step1Label = new Label() { Text = "• Click 'Click to focus DatePicker'" };
		var step2Label = new Label() { Text = "• Click 'Cancel' or back button" };
		var step3Label = new Label() { Text = "• Click 'Click to focus DatePicker'" };
		var step4Label = new Label() { Text = "• Check that date selector appears" };
		var datePickerFocusStateLabel = new Label() { Text = "FocusStateLabel", AutomationId = "FocusStateLabel" };
		var datePicker = new DatePicker
		{
			AutomationId = DatePicker,
			HorizontalOptions = LayoutOptions.Center
		};
		datePicker.Focused += (sender, args) => { datePickerFocusStateLabel.Text = "focused"; };

		var datePickerFocusButton = new Button
		{
			Text = "Click to focus DatePicker",
			AutomationId = "ProgrammaticFocus",
			Command = new Command(() => datePicker.Focus())
		};

		var getDatePickerFocusStateButton = new Button
		{
			Text = "Click to view focus state",
			AutomationId = "GetFocusState",
			Command = new Command(() =>
			{
				datePickerFocusStateLabel.Text = datePicker.IsFocused ? "focused" : "unfocused";
			})
		};

		Content = new StackLayout
		{
			Children =
			{
				stepsTitleLabel,
				step1Label,
				step2Label,
				step3Label,
				step4Label,
				datePicker,
				datePickerFocusButton,
				getDatePickerFocusStateButton,
				datePickerFocusStateLabel
			}
		};
	}
}
