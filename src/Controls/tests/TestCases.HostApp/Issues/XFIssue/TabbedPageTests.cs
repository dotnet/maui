namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "TabbedPage nav basic tests", PlatformAffected.All)]
public class TabbedPageTests : ContentPage
{
	public TabbedPageTests()
	{
		var navigate = new Button() { Text = "HomePage", AutomationId = "HomePage" };

		navigate.Clicked += async (s, a) =>
		{
			var tabbedPage = new TabbedPage();
			var popButton1 = new Button() { Text = "Pop", BackgroundColor = Colors.Blue };
			var popButton2 = new Button() { Text = "Pop 2", BackgroundColor = Colors.Blue };

			popButton1.Clicked += (s, a) => Navigation.PopModalAsync();
			popButton2.Clicked += (s, a) => Navigation.PopModalAsync();

			tabbedPage.Children.Add(new ContentPage() { Title = "Page 1", Content = popButton1 });
			tabbedPage.Children.Add(new ContentPage() { Title = "Page 2", Content = popButton2 });

			await Navigation.PushModalAsync(tabbedPage);

		};

		this.Content = new StackLayout
		{
			Children = {
				navigate
			}
		};



	}

}