namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Swipe back nav crash", PlatformAffected.iOS)]

public class SwipeBackNavCrash : TestNavigationPage
{
	protected override void Init()
	{
		Navigation.PushAsync(new SwipeBackNavCrashPageOne());
	}
}
public class SwipeBackNavCrashPageOne : ContentPage
{
	public SwipeBackNavCrashPageOne()
	{
		Title = "Page One";
		var button = new Button
		{
			Text = "Go to second page"
		};
		button.Clicked += (sender, e) => Navigation.PushAsync(new PageTwo());
		Content = button;
	}
}

public class PageTwo : ContentPage
{
	public PageTwo()
	{
		Title = "Second Page";
		Content = new StackLayout
		{
			new Label { Text = "Swipe lightly left and right to crash this page" },
			new BoxView { Color = new Color (0.0f) }
		};
	}
}