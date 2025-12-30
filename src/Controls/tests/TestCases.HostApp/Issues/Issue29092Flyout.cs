namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "29092Flyout", "Flyout - Auto Resize chrome icons on iOS to make it more consistent with other platforms - hamburger icon", PlatformAffected.iOS)]
public partial class Issue29092Flyout : FlyoutPage
{
	public Issue29092Flyout()
	{
		Flyout = new ContentPage
		{
			IconImageSource = "groceries.png",
			Title = "Flyout"
		};

		Detail = new NavigationPage(new ContentPage
		{
			Title = "Title",
			Content = new Label() { Text = "Hello, World!", AutomationId = "HelloWorldLabel", },
		});
	}
}