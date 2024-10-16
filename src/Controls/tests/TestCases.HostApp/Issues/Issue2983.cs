namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 2983, "ListView.Footer can cause NullReferenceException", PlatformAffected.iOS)]
	public class Issue2983 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ListView
			{
				Footer = new StackLayout
				{
					Children = { new Label { Text = "Footer", AutomationId = "footer" } }
				}
			};
		}
	}
}