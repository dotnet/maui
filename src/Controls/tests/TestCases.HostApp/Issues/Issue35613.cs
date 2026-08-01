using System.Diagnostics;
using System.Text;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35613, "OnNavigatingFrom with a NavigationPage always has an incorrect DestinationPage parameter", PlatformAffected.All)]
public class Issue35613 : NavigationPage
{
    static readonly List<string> s_logEntries = new();
    static event Action LogChanged;

    public Issue35613()
    {
        Navigation.PushAsync(new Issue35613FirstPage());
    }

    public static void AppendLog(string message)
    {
        s_logEntries.Add(message);
        Debug.WriteLine($"ISSUE35613: {message}");
        Console.WriteLine($"ISSUE35613: {message}");
        RaiseLogChanged();
    }

    public static void ClearLog()
    {
        s_logEntries.Clear();
        RaiseLogChanged();
    }

    public static void SubscribeToLogChanged(Action callback)
    {
        LogChanged += callback;
    }

    public static void UnsubscribeFromLogChanged(Action callback)
    {
        LogChanged -= callback;
    }

    public static string GetLogText()
    {
        var builder = new StringBuilder();
        foreach (var entry in s_logEntries)
        {
            builder.AppendLine(entry);
        }
        return builder.ToString();
    }

    static void RaiseLogChanged()
    {
        LogChanged?.Invoke();
    }
}

public class Issue35613FirstPage : TestContentPage
{
    Editor _logEditor;

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

        _logEditor = new Editor
        {
            AutomationId = "Issue35613_LogEditor",
            IsReadOnly = true,
            IsSpellCheckEnabled = false,
            IsTextPredictionEnabled = false,
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 220,
            FontFamily = "Courier New",
            FontSize = 12
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Children =
            {
                navigateButton,
                new Label { Text = "Event Log:", FontAttributes = FontAttributes.Bold },
                _logEditor,
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Issue35613.SubscribeToLogChanged(RefreshLogEditor);
        RefreshLogEditor();
    }

    protected override void OnDisappearing()
    {
        Issue35613.UnsubscribeFromLogChanged(RefreshLogEditor);
        base.OnDisappearing();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatedTo FirstPage [{args.NavigationType}], PreviousPage={previousPage}");
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatingFrom FirstPage [{args.NavigationType}], DestinationPage={destinationPage}");
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatedFrom FirstPage [{args.NavigationType}], DestinationPage={destinationPage}");
    }

    void OnNavigateButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Issue35613SecondPage());
    }

    void RefreshLogEditor()
    {
        if (_logEditor is null)
            return;
        _logEditor.Text = Issue35613.GetLogText();
    }
}

public class Issue35613SecondPage : TestContentPage
{
    Editor _logEditor;

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

        var navigateToThirdButton = new Button
        {
            Text = "Navigate to Third Page",
            AutomationId = "Issue35613_NavigateToThirdButton"
        };
        navigateToThirdButton.Clicked += OnNavigateToThirdButtonClicked;

        _logEditor = new Editor
        {
            AutomationId = "Issue35613_Second_LogEditor",
            IsReadOnly = true,
            IsSpellCheckEnabled = false,
            IsTextPredictionEnabled = false,
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 220,
            FontFamily = "Courier New",
            FontSize = 12
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Children =
            {
                navigateBackButton,
                navigateToThirdButton,
                new Label { Text = "Event Log:", FontAttributes = FontAttributes.Bold },
                _logEditor,
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Issue35613.SubscribeToLogChanged(RefreshLogEditor);
        RefreshLogEditor();
    }

    protected override void OnDisappearing()
    {
        Issue35613.UnsubscribeFromLogChanged(RefreshLogEditor);
        base.OnDisappearing();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatedTo SecondPage [{args.NavigationType}], PreviousPage={previousPage}");
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatingFrom SecondPage [{args.NavigationType}], DestinationPage={destinationPage}");
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatedFrom SecondPage [{args.NavigationType}], DestinationPage={destinationPage}");
    }

    void OnNavigateBackButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    void OnNavigateToThirdButtonClicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new Issue35613ThirdPage());
    }

    void RefreshLogEditor()
    {
        if (_logEditor is null)
            return;
        _logEditor.Text = Issue35613.GetLogText();
    }
}

public class Issue35613ThirdPage : TestContentPage
{
    Editor _logEditor;

    public Issue35613ThirdPage()
    {
        Title = "Issue35613 Third";
    }

    protected override void Init()
    {
        var popToRootButton = new Button
        {
            Text = "Pop To Root",
            AutomationId = "Issue35613_PopToRootButton"
        };
        popToRootButton.Clicked += OnPopToRootButtonClicked;

        _logEditor = new Editor
        {
            AutomationId = "Issue35613_Third_LogEditor",
            IsReadOnly = true,
            IsSpellCheckEnabled = false,
            IsTextPredictionEnabled = false,
            AutoSize = EditorAutoSizeOption.TextChanges,
            MinimumHeightRequest = 220,
            FontFamily = "Courier New",
            FontSize = 12
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Children =
            {
                popToRootButton,
                new Label { Text = "Event Log:", FontAttributes = FontAttributes.Bold },
                _logEditor,
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Issue35613.SubscribeToLogChanged(RefreshLogEditor);
        RefreshLogEditor();
    }

    protected override void OnDisappearing()
    {
        Issue35613.UnsubscribeFromLogChanged(RefreshLogEditor);
        base.OnDisappearing();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        var previousPage = args.PreviousPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatedTo ThirdPage [{args.NavigationType}], PreviousPage={previousPage}");
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatingFrom ThirdPage [{args.NavigationType}], DestinationPage={destinationPage}");
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        var destinationPage = args.DestinationPage?.GetType().Name ?? "Null";
        Issue35613.AppendLog($"OnNavigatedFrom ThirdPage [{args.NavigationType}], DestinationPage={destinationPage}");
    }

    void OnPopToRootButtonClicked(object sender, EventArgs e)
    {
        Navigation.PopToRootAsync();
    }

    void RefreshLogEditor()
    {
        if (_logEditor is null)
            return;
        _logEditor.Text = Issue35613.GetLogText();
    }
}
