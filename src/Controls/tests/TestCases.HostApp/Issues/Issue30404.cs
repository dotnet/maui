using Microsoft.Maui.Controls.Shapes;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30404, "[Windows] ContentView clip is not updated when wrapping inside the Border", PlatformAffected.UWP)]
public class Issue30404 : ContentPage
{
    public Issue30404()
    {
        var border = new Border
        {
            HeightRequest = 200,
            BackgroundColor = Colors.Green,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var customView = new Issue30404CustomContentView
        {
            HeightRequest = 100,
            WidthRequest = 100,
        };

        border.Content = customView;
        Content = border;
    }
}

public class Issue30404CustomContentView : ContentView
{
    public Issue30404CustomContentView()
    {
        BackgroundColor = Colors.Red;
        Content = new Label
        {
            Text = "Clipped Content",
            TextColor = Colors.Blue,
            AutomationId = "BorderContentLabel",
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, EventArgs e)
    {
        if (Width > 0 && Height > 0)
        {
            Clip = new RoundRectangleGeometry(25, new Rect(0, 0, Width, Height));
        }
    }
}