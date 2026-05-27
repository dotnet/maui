namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35613, "OnNavigatingFrom with a NavigationPage always has an incorrect DestinationPage parameter", PlatformAffected.All)]
public class Issue35613 : NavigationPage
{
    public Issue35613()
    {
        Navigation.PushAsync(new Issue35613FirstPage());
    }
}

public class Issue35613FirstPage : TestContentPage
{
    Label _onNavigatingToLabel;
    Label _onNavigatingFromLabel;
    Label _onNavigatedFromLabel;

    public Issue35613FirstPage()
    {
        Title = "Issue35613 First";
    }

    protected override void Init()
    {
        var navigateButton = new Button
        {
            Text = "Navigate to Second Page",
            AutomationId = "Issue35613_NavigateButton"
        };
        navigateButton.Clicked += OnNavigateButtonClicked;

        _onNavigatingToLabel = new Label
        {
            Text = "-",
            AutomationId = "Issue35613_First_OnNavigatingToLabel"
        };

        _onNavigatingFromLabel = new Label
        {
            Text = "-",
            AutomationId = "Issue35613_First_OnNavigatingFromLabel"
        };

        _onNavigatedFromLabel = new Label
        {
            Text = "-",
            AutomationId = "Issue35613_First_OnNavigatedFromLabel"
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Children =
            {
                navigateButton,
                new Label { Text = "NavigatingToEventArgs", FontAttributes = FontAttributes.Bold },
                _onNavigatingToLabel,
                new Label { Text = "NavigatingFromEventArgs", FontAttributes = FontAttributes.Bold },
                _onNavigatingFromLabel,
                new Label { Text = "NavigatedFromEventArgs", FontAttributes = FontAttributes.Bold },
                _onNavigatedFromLabel,
            }
        };
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
        _onNavigatingToLabel.Text = $"PreviousPage: {previousPage}, NavigationType: {args.NavigationType}";
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        _onNavigatingFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        _onNavigatedFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
    }

    void OnNavigateButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Issue35613SecondPage());
    }
}

public class Issue35613SecondPage : TestContentPage
{
    Label _onNavigatingToLabel;
    Label _onNavigatingFromLabel;
    Label _onNavigatedFromLabel;

    public Issue35613SecondPage()
    {
        Title = "Issue35613 Second";
    }

    protected override void Init()
    {
        var navigateBackButton = new Button
        {
            Text = "Navigate Back",
            AutomationId = "Issue35613_NavigateBackButton"
        };
        navigateBackButton.Clicked += OnNavigateBackButtonClicked;

        _onNavigatingToLabel = new Label
        {
            Text = "-",
            AutomationId = "Issue35613_Second_OnNavigatingToLabel"
        };

        _onNavigatingFromLabel = new Label
        {
            Text = "-",
            AutomationId = "Issue35613_Second_OnNavigatingFromLabel"
        };

        _onNavigatedFromLabel = new Label
        {
            Text = "-",
            AutomationId = "Issue35613_Second_OnNavigatedFromLabel"
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Children =
            {
                navigateBackButton,
                new Label { Text = "NavigatingToEventArgs", FontAttributes = FontAttributes.Bold },
                _onNavigatingToLabel,
                new Label { Text = "NavigatingFromEventArgs", FontAttributes = FontAttributes.Bold },
                _onNavigatingFromLabel,
                new Label { Text = "NavigatedFromEventArgs", FontAttributes = FontAttributes.Bold },
                _onNavigatedFromLabel,
            }
        };
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
        _onNavigatingToLabel.Text = $"PreviousPage: {previousPage}, NavigationType: {args.NavigationType}";
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        _onNavigatingFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        _onNavigatedFromLabel.Text = $"DestinationPage: {destinationPage}, NavigationType: {args.NavigationType}";
    }

    void OnNavigateBackButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}
