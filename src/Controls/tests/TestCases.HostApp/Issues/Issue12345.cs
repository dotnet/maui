namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12345, "RadioButton background remains visible after being set to null on Android",
    PlatformAffected.Android)]
public class Issue12345 : ContentPage
{
    public Issue12345()
    {
        var radioButton = new RadioButton
        {
            Content = "RadioButton Background Test",
            Background = new SolidColorBrush(Colors.Red),
            AutomationId = "BackgroundRadioButton",
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(10),
        };

        var resetButton = new Button
        {
            Text = "Set Background to null",
            AutomationId = "ResetBackgroundButton",
        };
        resetButton.Clicked += (s, e) =>
        {
            radioButton.Background = null;
        };

        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Margin = new Thickness(20),
            Children = { radioButton, resetButton }
        };
    }
}
