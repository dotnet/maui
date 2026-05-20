namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35513, "Button TextColor does not restore to platform default when reset to null after dynamic update", PlatformAffected.All)]
public class Issue35513 : ContentPage
{
	Button _sampleButton;

	public Issue35513()
	{
		_sampleButton = new Button
		{
			Text = "Sample Button",
			AutomationId = "SampleButton"
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(24),
			Spacing = 14,
			Children =
			{
				_sampleButton,
				new Button
				{
					Text = "Set Text Color",
					AutomationId = "SetTextColorButton",
					Command = new Command(() => _sampleButton.TextColor = Colors.DarkRed)
				},
				new Button
				{
					Text = "Reset Text Color",
					AutomationId = "ResetTextColorButton",
					Command = new Command(() => _sampleButton.TextColor = null)
				}
			}
		};
	}
}
