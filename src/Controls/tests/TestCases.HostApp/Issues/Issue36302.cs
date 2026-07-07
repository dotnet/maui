namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 36302, "Image and ImageButton backgrounds not cleared on iOS/MacCatalyst when set to null",
    PlatformAffected.iOS)]
public class Issue36302 : ContentPage
{
    public Issue36302()
    {
        var image = new Image
        {
            Source = "dotnet_bot.png",
            HeightRequest = 80,
            WidthRequest = 80,
            Background = new SolidColorBrush(Colors.Blue),
            AutomationId = "BackgroundImage",
        };

        var imageButton = new ImageButton
        {
            Source = "dotnet_bot.png",
            HeightRequest = 80,
            WidthRequest = 80,
            Background = new SolidColorBrush(Colors.Blue),
            AutomationId = "BackgroundImageButton",
        };

        var clearButton = new Button
        {
            Text = "Clear Backgrounds",
            AutomationId = "ClearBackgroundsButton",
        };
        clearButton.Clicked += (s, e) =>
        {
            image.Background = null;
            imageButton.Background = null;
        };

        Content = new VerticalStackLayout
        {
            Spacing = 20,
            Margin = new Thickness(20),
            Children =
            {
                image,
                imageButton,
                clearButton,
            }
        };
    }
}
