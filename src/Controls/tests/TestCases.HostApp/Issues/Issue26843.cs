    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Issue(IssueTracker.Github, 26843, "WebView Fails to Load URLs with Certain Encoded Characters on Android", PlatformAffected.Android)]
    public partial class Issue26843 : ContentPage
    {
        private Grid layout;

        public Issue26843()
        {
            var webViewSourceEntry = new Entry
            {
                AutomationId = "WebViewSourceEntry",
                Placeholder = "Enter URL"
            };
            var loadButton = new Button
            {
                AutomationId = "LoadUrlButton",
                Text = "Load URL"
            };
            loadButton.Clicked += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(webViewSourceEntry.Text))
                {
                    var existingWebView = layout.Children.FirstOrDefault(c => c is WebView);
                    if (existingWebView != null)
                    {
                        layout.Children.Remove(existingWebView);
                    }
                    var webView = new WebView
                    {
                        AutomationId = "WebView",
                        ZIndex = 1,
                        Source = webViewSourceEntry.Text
                    };
                    layout.Children.Add(webView);
                    Grid.SetRow(webView, 2);
                }
            };
            layout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                Children = { webViewSourceEntry, loadButton }
            };
            Grid.SetRow(webViewSourceEntry, 0);
            Grid.SetRow(loadButton, 1);
            Content = layout;
        }
    }
