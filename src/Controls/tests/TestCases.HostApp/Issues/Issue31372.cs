namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31372, "IsPresented=true Not Working on Initial Value in FlyoutPage", PlatformAffected.UWP | PlatformAffected.macOS)]
public class Issue31372 : FlyoutPage
{
	public Issue31372()
	{
		var flyoutPage = new ContentPage
		{
			Title = "Flyout",
			Content = new Label
			{
				Text = "This is Flyout",
				AutomationId = "FlyoutLabel"
			}
		};

		var detailPage = new ContentPage
		{
			Content = new Label
			{
				Text = "This is Detail Page which displays additional information.",
				AutomationId = "DetailLabel"
			}
		};

		Flyout = flyoutPage;
		Detail = detailPage;

		// Setting IsPresented to true should open the flyout on app launch
		IsPresented = true;
	}
}