namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 198, "TabbedPage shouldn't proxy content of NavigationPage", PlatformAffected.iOS)]
public class Issue198 : TestTabbedPage
{
	protected override void Init()
	{
		Title = "Tabbed Navigation Page";

		var leavePageBtn = new Button
		{
			Text = "Leave"
		};

		// Should work as expected, however, causes NRE
		leavePageBtn.Clicked += (s, e) => Navigation.PopModalAsync();

		var navigationPageOne = new NavigationPage(new ContentPage
		{
			IconImageSource = "calculator.png",
			Content = leavePageBtn
		})
		{
			Title = "Page One",
		};
		var navigationPageTwo = new NavigationPage(new ContentPage
		{
			IconImageSource = "calculator.png",
		})
		{
			Title = "Page Two",
		};
		var navigationPageThree = new NavigationPage(new ContentPage
		{
			Title = "No Crash",
		})
		{
			Title = "Page Three",
			IconImageSource = "calculator.png"
		};
		var navigationPageFour = new NavigationPage(new ContentPage())
		{
			Title = "Page Four",
			IconImageSource = "calculator.png"
		};

		Children.Add(navigationPageOne);
		Children.Add(navigationPageTwo);
		Children.Add(navigationPageThree);
		Children.Add(navigationPageFour);
	}
}