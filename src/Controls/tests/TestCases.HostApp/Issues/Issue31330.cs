using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31330, "Rectangle renders as thin line instead of filled shape for small height values", PlatformAffected.Android)]
public class Issue31330 : ContentPage
{
    public Issue31330()
    {
        var grid = new Grid();

        var boxView = new BoxView
        {
            Color = Colors.Green,
            HeightRequest = 100,
            WidthRequest = 100,
            AutomationId = "Issue31330BoxView"
        };

        var ellipse = new Ellipse
        {
            Fill = new SolidColorBrush(Colors.Red),
            StrokeThickness = 20,
            WidthRequest = 100,
            HeightRequest = 100
        };

        grid.Children.Add(boxView);
        grid.Children.Add(ellipse);
        Content = grid;
    }
}