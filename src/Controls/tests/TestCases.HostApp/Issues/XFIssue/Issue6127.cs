namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6127, "[Bug] ToolbarItem Order property ignored", PlatformAffected.Android)]
public class Issue6127 : TestShell
{
	const string PrimaryToolbarIcon = "PrimaryToolbarIcon";
	const string SecondaryToolbarIcon = "SecondaryToolbarIcon";

	protected override void Init()
	{
		AddTopTab(createContentPage("title 1"), "page 1");

		ContentPage createContentPage(string titleView)
		{
			var page = new ContentPage
			{
				Content = new StackLayout
				{
					new Label
					{
						Text = "If there is one toolbar item visible and one under the overflow menu, this test passed"
					}
				}
			};

			page.ToolbarItems.Add(new ToolbarItem { IconImageSource = "coffee.png", Order = ToolbarItemOrder.Primary, Priority = 0, AutomationId = PrimaryToolbarIcon });
			page.ToolbarItems.Add(new ToolbarItem { Text = "Coffee", IconImageSource = "coffee.png", Order = ToolbarItemOrder.Secondary, Priority = 0, AutomationId = SecondaryToolbarIcon });

			return page;
		}
	}
}