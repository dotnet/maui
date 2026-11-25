namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38452, "WebView inside a ScrollView blocks page scrolling when WebView has nothing to scroll", PlatformAffected.Android)]
public class Issue38452 : ContentPage
{
    public Issue38452()
    {
        var scrollStateLabel = new Label
        {
            Text = "NotScrolled",
            HeightRequest = 400,
            AutomationId = "ScrollStateLabel",
            FontSize = 20,
            HorizontalTextAlignment = TextAlignment.Center,
        };

        var webView = new WebView
        {
            AutomationId = "TestWebView",
            HeightRequest = 200,
            VerticalOptions = LayoutOptions.Start,
        };

        webView.Source = new HtmlWebViewSource
        {
            Html = @"
                <!DOCTYPE html>
                <html>
                <body>
                    <p>This content fits completely inside the WebView.</p>
                </body>
                </html>"
        };

        var webViewContainer = new Grid
        {
            AutomationId = "WebViewContainer",
            HeightRequest = 200,
            Children = { webView },
        };

        var scrollView = new ScrollView
        {
            AutomationId = "TestScrollView",
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Spacing = 16,
                Children =
                {
                    new Label
                    {
                        Text = "Drag on the WebView below. The parent page should scroll.",
                        HeightRequest = 200,
                        BackgroundColor = Colors.LightGray,
                    },

                    webViewContainer,
                    scrollStateLabel,
                }
            }
        };

        scrollView.Scrolled += (sender, e) =>
        {
            scrollStateLabel.Text = "Scrolled";
        };

        Content = scrollView;
    }
}
