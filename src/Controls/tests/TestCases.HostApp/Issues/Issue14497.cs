namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14497, "Dynamically setting SearchHandler Query property does not update text in the search box", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue14497 : Shell
{
	public Issue14497()
	{
		ShellContent shellContent = new ShellContent
		{
			Title = "Home",
			ContentTemplate = new DataTemplate(typeof(Issue14497Page)),
			Route = "MainPage"
		};

		Items.Add(shellContent);
	}
}

public class Issue14497Page : ContentPage
{
	Issue14497CustomSearchHandler _searchHandler;
	public Issue14497Page()
	{
		_searchHandler = new Issue14497CustomSearchHandler();

		Button button = new Button
		{
			Text = "Change Search Text",
			AutomationId = "ChangeSearchText"
		};

		button.Clicked += (s, e) => _searchHandler.SetQuery("Hello World");

		VerticalStackLayout stackLayout = new VerticalStackLayout
		{
			Children = { button }
		};

		Content = stackLayout;
		Shell.SetSearchHandler(this, _searchHandler);
	}
}

public class Issue14497CustomSearchHandler : SearchHandler
{
	public Issue14497CustomSearchHandler()
	{
		Placeholder = "Search...";
		ShowsResults = false;
	}

	public void SetQuery(string searchText)
	{
		Query = searchText;
	}

}