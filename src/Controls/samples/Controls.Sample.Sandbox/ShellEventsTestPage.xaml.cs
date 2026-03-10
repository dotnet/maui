using System.Text;

namespace Maui.Controls.Sample;

public partial class ShellEventsTestPage : ContentPage
{
    readonly StringBuilder _eventLog = new();
    bool _isLogging;

    public ShellEventsTestPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_isLogging)
            SubscribeToEvents();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        UnsubscribeFromEvents();
    }

    // --- Event Logging ---

    void OnStartEventLoggingClicked(object? sender, EventArgs e)
    {
        _isLogging = true;
        SubscribeToEvents();
        LogEvent("Event logging started");
        StatusLabel.Text = "Status: Logging enabled";
    }

    void OnStopEventLoggingClicked(object? sender, EventArgs e)
    {
        _isLogging = false;
        UnsubscribeFromEvents();
        LogEvent("Event logging stopped");
        StatusLabel.Text = "Status: Logging disabled";
    }

    void OnClearLogClicked(object? sender, EventArgs e)
    {
        _eventLog.Clear();
        EventLogLabel.Text = "(no events logged yet)";
        StatusLabel.Text = "Status: Log cleared";
    }

    void SubscribeToEvents()
    {
        if (Shell.Current is null)
            return;
        // Avoid duplicate subscriptions
        UnsubscribeFromEvents();
        Shell.Current.Navigating += OnShellNavigating;
        Shell.Current.Navigated += OnShellNavigated;
    }

    void UnsubscribeFromEvents()
    {
        if (Shell.Current is null)
            return;
        Shell.Current.Navigating -= OnShellNavigating;
        Shell.Current.Navigated -= OnShellNavigated;
    }

    void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        LogEvent($"NAVIGATING: {e.Source} → Current={e.Current?.Location} Target={e.Target?.Location}");
    }

    void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        LogEvent($"NAVIGATED: {e.Source} → Current={e.Current?.Location} Previous={e.Previous?.Location}");
    }

    void LogEvent(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _eventLog.AppendLine($"[{timestamp}] {message}");
        EventLogLabel.Text = _eventLog.ToString();
    }

    // --- Trigger Navigation ---

    async void OnPushPageClicked(object? sender, EventArgs e)
    {
        var page = new ContentPage
        {
            Title = "Pushed Page",
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(30),
                VerticalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = "Pushed page — check event log after going back",
                        FontSize = 16,
                        HorizontalOptions = LayoutOptions.Center,
                        HorizontalTextAlignment = TextAlignment.Center
                    },
                    new Button
                    {
                        Text = "Go Back",
                        HorizontalOptions = LayoutOptions.Center,
                        Command = new Command(async () => await Shell.Current.GoToAsync(".."))
                    }
                }
            }
        };

        await Navigation.PushAsync(page);
    }

    async void OnGoToHomeClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//Home/MainPage");
    }

    async void OnGoToMultiTabClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MultiTab/Content1");
    }

    // --- MenuItem Tests ---
    // Shell MenuItems are added via XAML. The SandboxShell.xaml contains a <MenuItem> element.
    // These buttons test toggling flyout to observe it and logging when it is clicked.

    void OnAddMenuItemClicked(object? sender, EventArgs e)
    {
        // Open flyout so user can see the MenuItem
        if (Shell.Current is not null)
        {
            Shell.Current.FlyoutIsPresented = true;
            StatusLabel.Text = "Status: Flyout opened — look for 'Test Action' menu item";
            LogEvent("Flyout opened to show MenuItem");
        }
    }

    void OnRemoveMenuItemClicked(object? sender, EventArgs e)
    {
        // Close flyout
        if (Shell.Current is not null)
        {
            Shell.Current.FlyoutIsPresented = false;
            StatusLabel.Text = "Status: Flyout closed";
            LogEvent("Flyout closed");
        }
    }
}
