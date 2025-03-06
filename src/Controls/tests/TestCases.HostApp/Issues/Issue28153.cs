namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28153, "The border color of the RadioButton is visible in Windows only", PlatformAffected.UWP)]
class Issue28153 : ContentPage
{
	public Issue28153()
	{
		var verticalStackLayout = new VerticalStackLayout();
		var radioButton = new RadioButton
		{
			AutomationId = "RadioButton",
			Content = "RadioButton",         
			BorderColor = Colors.Red          
		};
		verticalStackLayout.Children.Add(radioButton);
		Content = verticalStackLayout;
	}
}
