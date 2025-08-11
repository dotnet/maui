using System.Collections.ObjectModel;
using System.Text;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30536, "[Windows] PointerGestureRecognizer behaves incorrectly when multiple windows are open", PlatformAffected.UWP)]
public partial class Issue30536 : ContentPage
{
    private readonly ObservableCollection<string> _eventLog = new();
    private int _eventCount = 0;
    private DateTime _lastLogTime = DateTime.Now;

    public Issue30536()
    {
        InitializeComponent();
        UpdateWindowCount();
    }

    private void OnOpenWindowClicked(object sender, EventArgs e)
    {
        try
        {
            // Create a simple second window
            var secondWindow = new Window
            {
                Title = "Second Window - Issue 30536",
                Page = new ContentPage
                {
                    Title = "Second Window",
                    Content = new StackLayout
                    {
                        Padding = 20,
                        Children =
                        {
                            new Label 
                            { 
                                Text = "This is the second window for testing multi-window pointer gesture behavior.",
                                FontSize = 16,
                                Margin = new Thickness(0, 0, 0, 20)
                            },
                            new Label 
                            { 
                                Text = "Instructions:",
                                FontAttributes = FontAttributes.Bold,
                                Margin = new Thickness(0, 0, 0, 10)
                            },
                            new Label 
                            { 
                                Text = "1. Minimize this window\n2. Go back to the main window\n3. Move your mouse in and out of the blue test area\n4. Check that PointerEntered/Exited events behave correctly",
                                Margin = new Thickness(0, 0, 0, 20)
                            },
                            new Button
                            {
                                Text = "Close This Window",
                                AutomationId = "CloseSecondWindowButton",
                                Command = new Command(() => Application.Current?.CloseWindow(secondWindow))
                            }
                        }
                    }
                }
            };

            Application.Current?.OpenWindow(secondWindow);
            
            // Update window count after opening
            Dispatcher.BeginInvokeOnMainThread(() =>
            {
                UpdateWindowCount();
                LogEvent("Second window opened");
            });
        }
        catch (Exception ex)
        {
            LogEvent($"Error opening window: {ex.Message}");
        }
    }

    private void OnClearLogsClicked(object sender, EventArgs e)
    {
        _eventLog.Clear();
        _eventCount = 0;
        EventCountLabel.Text = "0";
        EventLogLabel.Text = "Event Log (most recent first):";
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {
        var timestamp = DateTime.Now;
        var timeSinceLastEvent = timestamp - _lastLogTime;
        _lastLogTime = timestamp;
        
        LogEvent($"PointerEntered at {timestamp:HH:mm:ss.fff} (Δ{timeSinceLastEvent.TotalMilliseconds:F0}ms)");
    }

    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        var timestamp = DateTime.Now;
        var timeSinceLastEvent = timestamp - _lastLogTime;
        _lastLogTime = timestamp;
        
        LogEvent($"PointerExited at {timestamp:HH:mm:ss.fff} (Δ{timeSinceLastEvent.TotalMilliseconds:F0}ms)");
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        // Only log occasionally to avoid spam, but still verify the events are working
        if (_eventCount % 10 == 0)
        {
            LogEvent($"PointerMoved (logged every 10th event)");
        }
        _eventCount++;
        EventCountLabel.Text = _eventCount.ToString();
    }

    private void LogEvent(string eventDescription)
    {
        _eventCount++;
        
        // Add to the beginning of the collection so most recent events appear first
        _eventLog.Insert(0, eventDescription);
        
        // Keep only the last 20 events to prevent memory issues
        while (_eventLog.Count > 20)
        {
            _eventLog.RemoveAt(_eventLog.Count - 1);
        }
        
        // Update the label with all events
        var logBuilder = new StringBuilder("Event Log (most recent first):\n");
        foreach (var eventLog in _eventLog)
        {
            logBuilder.AppendLine(eventLog);
        }
        
        EventLogLabel.Text = logBuilder.ToString();
        EventCountLabel.Text = _eventCount.ToString();
    }

    private void UpdateWindowCount()
    {
        var windowCount = Application.Current?.Windows?.Count ?? 1;
        WindowCountLabel.Text = windowCount.ToString();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateWindowCount();
        LogEvent("Page loaded - Test area ready");
    }
}