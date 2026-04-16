namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34993, "RadioButton background remains visible after being set to null",
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
