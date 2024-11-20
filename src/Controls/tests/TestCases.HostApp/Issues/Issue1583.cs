namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1583, "NavigationPage.TitleIcon broken", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1583 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Test";
			BackgroundColor = Colors.Pink;
			Content = new Label { Text = "Hello", AutomationId = "lblHello" };
			NavigationPage.SetTitleIconImageSource(this, "bank.png");
		}
	}
}

