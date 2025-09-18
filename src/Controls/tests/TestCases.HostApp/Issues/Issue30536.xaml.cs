using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30536, "PointerGestureRecognizer behaves incorrectly when multiple windows are open.", PlatformAffected.UWP)]
    public partial class Issue30536 : ContentPage
    {
        private Window _secondWindow;
        private int _pointerEnterCount = 0;
        private int _pointerExitCount = 0;

        public Issue30536()
        {
            InitializeComponent();
            UpdateWindowCount();
        }

        private void OnPointerEntered(object sender, PointerEventArgs e)
        {
            _pointerEnterCount++;
            UpdateEventCounts();
            
            // Change background to red on pointer enter
            if (sender is Grid grid)
            {
                grid.BackgroundColor = Colors.Red;
                StatusLabel.Text = "Pointer Entered";
            }
        }

        private void OnPointerExited(object sender, PointerEventArgs e)
        {
            _pointerExitCount++;
            UpdateEventCounts();
            
            // Change background back to light blue on pointer exit
            if (sender is Grid grid)
            {
                grid.BackgroundColor = Colors.LightBlue;
                StatusLabel.Text = "Pointer Exited";
            }
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
                                Text = "This is the second window.\nMinimize this window and test the main window's pointer behavior.",
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
                    UpdateWindowCount();
                };

                Application.Current?.OpenWindow(_secondWindow);
                UpdateWindowCount();
            }
        }

        private void OnCloseWindowClicked(object sender, EventArgs e)
        {
            if (_secondWindow != null)
            {
                Application.Current?.CloseWindow(_secondWindow);
                _secondWindow = null;
                UpdateWindowCount();
            }
        }

        private void UpdateEventCounts()
        {
            EventCountLabel.Text = $"Pointer Events: {_pointerEnterCount} Enter, {_pointerExitCount} Exit";
        }

        private void UpdateWindowCount()
        {
            var windowCount = Application.Current?.Windows?.Count ?? 1;
            WindowCountLabel.Text = $"Windows Open: {windowCount}";
        }
    }
}