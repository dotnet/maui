namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 57717, "Setting background color on Button in Android FormsApplicationActivity causes NRE", PlatformAffected.Android)]
public class Bugzilla57717 : TestContentPage
{
	const string ButtonText = "I am a button";

	protected override void Init()
	{
		var layout = new StackLayout();

		var instructions = new Label { Text = "If you can see this, the test has passed." };

		var button = new Button { Text = ButtonText, AutomationId = ButtonText, BackgroundColor = Colors.CornflowerBlue };

		layout.Children.Add(instructions);
		layout.Children.Add(button);

		Content = layout;
	}
}