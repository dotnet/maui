using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31330, "Rectangle renders as thin line instead of filled shape for small height values", PlatformAffected.Android)]
public class Issue31330 : ContentPage
{
    public Issue31330()
    {
        Grid grid = new Grid();
        grid.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Star },
            new RowDefinition { Height = GridLength.Star }
        };

        Label labelText = new Label { Text = "The test passes if the edges of the circle touch the BoxView.", FontAttributes = FontAttributes.Bold };

        BoxView boxView = new BoxView
        {
            Color = Colors.Green,
            HeightRequest = 100,
            WidthRequest = 100,
            AutomationId = "Issue31330BoxView"
        };

        Ellipse ellipse = new Ellipse
        {
            Fill = Colors.Yellow,
            StrokeThickness = 0,
            WidthRequest = 100,
            HeightRequest = 100
        };

        Button button = new Button { Text = "Update StrokeThickness" };
        button.Clicked += (s, e) =>
        {
            ellipse.StrokeThickness = 20;
        };

        Label label = new Label
        {
            Text = "Test passes if the Rectangle renders as filled shape for small height",
            FontAttributes = FontAttributes.Bold
        };

        Rectangle rectangle = new Rectangle
        {
            WidthRequest = 50,
            HeightRequest = 1.2,
            Fill = Colors.Blue
        };

        Grid bottomGrid = new Grid { Background = Colors.AliceBlue };
        bottomGrid.Children.Add(label);
        bottomGrid.Children.Add(rectangle);

        bottomGrid.RowDefinitions = new RowDefinitionCollection
        {
            new RowDefinition { Height = GridLength.Auto },
            new RowDefinition { Height = GridLength.Star },
        };

        bottomGrid.SetRow(label, 0);
        bottomGrid.SetRow(rectangle, 1);

        grid.Children.Add(labelText);
        grid.SetRow(labelText, 0);
        grid.Children.Add(button);
        grid.SetRow(button, 1);
        grid.Children.Add(boxView);
        grid.SetRow(boxView, 2);
        grid.Children.Add(ellipse);
        grid.SetRow(ellipse, 2);
        grid.SetRow(bottomGrid, 3);
        grid.Children.Add(bottomGrid);

        Content = grid;
    }
}