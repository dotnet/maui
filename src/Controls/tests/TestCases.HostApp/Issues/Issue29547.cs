namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29547, "SearchBar with IsReadOnly=True still allows text deletion While pressing delete icon", PlatformAffected.Android)]
public class Issue29547 : ContentPage
{
	public Issue29547()
	{
		var searchBar = new SearchBar
		{
			Text = "Search",
			IsReadOnly = true,
			AutomationId = "searchbar"
		};

		var grid = new Grid();
		grid.Children.Add(searchBar);
		Content = grid;
	}
}
