namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26908, "Time Picker Focus Unfocus is not working", PlatformAffected.Android)]
	public class Issue26908 : TestContentPage
	{
		protected override void Init()
		{
			var timePicker = new TimePicker();
			var statusLabel = new Label
			{
				AutomationId = "StatusLabel",
			};

			timePicker.Focused += (s, e) =>
			{
				statusLabel.Text += "Focused";
				timePicker.Unfocus();
			};

			timePicker.Unfocused += (s, e) => statusLabel.Text += "Unfocused";

			Content = new StackLayout
			{
				Children =
				{
					timePicker,
					statusLabel,
					new Button()
					{
						Text = "Focus",
						AutomationId = "FocusButton",
						Command = new Command(() => timePicker.Focus())
					}
				}
			};
		}
	}
}