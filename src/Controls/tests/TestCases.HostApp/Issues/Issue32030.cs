namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32030, "Android - WebView in a grid expands beyond it's cell", PlatformAffected.Android)]

public class Issue32030 : ContentPage
{
    WebView webView;

    public Issue32030()
    {
        // Create Grid
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = new GridLength(50) },
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = new GridLength(50) }
            }
        };

        // Create "above webview" Label
        var topLabel = new Label
        {
            Text = "Above webview",
            AutomationId = "TopLabel",
            HorizontalTextAlignment = TextAlignment.Center
        };
        grid.Add(topLabel);
        Grid.SetRow(topLabel, 0);

        // Create WebView
        webView = new WebView
        {
            BackgroundColor = Colors.BurlyWood,
            Source = new HtmlWebViewSource
            {
                Html = @"<html><body><h1>Initial Content</h1></body></html>"
            }
        };
        grid.Add(webView);
        Grid.SetRow(webView, 1);

        var button = new Button
        {
            Text = "Click Me",
            AutomationId = "BottomButton",
        };

        button.Clicked += (s, e) =>
        {
            webView.Source = null;
        };
        grid.Add(button);
        Grid.SetRow(button, 2);

        // Set the grid as the page content
        Content = grid;
    }
}