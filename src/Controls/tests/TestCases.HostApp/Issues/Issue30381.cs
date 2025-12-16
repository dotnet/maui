namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30381, "WebView GoBack/GoForward not working for HtmlWebViewSource on iOS", PlatformAffected.iOS)]
public class Issue30381 : ContentPage
{
    WebView MyWebView;
    Label CanGoBackLabel;
    Label CanGoForwardLabel;
    Button ClickLinkButton;
    Button GoBackButton;
    Button GoForwardButton;
    Button UpdateStatusButton;

    public Issue30381()
    {
        CreateUI();
        SetupWebView();
        MyWebView.Navigated += OnWebViewNavigated;
    }

    void CreateUI()
    {
        // Create all UI elements in code
        var instructionLabel = new Label
        {
            Text = "Click the link in WebView to navigate, then test CanGoForward",
            AutomationId = "InstructionLabel",
            FontAttributes = FontAttributes.Bold
        };

        MyWebView = new WebView
        {
            AutomationId = "TestWebView",
            HeightRequest = 300
        };

        GoBackButton = new Button
        {
            Text = "Go Back",
            AutomationId = "GoBackButton"
        };
        GoBackButton.Clicked += OnGoBackClicked;

        GoForwardButton = new Button
        {
            Text = "Go Forward",
            AutomationId = "GoForwardButton"
        };
        GoForwardButton.Clicked += OnGoForwardClicked;

        ClickLinkButton = new Button
        {
            Text = "Click Link",
            AutomationId = "ClickLinkButton"
        };
        ClickLinkButton.Clicked += OnClickLinkClicked;

        UpdateStatusButton = new Button
        {
            Text = "Get Status",
            AutomationId = "UpdateStatusButton"
        };
        UpdateStatusButton.Clicked += OnUpdateStatusClicked;

        var buttonLayout = new HorizontalStackLayout
        {
            Spacing = 10,
            Children = { GoBackButton, GoForwardButton, ClickLinkButton, UpdateStatusButton }
        };

        CanGoBackLabel = new Label
        {
            AutomationId = "CanGoBackLabel",
            Text = "CanGoBack: False"
        };

        CanGoForwardLabel = new Label
        {
            AutomationId = "CanGoForwardLabel",
            Text = "CanGoForward: False"
        };

        Content = new VerticalStackLayout
        {
            Padding = 20,
            Spacing = 10,
            Children =
            {
                instructionLabel,
                MyWebView,
                buttonLayout,
                CanGoBackLabel,
                CanGoForwardLabel,
            }
        };
    }

    void SetupWebView()
    {
        MyWebView.Source = new HtmlWebViewSource
        {
            Html = @"
            <html>
            <head>
                <title>Wikipedia</title>
                <style>
                    body { font-family: Arial; padding: 16px; line-height: 1.6; }
                    h1 { color: #3366cc; }
                    a { color: #0645ad; text-decoration: none; padding: 5px; background: #f0f0f0; border-radius: 3px; }
                    a:hover { background: #e0e0e0; }
                </style>
            </head>
            <body>
                <h1>Welcome to Wikipedia</h1>
                <p>Wikipedia is a free online encyclopedia.</p>
                <p>Read more <a href='https://github.com/dotnet/maui' id='testLink'>here</a>.</p>
            </body>
            </html>"
        };

        UpdateNavigationStatus();
    }

    async void OnClickLinkClicked(object sender, EventArgs e)
    {
        // Use JavaScript to programmatically click the link
        await MyWebView.EvaluateJavaScriptAsync("document.getElementById('testLink').click();");
        // Don't update status here - user needs to click Update Status button
    }

    void OnUpdateStatusClicked(object sender, EventArgs e)
    {
        UpdateNavigationStatus();
    }

    void OnGoBackClicked(object sender, EventArgs e)
    {
        if (MyWebView.CanGoBack)
        {
            MyWebView.GoBack();
        }
        
        UpdateNavigationStatus();
    }

    void OnGoForwardClicked(object sender, EventArgs e)
    {
        if (MyWebView.CanGoForward)
        {
            MyWebView.GoForward();
        }
        
        UpdateNavigationStatus();
    }

    void UpdateNavigationStatus()
    {
        CanGoBackLabel.Text = $"CanGoBack: {MyWebView.CanGoBack}";
        CanGoForwardLabel.Text = $"CanGoForward: {MyWebView.CanGoForward}";
    }

    void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        if (e.Url.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            // Update navigation status after navigation completes
            UpdateNavigationStatus();
        }
    }
}