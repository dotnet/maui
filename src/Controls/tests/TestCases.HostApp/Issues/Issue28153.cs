namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 28153, "The border color of the RadioButton is visible in Windows only", PlatformAffected.UWP)]
class Issue28153 : ContentPage
{
	public Issue28153()
	{
		var stackLayout = new StackLayout();
		var radioButton = new RadioButton
		{
			AutomationId = "RadioButton",
			Content = "RadioButton",         
			BorderColor = Colors.Red          
		};
		stackLayout.Children.Add(radioButton);
		Content = stackLayout;
	}
}
