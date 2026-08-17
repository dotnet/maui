namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37335, "Orientation property as Horizontal is not working properly in ScrollView", PlatformAffected.Android)]
public class Issue37335 : ContentPage
{
    public Issue37335()
    {
        Title = "Issue 37335";
        var text = string.Join(
            Environment.NewLine,
            Enumerable.Range(1, 60).Select(i =>
                $"Row {i:D2} — Hello, World! This is a sample of a very long text that will require scrolling to view completely."));

        var scrollView = new ScrollView
        {
            AutomationId = "HorizontalScrollView",
            Orientation = ScrollOrientation.Horizontal,
            VerticalScrollBarVisibility = ScrollBarVisibility.Always,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Always,
            Content = new Label
            {
                AutomationId = "ScrollContent",
                Text = text,
                TextColor = Colors.Black,
                BackgroundColor = Colors.Red,
                FontSize = 18,
                WidthRequest = 1500,
                HeightRequest = 2500
            }
        };
        Content = scrollView;
    }
}
