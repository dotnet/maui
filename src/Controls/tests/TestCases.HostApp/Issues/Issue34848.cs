namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34848, "DatePicker Opened and Closed events are not raised on MacCatalyst", PlatformAffected.macOS)]
public class Issue34848 : TestContentPage
{
	DatePicker _datePicker;
	Label _OpenstatusLabel;
	Label _ClosestatusLabel;
	protected override void Init()
	{
		_OpenstatusLabel = new Label
		{
			AutomationId = "Issue34848OpenStatusLabel",
			Text = "Opened: Unknown",
			HorizontalOptions = LayoutOptions.Center
		};

		_ClosestatusLabel = new Label
		{
			AutomationId = "Issue34848CloseStatusLabel",
			Text = "Closed: Unknown",
			HorizontalOptions = LayoutOptions.Center
		};

		_datePicker = new DatePicker
		{
			AutomationId = "Issue34848TestDatePicker",
			Date = DateTime.Today,
			HorizontalOptions = LayoutOptions.Center
		};

		_datePicker.Opened += (s, e) => _OpenstatusLabel.Text = "Opened";
		_datePicker.Closed += (s, e) => _ClosestatusLabel.Text = "Closed";

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children =
			{
				_OpenstatusLabel,
				_ClosestatusLabel,
				_datePicker
			}
		};
	}
}
