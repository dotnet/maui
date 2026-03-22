namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28634, "[Android] SearchHandler Placeholder Text", PlatformAffected.Android)]
public class Issue28634 : TestShell
{

	protected override void Init()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;

		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue28634ContentPage() { Title = "Home" }
		};

		Items.Add(shellContent);

	}
	class Issue28634ContentPage : ContentPage
	{
		public Issue28634ContentPage()
		{
			this.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			var searchHandler = new SearchHandler
			{
				Placeholder = "Type a fruit name to search",
				PlaceholderColor = Colors.Red,
			};

			var button = new Button
			{
				Text = "Change SearchHandler Placeholder",
				AutomationId = "button",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
			button.Clicked += (s, e) =>
			{
				searchHandler.Placeholder = "Type a vegetable name to search";
			};

			Shell.SetSearchHandler(this, searchHandler);

			Content = button;
		}
	}
}