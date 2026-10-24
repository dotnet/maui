namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32463, "Direct test launch", PlatformAffected.All)]
public class Issue32463 : ContentPage
{
	public Issue32463()
	{
		var count = 0;
		var label = new Label
		{
			Text = "Launched directly to the test",
			AutomationId = "DirectLaunchLabel"
		};
		var counter = new Label { Text = "0", AutomationId = "LaunchCounter" };
		var windows = new Label { Text = Application.Current.Windows.Count.ToString(), AutomationId = "StartupWindowCount" };
		var button = new Button { Text = "Increment", AutomationId = "IncrementCounter" };
		button.Clicked += (_, _) => counter.Text = (++count).ToString();
		Content = new VerticalStackLayout { Children = { label, counter, windows, button } };
	}
}
