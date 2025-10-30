namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32266, "TimePicker on Windows Defaults to Midnight When Time Value Is Null", PlatformAffected.UWP)]
public class Issue32266 : ContentPage
{
	TimePicker timePicker;
	Button clearTimeButton;
	public Issue32266()
	{
		timePicker = new TimePicker
		{
			Time = new TimeSpan(6, 0, 0),
			Format = "hh:mm" 
		};

		clearTimeButton = new Button
		{
			AutomationId = "ClearTimeButton",
			Text = "Clear Time"
		};
		clearTimeButton.Clicked += ClearTime_Clicked;

		VerticalStackLayout stackLayout = new VerticalStackLayout
		{
			Padding = 25,
			Children = { timePicker, clearTimeButton }
		};

		Content = stackLayout;
	}
	
	void ClearTime_Clicked(object sender, EventArgs e)
	{
		timePicker.Time = null;
	}
}