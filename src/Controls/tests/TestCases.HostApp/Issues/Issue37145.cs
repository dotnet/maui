namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37145, "RadioButton border properties not cleared when reset to default values on Android", PlatformAffected.Android)]
public class Issue37145 : ContentPage
{
	public Issue37145()
	{
		var radioButton = new RadioButton
		{
			Content = "Test RadioButton",
			AutomationId = "TestRadioButton",
			BorderColor = Colors.Green,
			BorderWidth = 3,
			CornerRadius = 10,
			Padding = new Thickness(20),
			FontSize = 18
		};

		var clearBorderButton = new Button
		{
			Text = "Clear Border",
			AutomationId = "ClearBorderButton",
			Command = new Command(() =>
			{
				radioButton.BorderColor = null;
				radioButton.BorderWidth = 0;
				radioButton.CornerRadius = 0;
			})
		};

		var setBorderButton = new Button
		{
			Text = "Set Border",
			AutomationId = "SetBorderButton",
			Command = new Command(() =>
			{
				radioButton.BorderColor = Colors.Blue;
				radioButton.BorderWidth = 3;
				radioButton.CornerRadius = 10;
			})
		};

		Content = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = 30,
			Children =
			{
				radioButton,
				clearBorderButton,
				setBorderButton
			}
		};
	}
}
