namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19256, "DatePicker control minimum date issue", PlatformAffected.Android)]
public class Issue19256 : ContentPage
{
	readonly DatePicker _leftDatePicker;
	readonly DatePicker _rightDatePicker;

	static readonly DateTime BaseDate = new DateTime(2025, 6, 15);
	static readonly DateTime FutureDate = new DateTime(2025, 6, 25);  // BaseDate + 10 days
	static readonly DateTime EarlierDate = new DateTime(2025, 6, 20); // BaseDate + 5 days

	public Issue19256()
	{
		_leftDatePicker = new DatePicker
		{
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "LeftDatePicker",
			Date = BaseDate
		};
		_leftDatePicker.DateSelected += OnLeftDateSelected;

		_rightDatePicker = new DatePicker
		{
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "RightDatePicker",
			Date = BaseDate,
			MinimumDate = BaseDate
		};

		var setFutureDateButton = new Button
		{
			AutomationId = "SetFutureDateButton",
			Text = "Set Future Date"
		};
		setFutureDateButton.Clicked += OnSetFutureDateClicked;

		var setEarlierDateButton = new Button
		{
			AutomationId = "SetEarlierDateButton",
			Text = "Set Earlier Date"
		};
		setEarlierDateButton.Clicked += OnSetEarlierDateClicked;

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 20,
			Children =
				{
					_leftDatePicker,
					_rightDatePicker,
					setFutureDateButton,
					setEarlierDateButton
				}
		};
	}


	void OnLeftDateSelected(object sender, DateChangedEventArgs e)
	{
		_rightDatePicker.MinimumDate = e.NewDate;

		if (_rightDatePicker.Date < e.NewDate)
		{
			_rightDatePicker.Date = e.NewDate;
		}
	}

	void OnSetFutureDateClicked(object sender, EventArgs e)
	{
		_leftDatePicker.Date = FutureDate;
	}

	void OnSetEarlierDateClicked(object sender, EventArgs e)
	{
		_leftDatePicker.Date = EarlierDate;
	}
}
