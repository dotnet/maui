namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31124, "DatePicker should maintain today's date when set to null", PlatformAffected.macOS)]
public class Issue31124 : ContentPage
{
	DatePicker _datePicker;
	Button _setNullButton;
	Label _dateLabel;

	public Issue31124()
	{
		InitializeComponent();
	}

	void InitializeComponent()
	{
		Title = "DatePicker Null Test";
			
		_datePicker = new DatePicker
		{
			AutomationId = "TestDatePicker",
			Date = DateTime.Today
		};

		_setNullButton = new Button
		{
			Text = "Set Date to Null",
			AutomationId = "SetNullButton"
		};
		_setNullButton.Clicked += Button_Clicked;

		_dateLabel = new Label
		{
			Text = $"Current Date: {_datePicker.Date:d}",
			AutomationId = "DateLabel"
		};

		// Listen for date changes to update the label
		_datePicker.DateSelected += (s, e) =>
		{
			_dateLabel.Text = $"Current Date: {e.NewDate:d}";
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				new Label { Text = "Test: Setting DatePicker.Date to null should remove today's date" },
				_datePicker,
				_setNullButton,
				_dateLabel,
				new Label 
				{ 
					Text = "Expected: After clicking button, date should be null",
					FontSize = 12,
					TextColor = Colors.Gray
				}
			}
		};
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		// Set datePicker.Date to null
		_datePicker.Date = null;
			
		// Update the label to show the current date
		_dateLabel.Text = $"Current Date: {_datePicker.Date:d}";
	}
}