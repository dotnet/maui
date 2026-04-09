namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34823, "WebView on Windows Does Not Inherit App Theme", PlatformAffected.UWP)]
public class Issue34823 : ContentPage
{
    readonly WebView _webView;
    public Issue34823()
    {
        _webView = new WebView
        {
            AutomationId = "ThemedWebView",
            HeightRequest = 260,
            Background = new SolidColorBrush(Colors.Green),
            Source = new HtmlWebViewSource
            {
                Html = """
<!DOCTYPE html>
<html>
<head>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <style>
    @media (prefers-color-scheme: dark) {
      html, body { background-color: black; color: white; }
    }
    @media (prefers-color-scheme: light) {
      html, body { background-color: white; color: black; }
    }
  </style>
</head>
<body>
  <h3>Issue34823 WebView Theme Probe</h3>
</body>
</html>
"""
            }
        };

        var forceDarkButton = new Button
        {
            AutomationId = "ForceDarkThemeButton",
            Text = "Force Dark Theme"
        };

        forceDarkButton.Clicked += (_, _) =>
        {
            if (Application.Current is null)
                return;

            Application.Current.UserAppTheme = AppTheme.Dark;
        };

        var forceLightButton = new Button
        {
            AutomationId = "ForceLightThemeButton",
            Text = "Force Light Theme"
        };

        forceLightButton.Clicked += (_, _) =>
        {
            if (Application.Current is null)
                return;

            Application.Current.UserAppTheme = AppTheme.Light;
        };

        Content = new VerticalStackLayout
        {
            Padding = 16,
            Spacing = 10,
            Children =
            {
                new Label
                {
                    Text = "This page verifies that explicit WebView background keeps the web color-scheme as light.",
                    AutomationId = "InstructionLabel"
                },
                forceDarkButton,
                forceLightButton,
                _webView
            }
        };
    }
}