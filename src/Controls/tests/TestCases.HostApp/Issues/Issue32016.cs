namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32016, "iOS 26 MaxLength not enforced on Entry", PlatformAffected.iOS)]
	public class Issue32016 : ContentPage
	{
		public Issue32016()
		{
			Content = new Entry()
			{
				AutomationId = "TestEntry",
				MaxLength = 10,
			};
		}
	}
}