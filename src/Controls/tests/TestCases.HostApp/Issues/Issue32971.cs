namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32971, "WebView content does not scroll when placed inside a ScrollView", PlatformAffected.Android)]
public class Issue32971 : ContentPage
{
    public Issue32971()
    {
        Label _scrollStateLabel = new Label
        {
            Text = "NotScrolled",
            AutomationId = "ScrollStateLabel",
            FontSize = 20,
            HorizontalTextAlignment = TextAlignment.Center,
        };

        WebView _webView = new WebView
        {
            AutomationId = "TestWebView",
            HeightRequest = 600,
            VerticalOptions = LayoutOptions.Start,
        };

        _webView.Source = new HtmlWebViewSource
        {
            Html = @"
            <!DOCTYPE html>
            <html>
            <head>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    body { font-family: Arial; padding: 20px; margin: 0; }
                    .section { border: 2px solid #0066cc; padding: 20px; margin: 10px 0; background: #f0f8ff; min-height: 300px; }
                </style>
            </head>
            <body>
                <h1>WebView Scrolling Test</h1>
                <div class='section'><h2>Section 1</h2><p>Scroll down to test...</p></div>
                <div class='section'><h2>Section 2</h2><p>Keep scrolling...</p></div>
                <div class='section'><h2>Section 3</h2><p>More content...</p></div>
                <div class='section'><h2>Section 4</h2><p>Almost there...</p></div>
                <div class='section'><h2>Section 5</h2><p>Bottom reached!</p></div>
            </body>
            </html>"
        };

        var checkButton = new Button
        {
            Text = "Check Scroll State",
            AutomationId = "CheckButton",
        };

        checkButton.Clicked += async (s, e) =>
        {
            var result = await _webView.EvaluateJavaScriptAsync("Math.round(window.pageYOffset);");
            if (int.TryParse(result, out int scrollPos) && scrollPos > 50)
            {
                _scrollStateLabel.Text = "Scrolled";
            }
        };

        var scrollView = new ScrollView
        {
            AutomationId = "TestScrollView",
            Content = new VerticalStackLayout
            {
                Children = { _scrollStateLabel, checkButton, _webView }
            }
        };

        Content = scrollView;
    }
}