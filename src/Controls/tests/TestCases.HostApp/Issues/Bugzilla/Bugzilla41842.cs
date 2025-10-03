namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 41842, "Set FlyoutPage.Detail = New Page() twice will crash the application when set FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split", PlatformAffected.WinRT)]
	public class Bugzilla41842 : TestFlyoutPage
	{
		protected override void Init()
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;

			Flyout = new ContentPage() { Title = "Flyout" };

			Detail = new NavigationPage(new ContentPage());
			Detail = new NavigationPage(new ContentPage { Content = new Label { AutomationId = "SuccessLabel", Text = "Success" } });
		}
	}
}
