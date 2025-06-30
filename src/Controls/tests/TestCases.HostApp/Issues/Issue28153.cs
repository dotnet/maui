namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28153, "The border color of the RadioButton is visible in Windows only", PlatformAffected.UWP)]
class Issue28153 : ContentPage
{
	public Issue28153()
	{
		VerticalStackLayout verticalStackLayout = new VerticalStackLayout();

		RadioButton radioButtonWithoutBorder = new RadioButton
		{
			AutomationId = "RadioButtonWithoutBorder",
			Content = "RadioButton",
			BorderColor = Colors.Red
		};

		RadioButton radioButtonWithBorder = new RadioButton
		{
			AutomationId = "RadioButtonWithBorder",
			Content = "RadioButton",
			BorderColor = Colors.Red,
			BorderWidth = 3
		};

		verticalStackLayout.Children.Add(radioButtonWithoutBorder);
		verticalStackLayout.Children.Add(radioButtonWithBorder);

		Content = verticalStackLayout;
	}
}
