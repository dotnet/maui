namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14566, "SearchBar IsEnabled property not functioning", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue14566 : TestContentPage
	{
		const string SearchBar = "SearchBar";

		protected override void Init()
		{
			var layout = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(30, 0),
				Spacing = 25
			};

			var searchBar = new SearchBar
			{
				AutomationId = SearchBar,
				Placeholder = "Search Placeholder",
				IsEnabled = false
			};

			layout.Children.Add(searchBar);

			Content = layout;
		}
	}
}