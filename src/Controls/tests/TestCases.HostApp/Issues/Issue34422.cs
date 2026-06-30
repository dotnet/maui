namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34422, "SearchBar clear button still appears on MacCatalyst after clearing input", PlatformAffected.macOS)]
public class Issue34422 : ContentPage
{
	readonly SearchBar _searchBar;

	public Issue34422()
	{
		_searchBar = new SearchBar
		{
			Placeholder = "Search...",
			AutomationId = "TestSearchBar"
		};

		var addTextButton = new Button
		{
			Text = "Add Text",
			AutomationId = "AddTextButton"
		};

		addTextButton.Clicked += (s, e) =>
		{
			_searchBar.Text = "Search text";
		};

		var clearButton = new Button
		{
			Text = "Clear SearchBar Text",
			AutomationId = "ClearButton"
		};

		clearButton.Clicked += (s, e) =>
		{
			_searchBar.Text = string.Empty;
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			Children =
			{
				_searchBar,
				addTextButton,
				clearButton
			}
		};
	}
}
