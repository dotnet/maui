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

public partial class Issue14497Page : ContentPage
{
    CustomSearchHandler _searchHandler;
    public Issue14497Page()
    {
        _searchHandler = new CustomSearchHandler()
        {
            AutomationId = "searchHandler"
        };

        Button button = new Button
        {
            Text = "Change Search Text",
            AutomationId = "Button_ChangeSearchText"
        };

        button.Clicked += Button_Clicked;

        VerticalStackLayout stackLayout = new VerticalStackLayout
        {
            Children = { button }
        };

        Content = stackLayout;
        Shell.SetSearchHandler(this, _searchHandler);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        _searchHandler.SetQuery("Hello World");
    }
}

public class CustomSearchHandler : SearchHandler
{
    public CustomSearchHandler()
    {
        Placeholder = "Search...";
        ShowsResults = false;
    }

    public void SetQuery(string searchText)
    {
        Query = searchText;
    }

}