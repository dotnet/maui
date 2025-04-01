namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 10134, "Shell Top Tabbar focus issue", PlatformAffected.iOS)]
public class Issue10134 : TestShell
{
	protected override void Init()
	{
		ContentPage page1 = AddTopTab("Tab 1");
		page1.Title = "Top Bar Page 1";

		for (int i = 2; i < 20; i++)
		{
			AddTopTab($"Tab {i}");
		}

		page1.Content =
			new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Scroll and click on any of the currently non visible tabs. After clicking, if the Top Tabs don't scroll back to the beginninig the test has passed"
					}
				}
			};
	}
}
