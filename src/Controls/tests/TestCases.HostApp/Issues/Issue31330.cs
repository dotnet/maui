using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31330, "Rectangle renders as thin line instead of filled shape for small height values", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue31330 : ContentPage
{
    public Issue31330()
    {
        var scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Both,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
        };

        var grid = new Grid
        {
            WidthRequest = 800,
            HeightRequest = 600,
            BackgroundColor = Colors.LightGray,
            RowSpacing = 10,
            Padding = 20
        };

        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

        // Instructions
        var instructions = new Label
        {
            Text = "Test passes if:\n1. Red BoxView (height 1.2) is visible as a filled rectangle\n2. Blue Rectangle (height 1.2) is visible as a filled rectangle (not a thin line)\n3. Both should have similar appearance",
            FontAttributes = FontAttributes.Bold,
            AutomationId = "Instructions"
        };
        Grid.SetRow(instructions, 0);
        grid.Children.Add(instructions);

        // BoxView with small height (reference for correct rendering)
        var boxViewLabel = new Label { Text = "BoxView (height 1.2):" };
        Grid.SetRow(boxViewLabel, 1);
        grid.Children.Add(boxViewLabel);

        var boxView = new BoxView
        {
            Color = Colors.Red,
            WidthRequest = 50,
            HeightRequest = 1.2,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            AutomationId = "TestBoxView"
        };
        Grid.SetRow(boxView, 1);
        grid.Children.Add(boxView);

        // Rectangle with small height (should render like BoxView, not as a line)
        var rectangleLabel = new Label { Text = "Rectangle (height 1.2, Fill only, no Stroke):" };
        Grid.SetRow(rectangleLabel, 2);
        grid.Children.Add(rectangleLabel);

        var rectangle = new Rectangle
        {
            WidthRequest = 50,
            HeightRequest = 1.2,
            Fill = Colors.Blue,
            Stroke = null, // Explicitly no stroke
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            AutomationId = "TestRectangle"
        };
        Grid.SetRow(rectangle, 2);
        grid.Children.Add(rectangle);

        // AbsoluteLayout test (from original issue report)
        var absoluteLayout = new AbsoluteLayout
        {
            BackgroundColor = Colors.White,
            AutomationId = "AbsoluteLayoutTest"
        };
        Grid.SetRow(absoluteLayout, 3);
        grid.Children.Add(absoluteLayout);

        double shapeWidth = 20;
        double shapeHeight = 1.2;
        double centerX = 400;
        double centerY = 200;

        // BoxView in AbsoluteLayout (reference)
        var absBoxView = new BoxView
        {
            BackgroundColor = Colors.Red
        };
        AbsoluteLayout.SetLayoutBounds(absBoxView, new Rect(
            centerX - shapeWidth - 30,
            centerY - (shapeHeight / 2),
            shapeWidth,
            shapeHeight
        ));
        AbsoluteLayout.SetLayoutFlags(absBoxView, AbsoluteLayoutFlags.None);
        absoluteLayout.Children.Add(absBoxView);

        // Rectangle in AbsoluteLayout (should match BoxView appearance)
        var absRectangle = new Rectangle
        {
            Fill = Colors.Blue
        };
        AbsoluteLayout.SetLayoutBounds(absRectangle, new Rect(
            centerX + 30,
            centerY - (shapeHeight / 2),
            shapeWidth,
            shapeHeight
        ));
        AbsoluteLayout.SetLayoutFlags(absRectangle, AbsoluteLayoutFlags.None);
        absoluteLayout.Children.Add(absRectangle);

        scrollView.Content = grid;
        Content = scrollView;
    }
}