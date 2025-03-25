namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 5239, "[iOS] Top Padding not working on iOS when it is set alone",
		PlatformAffected.iOS, navigationBehavior: NavigationBehavior.SetApplicationRoot)]
	public class Issue5239 : TestContentPage
	{
		protected override void Init()
		{
			Padding = new Thickness(0, 20, 0, 0);
			Label label = new Label { Text = "I should be 20 pixels from the top", AutomationId = "Hello" };
			Content = label;
		}
	}
}
