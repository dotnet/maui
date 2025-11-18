namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32483, "CursorPosition not calculated correctly on behaviors events for iOS devices", PlatformAffected.iOS)]

public class Issue32483 : ContentPage
{
	public Issue32483()
	{
		var stackLayout = new StackLayout();
		var label = new Label
		{
			Text = "CursorPosition :",
		};
		label.AutomationId = "CursorLabel";
		label.HeightRequest = 100;
		var entry = new Entry();
		entry.Keyboard = Keyboard.Numeric;
		entry.TextChanged += (s, e) =>
		{
			label.Text = "CursorPosition : " + entry.CursorPosition;
		};
		entry.AutomationId = "TestEntry";
		entry.HeightRequest = 100;
		stackLayout.Children.Add(label);
		stackLayout.Children.Add(entry);
		Content = stackLayout;
	}
}
	
