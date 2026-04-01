namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33595, "[net10] iOS 18.6 crashing on navigating to a ContentPage with Padding set and Content set to a Grid with RowDefinitions Star,Auto with ScrollView on row 0", PlatformAffected.iOS)]
public class Issue33595 : TestShell
{
    public Issue33595()
    {
        Shell.SetBackgroundColor(this, Colors.Red);
    }
    protected override void Init()
    {
        AddContentPage(new Issue33595StartPage());
    }
}

public class Issue33595StartPage : ContentPage
{
    public Issue33595StartPage()
    {
        Title = "Issue 33595";

        var navigateButton = new Button
        {
            Text = "Go to New Page",
            AutomationId = "NavigateButton",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        navigateButton.Clicked += async (s, e) =>
        {
            await Navigation.PushAsync(new Issue33595TargetPage());
        };

        Content = new VerticalStackLayout
        {
            VerticalOptions = LayoutOptions.Center,
            Children = { navigateButton }
        };
    }
}

public class Issue33595TargetPage : ContentPage
{
    public Issue33595TargetPage()
    {
        Title = "New Page";
        Padding = new Thickness(5, 5, 5, 5);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        // Row 0: ScrollView with content
        var scrollView = new ScrollView();
        var stackLayout = new VerticalStackLayout
        {
            Padding = new Thickness(16),
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 88)
        };

        stackLayout.Add(new Label
        {
            Text = "Text 2",
            FontSize = 24
        });

        stackLayout.Add(new Label
        {
            Text = "This is a long text content to simulate the original issue scenario. " +
                   "The page has Padding set on the ContentPage and the content is a Grid " +
                   "with RowDefinitions Star,Auto containing a ScrollView in row 0. " +
                   "This combination caused the app to freeze on iOS 18.6 with .NET 10."
        });

        scrollView.Content = stackLayout;
        Grid.SetRow(scrollView, 0);
        grid.Add(scrollView);

        // Row 1: Grid with Button
        var bottomGrid = new Grid
        {
            Padding = new Thickness(16)
        };

        var continueButton = new Button
        {
            Text = "Continue",
            HeightRequest = 52,
            AutomationId = "ContinueButton"
        };

        bottomGrid.Add(continueButton);
        Grid.SetRow(bottomGrid, 1);
        grid.Add(bottomGrid);

        // Label to verify navigation succeeded (placed in the ScrollView content)
        stackLayout.Add(new Label
        {
            Text = "Page loaded successfully",
            AutomationId = "SuccessLabel"
        });

        Content = grid;
    }
}
