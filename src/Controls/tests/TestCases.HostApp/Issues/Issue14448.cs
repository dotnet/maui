namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14448, "maui title bar disappears and does not re-appear on iOS when using shell.searchhandler", PlatformAffected.iOS)]
public class Issue14448 : Shell
{
    public Issue14448()
    {
        // Create a simple ShellContent with a content page
        var shellContent = new ShellContent
        {
            Title = "Home",
            Content = new Issue14448Page()
        };
        FlyoutBehavior = FlyoutBehavior.Flyout;

        // Add to root of the Shell
        Items.Add(shellContent);
    }
}

public class Issue14448Page : ContentPage
{
    public Issue14448Page()
    {
        Title = "Home";

        // Create search handler
        var searchHandler = new SearchHandler
        {
            Placeholder = "SearchHandler",
            ShowsResults = false,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            AutomationId = "SearchHandler"
        };

        // Set search handler on page
        Shell.SetSearchHandler(this, searchHandler);

        // Create content
        Content = new VerticalStackLayout
        {
            Children =
            {
                new Label
                {
                    Text = "Welcome to .NET MAUI!",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                }
            }
        };
    }
}