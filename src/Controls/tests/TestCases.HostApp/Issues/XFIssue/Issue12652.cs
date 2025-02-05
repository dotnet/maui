namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 12652, "[Bug] NullReferenceException in the Shell on UWP when navigating back to Shell Section with multiple content items",
	PlatformAffected.UWP)]
public class Issue12652 : TestShell
{
	protected override void Init()
	{
		AddBottomTab("Main 1")
			.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = @"Click on the tabs in the following order
									Top 3,
									Main 2,
									Main 1,
									Top 3,
									Main 2.
									If nothing crashes test has passed.",
						AutomationId = "TopTabPage2"
					}
				}
			};

		AddBottomTab("Main 2")
			.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Hello From Page 2",
						AutomationId = "TopTabPage2"
					}
				}
			};


		AddTopTab("Top 2");

		AddTopTab("Top 3")
			.Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Hello From Page 3",
						AutomationId = "TopTabPage3"
					}
				}
			};
	}
}
