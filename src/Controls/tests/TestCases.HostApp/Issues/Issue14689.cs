namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14689, "TabbedPage Back button not updated", PlatformAffected.UWP, NavigationBehavior.SetApplicationRoot)]
public class Issue14689 : TabbedPage
{
	public Issue14689()
	{
		Children.Add(new ContentPage() { Title = "NoNavigationPage" });
		Children.Add(new NavigationPage(new Issue14689Page()) { Title = "HasNavigationPage" });
		Children.Add(new NavigationPage(new Issue14689Page()) { Title = "HasNavigationPage1" });
	}
}

public class Issue14689Page : ContentPage
{
	public Issue14689Page()
	{
		var button = new Button() { Text = "Push a page", VerticalOptions = LayoutOptions.Start, AutomationId = "button" };
		button.Clicked += (s, e) => { this.Navigation.PushAsync(new Issue14689Page()); };
		Content = button;
	}
}

