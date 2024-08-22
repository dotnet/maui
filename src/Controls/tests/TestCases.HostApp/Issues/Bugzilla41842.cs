using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41842, "Set FlyoutPage.Detail = New Page() twice will crash the application when set FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split", PlatformAffected.WinRT)]
	public class Bugzilla41842 : TestFlyoutPage
	{
		protected override void Init()
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;

			Flyout = new Page() { Title = "Flyout" };

			Detail = new NavigationPage(new Page());
			Detail = new NavigationPage(new ContentPage { Content = new Label { AutomationId = "Success", Text = "Success" } });
		}
	}
}
