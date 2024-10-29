namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "TabbedPage nav tests", PlatformAffected.All)]
public class TabbedPageTests : TestTabbedPage
{
	protected override void Init()
	{
		var popButton1 = new Button() { Text = "Pop", BackgroundColor = Colors.Blue };
		popButton1.Clicked += (s, a) => Navigation.PopModalAsync();

		var popButton2 = new Button() { Text = "Pop 2", BackgroundColor = Colors.Blue };
		popButton2.Clicked += (s, a) => Navigation.PopModalAsync();

		Children.Add(new ContentPage() { Title = "Page 1", Content = popButton1 });
		Children.Add(new ContentPage() { Title = "Page 2", Content = popButton2 });
	}
}
