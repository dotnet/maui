using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14164, "[iOS, MAC] Border overlapped with content view label when setting clip", PlatformAffected.iOS)]
public class Issue14164 : ContentPage
{
    ContentView _contentView;
    public Issue14164()
    {
        var border = new Border
        {
            HeightRequest = 200,
            WidthRequest = 200,
            BackgroundColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = _contentView = new ContentView
            {
                HeightRequest = 100,
                WidthRequest = 100,
                BackgroundColor = Colors.Yellow,
            }
        };

        var button1 = new Button
        {
            Text = "Add clip to Border Content",
            AutomationId = "AddClipButton",
            Command = new Command(() =>
            {
                _contentView.Clip = new RoundRectangleGeometry(25, new Rect(0, 0, _contentView.Width, _contentView.Height));
            })
        };

        var button2 = new Button
        {
            Text = "Remove clip from Border Content",
            AutomationId = "RemoveClipButton",
            Command = new Command(() =>
            {
                _contentView.Clip = null;
            })
        };

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            Children =
            {
                button1,
                button2,
                border
            }
        };
    }
}
