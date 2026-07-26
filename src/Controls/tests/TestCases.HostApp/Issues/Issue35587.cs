namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35587, "RadioButton BorderColor and BorderWidth not applied when dynamically updated at runtime", PlatformAffected.Android | PlatformAffected.UWP)]
public class Issue35587 : ContentPage
{
	public Issue35587()
	{
		var radioButton = new RadioButton
		{
			AutomationId = "TestRadioButton",
			Content = "RadioButton",
		};

		var applyBorderButton = new Button
		{
			AutomationId = "ApplyBorderButton",
			Text = "Apply Border"
		};

		applyBorderButton.Clicked += (s, e) =>
		{
			radioButton.BorderColor = Colors.Green;
			radioButton.BorderWidth = 3;
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 20,
			Children =
			{
				radioButton,
				applyBorderButton
			}
		};
	}
}
