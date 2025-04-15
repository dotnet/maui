namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26795, "Specifying HeightRequest in Webview when wrapped by ScrollView set invisible causes crash in iOS", PlatformAffected.iOS)]
public class Issue26795 : ContentPage
{
    public Issue26795()
    {
        var layout = new VerticalStackLayout();

        var webView = new WebView
        {
            Source = "https://en.m.wikipedia.org/wiki",
            HeightRequest = 1000,
            AutomationId = "WebView"
        };

        var label = new Label
        {
            Text = "Test passes if no crash occurs",
            AutomationId = "Label"
        };

        var scrollView = new ScrollView
        {
            IsVisible = false,
            Content = webView
        };
        layout.Children.Add(label);
        layout.Children.Add(scrollView);
        Content = layout;
    }
}