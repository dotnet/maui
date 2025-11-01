namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea Flyout Page for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 8)]
public partial class Issue28986_FlyoutPage : FlyoutPage
{
	public Issue28986_FlyoutPage() : base()
	{
		var page = new Issue28986_ContentPage() { Title = "SafeArea Flyout Test" };
		page.ToolbarItems.Add(new ToolbarItem { Text = "Item 1" });
		Shell.SetBackgroundColor(page, Colors.Blue);
		Detail = new NavigationPage(page);
		Flyout = new ContentPage()
		{
			Title = "Flyout",
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "This is the flyout content" }
				}
			}
		};
	}
}