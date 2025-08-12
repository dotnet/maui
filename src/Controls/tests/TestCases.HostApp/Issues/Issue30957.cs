using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30957, "FlexLayout Wrap Misalignment with Dynamically-Sized Buttons in .NET MAUI", PlatformAffected.All)]
    public class Issue30957 : ContentPage
    {
        private FlexLayout _testFlexLayout;
        private Label _statusLabel;
        private Button _toggleButton1, _toggleButton2, _toggleButton3;
        private bool _isToggled = false;

        public Issue30957()
        {
            Title = "Issue30957";

            var stackLayout = new StackLayout { Padding = new Thickness(20) };

            var instructionsLabel = new Label
            {
                AutomationId = "Issue30957FirstLabel",
                Text = "This reproduces FlexLayout wrapping issue with font family switching. Click 'Toggle Font' to trigger precision issues.",
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackLayout.Children.Add(instructionsLabel);

            var toggleAllButton = new Button
            {
                Text = "Toggle Font Family",
                AutomationId = "Issue30957ToggleButton",
                BackgroundColor = Colors.LightBlue,
                Margin = new Thickness(0, 0, 0, 10)
            };
            toggleAllButton.Clicked += OnToggleFontClicked;
            stackLayout.Children.Add(toggleAllButton);

            var border = new Border
            {
                Stroke = Colors.Black,
                StrokeThickness = 1,
                BackgroundColor = Colors.White,
                HorizontalOptions = LayoutOptions.Start,
                Padding = new Thickness(4)
            };

            _testFlexLayout = new FlexLayout
            {
                Wrap = FlexWrap.Wrap
            };

            _toggleButton1 = new Button
            {
                Text = "Button1",
                AutomationId = "Issue30957Button1",
                BackgroundColor = Colors.White,
                FontFamily = "OpenSansRegular",
                CornerRadius = 0,
                FontSize = 16,
                TextColor = Colors.Black
            };

            _toggleButton2 = new Button
            {
                Text = "Button2",
                AutomationId = "Issue30957Button2",
                BackgroundColor = Colors.White,
                FontFamily = "OpenSansRegular",
                CornerRadius = 0,
                FontSize = 16,
                TextColor = Colors.Black
            };

            _toggleButton3 = new Button
            {
                Text = "Button3",
                AutomationId = "Issue30957Button3",
                BackgroundColor = Colors.White,
                FontFamily = "OpenSansRegular",
                CornerRadius = 0,
                FontSize = 16,
                TextColor = Colors.Black
            };

            _testFlexLayout.Children.Add(_toggleButton1);
            _testFlexLayout.Children.Add(_toggleButton2);
            _testFlexLayout.Children.Add(_toggleButton3);

            border.Content = _testFlexLayout;
            stackLayout.Children.Add(border);

            _statusLabel = new Label
            {
                AutomationId = "Issue30957StatusLabel",
                Text = "Status: Ready to test - Click 'Toggle Font Family' to trigger precision issue",
                Margin = new Thickness(0, 10, 0, 0),
                TextColor = Colors.DarkGreen
            };
            stackLayout.Children.Add(_statusLabel);

            Content = stackLayout;
        }

        private void OnToggleFontClicked(object sender, EventArgs e)
        {
            _isToggled = !_isToggled;

            string fontFamily = _isToggled ? "OpenSansSemibold" : "OpenSansRegular";
            Color backgroundColor = _isToggled ? Colors.Black : Colors.White;
            Color textColor = _isToggled ? Colors.White : Colors.Black;

            _toggleButton1.FontFamily = fontFamily;
            _toggleButton1.BackgroundColor = backgroundColor;
            _toggleButton1.TextColor = textColor;

            _toggleButton2.FontFamily = fontFamily;
            _toggleButton2.BackgroundColor = backgroundColor;
            _toggleButton2.TextColor = textColor;

            _toggleButton3.FontFamily = fontFamily;
            _toggleButton3.BackgroundColor = backgroundColor;
            _toggleButton3.TextColor = textColor;

            _statusLabel.Text = $"Status: Font toggled to {fontFamily} - This triggers precision issue that tolerance fix addresses";
        }
    }
}