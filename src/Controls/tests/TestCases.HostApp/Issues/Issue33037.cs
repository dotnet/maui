namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33037, "iOS Large Title display disappears when scrolling in Shell", PlatformAffected.iOS)]
public class Issue33037 : TestShell
{
	protected override void Init()
	{
		var page = new ContentPage
		{
			Title = "Large Title Test"
		};

		// Explicitly set LargeTitleDisplay to Always — this should show a large title
		Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetLargeTitleDisplay(
			page,
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.LargeTitleDisplayMode.Always);

		var scrollView = new ScrollView
		{
			AutomationId = "TestScrollView",
			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Large Title Test Page",
						AutomationId = "PageTitle",
						FontSize = 18,
						Margin = new Thickness(20, 10)
					}
				}
			}
		};

		// Add enough items to make the page scrollable
		var layout = (VerticalStackLayout)scrollView.Content;
		for (int i = 0; i < 30; i++)
		{
			layout.Children.Add(new Label
			{
				Text = $"Item {i}",
				AutomationId = $"Item{i}",
				Margin = new Thickness(20, 5)
			});
		}

		page.Content = scrollView;
		AddContentPage(page);
	}
}
