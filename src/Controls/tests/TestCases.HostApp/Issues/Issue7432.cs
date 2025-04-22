namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7432, "Android Image.Scale produces wrong layout", PlatformAffected.Android)]
public class Issue7432 : ContentPage
{
    public Issue7432()
    {
        var grid = new Grid
        {
            BackgroundColor = Colors.Gray
        };

        var image = new Image
        {
            Source = "dotnet_bot.png",
            BackgroundColor = Colors.Red,
            WidthRequest = 100,
            HeightRequest = 100,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Scale = 0.5,
            AutomationId = "Image"
        };

        grid.Children.Add(image);

        Content = grid;
    }
}