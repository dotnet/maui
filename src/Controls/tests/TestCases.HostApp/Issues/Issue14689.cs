namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14689, "TabbedPage Back button not updated", PlatformAffected.UWP, NavigationBehavior.SetApplicationRoot)]
public partial class Issue14689 : TabbedPage
{
	public Issue14689()
	{
		Children.Add(new ContentPage() { Title = "NoNavigationPage" });
		Children.Add(new NavigationPage(new _14689NavigationPage()) { Title = "HasNavigationPage", AutomationId = "tab2" });
		Children.Add(new NavigationPage(new _14689NavigationPage()) { Title = "HasNavigationPage1", AutomationId = "tab3" });
	}
}

public partial class _14689NavigationPage : ContentPage
{
	public _14689NavigationPage()
	{
		var button = new Button() { Text = "Push a page", VerticalOptions = LayoutOptions.Start, AutomationId = "button" };
		button.Clicked += (s, e) => { this.Navigation.PushAsync(new _14689NavigationPage()); };
		Content = button;
	}
}

