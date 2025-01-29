using Microsoft.Maui.Controls;
using System.Linq;
 
namespace Maui.Controls.Sample.Issues
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Issue(IssueTracker.Github, 26843, "WebView Fails to Load URLs with Certain Encoded Characters on Android", PlatformAffected.Android, isInternetRequired:true)]
    public partial class Issue26843 : ContentPage
    {
        private Grid layout;
        private Label navigationResultLabel;
        private bool isSourceSet;
 
        public Issue26843()
        {
            navigationResultLabel = new Label
            {
                AutomationId = "NavigationResultLabel",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Colors.Black,
                FontSize = 14
            };
 
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
 
                    webView.Navigated += OnNavigated;
                    isSourceSet = true;
 
                    layout.Children.Add(webView);
                    Grid.SetRow(webView, 3);
                }
            };
 
            layout = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
                },
                Children = { navigationResultLabel, webViewSourceEntry, loadButton }
            };
 
            Grid.SetRow(navigationResultLabel, 0);
            Grid.SetRow(webViewSourceEntry, 1);
            Grid.SetRow(loadButton, 2);
 
            Content = layout;
        }
 
        private void OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            if (isSourceSet)
            {
                if (e.Result == WebNavigationResult.Success)
                {
                    navigationResultLabel.Text = "Successfully navigated to the encoded URL";
                }
                else
                {
                    navigationResultLabel.Text = "Failed to navigate to the encoded URL";
                }
                isSourceSet = false;
            }
        }
    }
}
 
 