namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29615, "Flyout icon is displayed when flyout is disabled on iOS and MacCatalyst", PlatformAffected.iOS)]
public class Issue29615 : Shell
{
	public Issue29615()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;
		this.FlyoutIcon = "dotnet_bot.png";

		var disableButton = new Button { Text = "Disable", AutomationId = "DisabledButton" };

		disableButton.Clicked += (s, e) => this.FlyoutBehavior = FlyoutBehavior.Disabled;

		var stack = new StackLayout
		{
			Children = { disableButton }
		};

		var contentPage = new ContentPage
		{
			Title = "Home",
			Content = stack
		};

		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = contentPage
		};

		Items.Add(shellContent);
	}
}
