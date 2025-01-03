namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26908, "Time Picker Focus Unfocus is not working", PlatformAffected.Android)]
	public class Issue26908 : TestContentPage
	{
		protected override void Init()
		{
			var timePicker = new TimePicker();
			var focusedLabel = new Label
			{
				AutomationId = "FocusedLabel",
				Text = "The time picker was focused",
				IsVisible = false,
			};

			var unfocusedLabel = new Label
			{
				AutomationId = "UnfocusedLabel",
				Text = "The time picker was unfocused",
				IsVisible = false,
			};

			timePicker.Focused += (s, e) =>
			{
				focusedLabel.IsVisible = true;
				timePicker.Unfocus();
			};

			timePicker.Unfocused += (s, e) => unfocusedLabel.IsVisible = true;

			Content = new StackLayout
			{
				Children =
				{
					timePicker,
					focusedLabel,
					unfocusedLabel,
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