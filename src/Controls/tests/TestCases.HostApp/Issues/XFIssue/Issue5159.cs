namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5159, "[Android] Calling Focus on all Pickers running an API 28 devices no longer opens Picker", PlatformAffected.Android)]
public class Issue5159 : TestContentPage
{
	const string DatePickerButton = "DatePickerButton";
	const string TimePickerButton = "TimePickerButton";
	const string PickerButton = "PickerButton";
	readonly string[] _pickerValues = { "Foo", "Bar", "42", "1337" };

	protected override void Init()
	{
		var stackLayout = new StackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		// DatePicker
		var datePickerButton = new Button
		{
			Text = "Show DatePicker",
			AutomationId = DatePickerButton
		};

		var datePicker = new DatePicker
		{
			IsVisible = false
		};

		datePickerButton.Clicked += (s, a) =>
		{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(() =>
			{
				if (datePicker.IsFocused)
					datePicker.Unfocus();

				datePicker.Focus();
			});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		};

		// TimePicker
		var timePickerButton = new Button
		{
			Text = "Show TimePicker",
			AutomationId = TimePickerButton
		};

		var timePicker = new TimePicker
		{
			IsVisible = false
		};

		timePickerButton.Clicked += (s, a) =>
		{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(() =>
			{
				if (timePicker.IsFocused)
					timePicker.Unfocus();

				timePicker.Focus();
			});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		};

		// Picker
		var pickerButton = new Button
		{
			Text = "Show Picker",
			AutomationId = PickerButton
		};

		var picker = new Picker
		{
			IsVisible = false,
			ItemsSource = _pickerValues
		};

		pickerButton.Clicked += (s, a) =>
		{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(() =>
			{
				if (picker.IsFocused)
					picker.Unfocus();

				picker.Focus();
			});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		};

		stackLayout.Add(datePickerButton);
		stackLayout.Add(datePicker);

		stackLayout.Add(timePickerButton);
		stackLayout.Add(timePicker);

		stackLayout.Add(pickerButton);
		stackLayout.Add(picker);

		Content = stackLayout;
	}
}
