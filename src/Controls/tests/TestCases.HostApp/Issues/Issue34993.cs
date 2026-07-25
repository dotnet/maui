namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34993, "RadioButton still shows gradient after background changed to solid color brush",
    PlatformAffected.Android | PlatformAffected.UWP)]
public class Issue34993 : ContentPage
{
    public Issue34993()
    {
        var radioButton = new RadioButton
        {
            Content = "RadioButton Background Test",
            AutomationId = "BackgroundRadioButton",
            HorizontalOptions = LayoutOptions.Fill,
            Padding = new Thickness(10),
        };

        radioButton.Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop { Color = Colors.Red, Offset = 0.0f },
                new GradientStop { Color = Colors.Blue, Offset = 1.0f }
            },
            new Point(0, 0),
            new Point(1, 1)
        );

        var resetButton = new Button
        {
            Text = "Set Background to Blue",
            AutomationId = "ChangeBackgroundButton",
        };
        resetButton.Clicked += (s, e) =>
        {
            radioButton.Background = new SolidColorBrush(Colors.Blue);
        };

        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Margin = new Thickness(20),
            Children = { radioButton, resetButton }
        };
    }
}
