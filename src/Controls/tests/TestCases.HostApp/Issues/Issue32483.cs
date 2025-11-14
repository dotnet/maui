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
		entry.Behaviors.Add(new Issue32483Behavior());
		entry.AutomationId = "TestEntry";
		entry.HeightRequest = 100;
		stackLayout.Children.Add(label);
		stackLayout.Children.Add(entry);
		Content = stackLayout;
	}
}

 public class Issue32483Behavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry bindable)
    {
        base.OnAttachedTo(bindable);
        bindable.TextChanged += OnTextChanged;
    }
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        (((sender as Entry).Parent as StackLayout).Children[0] as Label).Text = "CursorPosition : " + (sender as Entry).CursorPosition;
	}
}
	
