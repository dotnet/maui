using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 27430, "False positives of PointerGestureRecognizer on Windows with a second minimized window", PlatformAffected.UWP)]
    public partial class Issue27430 : ContentPage
    {
        private Window _secondWindow;
        private int _pointerEnterCount = 0;
        private int _pointerExitCount = 0;
        private DateTime _lastEventTime = DateTime.MinValue;

        public Issue27430()
        {
            InitializeComponent();
        }

        private void OnPointerEntered(object sender, PointerEventArgs e)
        {
            _pointerEnterCount++;
            _lastEventTime = DateTime.Now;
            UpdateEventCounts();
            
            // Change background to red on pointer enter
            TestGrid.BackgroundColor = Colors.Red;
            LastEventLabel.Text = $"Last Event: Entered at {_lastEventTime:HH:mm:ss.fff}";
        }

        private void OnPointerExited(object sender, PointerEventArgs e)
        {
            _pointerExitCount++;
            _lastEventTime = DateTime.Now;
            UpdateEventCounts();
            
            // Change background back to light blue on pointer exit
            TestGrid.BackgroundColor = Colors.LightBlue;
            LastEventLabel.Text = $"Last Event: Exited at {_lastEventTime:HH:mm:ss.fff}";
        }

        private void OnOpenWindowClicked(object sender, EventArgs e)
        {
            if (_secondWindow == null)
            {
                var secondPage = new ContentPage
                {
                    Title = "Second Window",
                    Content = new VerticalStackLayout
                    {
                        Padding = new Thickness(20),
                        Children =
                        {
                            new Label 
                            { 
                                Text = "This is the second window.\nMinimize this window and test hover behavior in the main window.",
                                AutomationId = "SecondWindowLabel"
                            }
                        }
                    }
                };

                _secondWindow = new Window(secondPage)
                {
                    Title = "Second Test Window"
                };

                // Subscribe to window closed event to clean up
                _secondWindow.Destroying += (s, e) => 
                {
                    _secondWindow = null;
                };

                Application.Current?.OpenWindow(_secondWindow);
            }
        }

        private void UpdateEventCounts()
        {
            EventCountLabel.Text = $"Events: {_pointerEnterCount} Enter, {_pointerExitCount} Exit";
        }
    }
}