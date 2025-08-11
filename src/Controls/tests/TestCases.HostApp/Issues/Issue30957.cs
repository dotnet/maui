using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30957, "FlexLayout Wrap Misalignment with Dynamically-Sized Buttons in .NET MAUI", PlatformAffected.All)]
    public class Issue30957 : ContentPage
    {
        private bool _fontToggled = false;
        private FlexLayout _testFlexLayout;
        private Label _statusLabel;

        public Issue30957()
        {
            Title = "Issue30957";

            var stackLayout = new StackLayout { Padding = new Thickness(20) };

            // Instructions
            var instructionsLabel = new Label
            {
                Text = "This test validates the Windows tolerance fix for FlexLayout wrapping precision issues. Click toggle to test.",
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackLayout.Children.Add(instructionsLabel);

            var toggleButton = new Button
            {
                Text = "Toggle Font",
                AutomationId = "ToggleFontButton",
                BackgroundColor = Colors.LightBlue,
                Margin = new Thickness(0, 0, 0, 10)
            };
            toggleButton.Clicked += OnToggleFontClicked;
            stackLayout.Children.Add(toggleButton);

            _testFlexLayout = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                BackgroundColor = Colors.LightGray,
                Padding = new Thickness(10)
            };

            var button1 = new Button
            {
                Text = "Button 1",
                AutomationId = "Button1",
                Margin = new Thickness(5),
                WidthRequest = 100
            };
            var button2 = new Button
            {
                Text = "Button 2",
                AutomationId = "Button2",
                Margin = new Thickness(5),
                WidthRequest = 100
            };
            var button3 = new Button
            {
                Text = "Button 3",
                AutomationId = "Button3",
                Margin = new Thickness(5),
                WidthRequest = 100
            };

            _testFlexLayout.Children.Add(button1);
            _testFlexLayout.Children.Add(button2);
            _testFlexLayout.Children.Add(button3);
            stackLayout.Children.Add(_testFlexLayout);

            _statusLabel = new Label
            {
                AutomationId = "StatusLabel",
                Text = "Status: Ready to test",
                Margin = new Thickness(0, 10, 0, 0),
                TextColor = Colors.DarkGreen
            };
            stackLayout.Children.Add(_statusLabel);

            Content = stackLayout;
        }

        private void OnToggleFontClicked(object sender, EventArgs e)
        {
            _fontToggled = !_fontToggled;

            foreach (var child in _testFlexLayout.Children)
            {
                if (child is Button button)
                {
                    button.FontSize = _fontToggled ? 14.1 : 14.0;

                    if (_fontToggled)
                        button.Text = button.Text.Replace("Button", "Btn", StringComparison.Ordinal);
                    else
                        button.Text = button.Text.Replace("Btn", "Button", StringComparison.Ordinal);
                }
            }

            _statusLabel.Text = $"Status: Font toggled {(_fontToggled ? "ON" : "OFF")}";
        }
    }
}
