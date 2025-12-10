namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32971, "WebView content does not scroll when placed inside a ScrollView", PlatformAffected.Android)]
public class Issue32971 : ContentPage
{
    public Issue32971()
    {
        var webView = new WebView
        {
            Source = "https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/",
            HeightRequest = 1000,
        };

        var scrollView = new ScrollView
        {
            AutomationId = "TestScrollView",
            Content = webView,
        };

        Content = scrollView;
    }
}