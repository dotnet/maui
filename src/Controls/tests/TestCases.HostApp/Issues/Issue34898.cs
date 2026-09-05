namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34898, "Shell.Items.Clear does not disconnect handlers correctly", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.macOS)]
public class Issue34898 : TestShell
{
    static Label _trackedLabel;
    static Entry _trackedEntry;
    static Button _trackedButton;
    static ContentPage _trackedPage;

    protected override void Init()
    {
        _trackedLabel = new Label { Text = "Tracked Label", AutomationId = "TrackedLabel" };
        _trackedEntry = new Entry { Placeholder = "Tracked Entry", AutomationId = "TrackedEntry" };
        _trackedButton = new Button { Text = "Tracked Button", AutomationId = "TrackedButton" };

        var clearAndNavigateButton = new Button
        {
            Text = "Clear Items and Navigate",
            AutomationId = "ClearAndNavigateButton",
            Command = new Command(async () =>
            {
                var tabBar = new TabBar { Route = "secondRoot" };
                tabBar.Items.Add(new Tab
                {
                    Items =
                    {
                        new ShellContent
                        {
                            ContentTemplate = new DataTemplate(() => new Issue34898SecondPage())
                        }
                    }
                });

                Items.Clear();
                Items.Add(tabBar);
                await GoToAsync("//secondRoot", true);
            })
        };

        _trackedPage = new ContentPage
        {
            Title = "First Page",
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(20),
                Children =
                {
                    _trackedLabel,
                    _trackedEntry,
                    _trackedButton,
                    clearAndNavigateButton
                }
            }
        };

        AddContentPage(_trackedPage, "First Page");
    }

    class Issue34898SecondPage : ContentPage
    {
        public Issue34898SecondPage()
        {
            Title = "Second Page";

            var pageHandlerStatus = new Label { AutomationId = "PageHandlerStatus" };
            var labelHandlerStatus = new Label { AutomationId = "LabelHandlerStatus" };
            var entryHandlerStatus = new Label { AutomationId = "EntryHandlerStatus" };
            var buttonHandlerStatus = new Label { AutomationId = "ButtonHandlerStatus" };

            var checkButton = new Button
            {
                Text = "Check Handler Status",
                AutomationId = "CheckHandlersButton",
                Command = new Command(() =>
                {
                    pageHandlerStatus.Text = _trackedPage?.Handler is null ? "Disconnected" : "Connected";
                    labelHandlerStatus.Text = _trackedLabel?.Handler is null ? "Disconnected" : "Connected";
                    entryHandlerStatus.Text = _trackedEntry?.Handler is null ? "Disconnected" : "Connected";
                    buttonHandlerStatus.Text = _trackedButton?.Handler is null ? "Disconnected" : "Connected";
                })
            };

            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Padding = new Thickness(20),
                Children =
                {
                    new Label { Text = "Second Root Page", FontSize = 18 },
                    checkButton,
                    new Label { Text = "Page Handler:" },
                    pageHandlerStatus,
                    new Label { Text = "Label Handler:" },
                    labelHandlerStatus,
                    new Label { Text = "Entry Handler:" },
                    entryHandlerStatus,
                    new Label { Text = "Button Handler:" },
                    buttonHandlerStatus
                }
            };
        }
    }
}
