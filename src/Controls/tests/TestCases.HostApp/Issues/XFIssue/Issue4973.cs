namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4973, "TabbedPage nav tests", PlatformAffected.Android)]
public class Issue4973 : TestTabbedPage
{
	protected override void Init()
	{
		Children.Add(new TabbedPage
		{
			Title = "Tab1",
			Children =
			{
				new ContentPage
				{
					Title = "InnerTab1"
				},
				new ContentPage
				{
					Title = "InnerTab2"
				}
			}
		});

		Children.Add(new ContentPage
		{
			Title = "Tab2"
		});

		Children.Add(new ContentPage
		{
			Title = "Tab3"
		});

		Children.Add(new ContentPage
		{
			Title = "Tab4"
		});

		Children.Add(new ContentPage
		{
			Title = "Tab5",
			Content = new Label
			{
				Text = "Test"
			}
		});
	}
}