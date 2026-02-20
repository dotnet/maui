namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31363, "The images under the I-CollectionView category are not showing up", PlatformAffected.UWP)]
public class Issue31363 : ContentPage
{
    public Issue31363()
    {
        var layout = new VerticalStackLayout();

        var image = new Image
        {
            HeightRequest = 60,
            WidthRequest = 60,
            AutomationId = "TestImage",
            Source = new UriImageSource
            {
                Uri = new Uri("https://upload.wikimedia.org/wikipedia/commons/thumb/1/13/Gelada-Pavian.jpg/320px-Gelada-Pavian.jpg")
            }
        };

        layout.Children.Add(image);
        Content = layout;
    }
}