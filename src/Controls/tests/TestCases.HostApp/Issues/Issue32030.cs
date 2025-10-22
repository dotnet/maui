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
            BackgroundColor = Colors.BurlyWood
        };
        webView.Navigated += WebView_Navigated;
        grid.Add(webView);
        Grid.SetRow(webView, 1);

        // Create "below webview" Label
        var bottomLabel = new Label
        {
            Text = "Below webview",
            HorizontalTextAlignment = TextAlignment.Center
        };
        grid.Add(bottomLabel);
        Grid.SetRow(bottomLabel, 2);

        // Set the grid as the page content
        Content = grid;
    }

    async void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Navigated to: {e.Url}");
        await DisplayAlert("Navigation Completed", $"WebView navigated to: {e.Url}", "OK");
    }
}