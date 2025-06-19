namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "29092Shell", "Shell - Auto Resize chrome icons on iOS to make it more consistent with other platforms - hamburger icon", PlatformAffected.iOS)]
public partial class Issue29092Shell : Shell
{
	public Issue29092Shell()
	{
		FlyoutIcon = "groceries.png";
		Items.Add(new ContentPage()
		{
			IconImageSource = "groceries.png",
			Content = new Label() { Text = "Hello, World!", AutomationId = "HelloWorldLabel" },
		});
		Items.Add(new ContentPage());
	}
}