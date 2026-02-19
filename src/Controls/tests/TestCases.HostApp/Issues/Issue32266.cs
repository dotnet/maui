namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32266, "TimePicker on Windows Defaults to Midnight When Time Value Is Null", PlatformAffected.UWP)]
public class Issue32266 : ContentPage
{
	TimePicker timePicker;
	Button clearTimeButton;
	Button setTimeButton;

	public Issue32266()
	{
		timePicker = new TimePicker
		{
			AutomationId = "Issue32266TimePicker",
			Time = null,
			Format = "hh:mm"
		};

		setTimeButton = new Button
		{
			AutomationId = "SetTimeButton",
			Text = "Set Time to 6:00 AM"
		};
		setTimeButton.Clicked += SetTime_Clicked;

		clearTimeButton = new Button
		{
			AutomationId = "ClearTimeButton",
			Text = "Clear Time (Set to Null)"
		};
		clearTimeButton.Clicked += ClearTime_Clicked;

		VerticalStackLayout stackLayout = new VerticalStackLayout
		{
			Padding = 25,
			Spacing = 10,
			Children = { timePicker, setTimeButton, clearTimeButton }
		};

		Content = stackLayout;
	}

	void SetTime_Clicked(object sender, EventArgs e)
	{
		timePicker.Time = new TimeSpan(6, 0, 0);
	}

	void ClearTime_Clicked(object sender, EventArgs e)
	{
		timePicker.Time = null;
	}
}