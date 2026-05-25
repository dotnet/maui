using Maui.Controls.Sample;

namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 30366, "SearchBar CharacterSpacing property is not working as expected", PlatformAffected.iOS | PlatformAffected.UWP)]
public class Issue30366 : ContentPage
{
    public Issue30366()
    {
        var searchBar = new SearchBar
        {
            Placeholder = "Search",
            CharacterSpacing = 5,
            AutomationId = "Issue30366_SearchBar",
        };
		searchBar.SetValue(UITestSearchBar.IsCursorVisibleProperty, false);

        var stackLayout = new VerticalStackLayout
        {
            Children =
            {
                searchBar
            }
        };

        Content = stackLayout;
    }
}