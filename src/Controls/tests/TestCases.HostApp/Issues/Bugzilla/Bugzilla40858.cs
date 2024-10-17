namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 40858, "Long clicking a text entry in a ListView header/footer cause a crash", PlatformAffected.Android)]
public class Bugzilla40858 : TestContentPage
{
	protected override void Init()
	{
		Content = new StackLayout
		{
			Children =
			{
				new ListView
				{
					Header = new Editor
					{
						AutomationId = "Header",
						HeightRequest = 50,
						Text = "ListView Header -- Editor"
					},
					Footer = new Entry
					{
						AutomationId = "Footer",
						HeightRequest = 50,
						Text = "ListView Footer -- Entry"
					}
				}
			}
		};
	}
}
