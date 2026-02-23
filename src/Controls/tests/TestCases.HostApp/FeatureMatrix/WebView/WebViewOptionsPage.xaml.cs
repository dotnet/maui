using System.Net;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample;

public partial class WebViewOptionsPage : ContentPage
{
	private WebViewViewModel _viewModel;
	public WebViewOptionsPage(WebViewViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}
	private async void ApplyButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
	private void OnHtmlSourceClicked(object sender, EventArgs e)
	{
		_viewModel.Source = new HtmlWebViewSource
		{
			Html = @"
            <html>
            <head>
                <title>HTML WebView Source</title>
            </head>
            <body style='font-family:sans-serif; padding:20px;'>
                <h1>WebView Feature Matrix</h1>
                <p>This page demonstrates various capabilities of the .NET MAUI WebView control, such as:</p>
                <ul>
                    <li>Rendering HTML content</li>
                    <li>Executing JavaScript</li>
                    <li>Cookie management</li>
                    <li>Back/Forward navigation</li>
                </ul>
                <h2>Test Content</h2>
                <p>
                    This is a longer body paragraph to help test the <strong>EvaluateJavaScript</strong> functionality 
                    and how it extracts body text. You can use this text to verify substring operations and test scrolling 
                    or formatting in the WebView.
                </p>
                <p>
                    Try interacting with navigation buttons, loading multiple pages, or checking cookie behavior.
                </p>
                <footer style='margin-top:40px; font-size:0.9em; color:gray;'>Generated for testing WebView features.</footer>
            </body>
            </html>",
		};
	}
	private void OnMicrosoftUrlClicked(object sender, EventArgs e)
	{
		_viewModel.Source = new UrlWebViewSource
		{
			Url = "https://www.microsoft.com"
		};
	}
	private void OnGithubUrlClicked(object sender, EventArgs e)
	{
		_viewModel.Source = new UrlWebViewSource
		{
			Url = "https://github.com/dotnet/maui"
		};
	}
	private void LoadMultiplePages_Clicked(object sender, EventArgs e)
	{
		_viewModel.Source = new HtmlWebViewSource
		{
			Html = @"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Multiple Pages Navigation</title>
        <style>
            body { font-family: Arial; padding: 16px; line-height: 1.6; }
            h1 { color: #3366cc; }
            a {
                display: inline-block;
                margin: 10px 5px;
                padding: 10px 16px;
                background-color: #0078D4;
                color: white;
                text-decoration: none;
                border-radius: 5px;
            }
            a:hover { background-color: #005a9e; }
        </style>
    </head>
    <body>
        <h1>Welcome to Page 1</h1>
        <p>This is the first page.</p>
        <p><a href='https://dotnet.microsoft.com'>Go to Page 2 (Microsoft)</a></p>
        <p><a href='https://github.com/dotnet/maui'>Go to Page 3 (GitHub)</a></p>
    </body>
    </html>"
		};
	}
	private void AddTestCookie_Clicked(object sender, EventArgs e)
	{
		_viewModel.AddTestCookies();
	}
	private void ClearCookies_Clicked(object sender, EventArgs e)
	{
		_viewModel.ClearCookiesForCurrentSource();
	}
	private void TestDocumentTitle_Clicked(object sender, EventArgs e)
	{
		_viewModel.Source = new HtmlWebViewSource
		{
			Html = "<html><head><title>WebView Test Page</title></head><body><h1>Page with Title</h1><p>This page has a title that can be retrieved via JavaScript.</p></body></html>"
		};
	}
	private void LoadPage1_Clicked(object sender, EventArgs e)
	{
		_viewModel.Source = new HtmlWebViewSource
		{
			Html = @"<!DOCTYPE html>
        <html>
        <head>
            <title>Navigation Test - Page 1</title>
            <meta charset='utf-8'>
        </head>
        <body>
            <h1>Navigation Test - Page 1</h1>
            <p>This is the first page for navigation testing.</p>
            <a href='#' onclick='loadPage2()'>Go to Page 2</a>
            <script>
                function loadPage2() {
                    document.body.innerHTML = `
                        <h1>Navigation Test - Page 2</h1>
                        <p>This is the second page content rendered dynamically.</p>
                        <button onclick='loadPage1()'>Go Back</button>
                    `;
                    // Update title when content changes
                    document.title = 'Navigation Test - Page 2 (Dynamic)';
                }
                function loadPage1() {
                    document.body.innerHTML = `
                        <h1>Navigation Test - Page 1</h1>
                        <p>This is the first page for navigation testing.</p>
                        <a href='#' onclick='loadPage2()'>Go to Page 2</a>
                    `;
                    // Update title when content changes
                    document.title = 'Navigation Test - Page 1';
                }
                
                // Helper function for JavaScript evaluation
                window.testEvaluation = function() {
                    return 'JavaScript working on navigation page!';
                };
            </script>
        </body>
        </html>",
		};
	}
	private void LoadPage2_Clicked(object sender, EventArgs e)
	{
		_viewModel.Source = new HtmlWebViewSource
		{
			Html = @"<!DOCTYPE html>
        <html>
        <head>
            <title>Navigation Test - Page 2</title>
            <meta charset='utf-8'>
        </head>
        <body>
            <h1>Navigation Test - Page 2</h1>
            <p>This is the second page for navigation testing.</p>
            <button onclick='history.back()'>Go Back</button>
            <script>
                // Helper function for JavaScript evaluation
                window.testEvaluation = function() {
                    return 'JavaScript working on Page 2!';
                };
            </script>
        </body>
        </html>",
		};
	}
	private void IsVisibleRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (!(sender is RadioButton rb) || !rb.IsChecked)
			return;
		_viewModel.IsVisible = rb.Content?.ToString() == "True";
	}
	private void ShadowRadio_CheckedChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && BindingContext is WebViewViewModel vm)
		{
			var rb = sender as RadioButton;
			if (rb?.Content?.ToString() == "True")
			{
				vm.Shadow = new Shadow { Brush = Brush.Black, Offset = new Point(5, 5), Radius = 5, Opacity = 0.5f };
			}
			else if (rb?.Content?.ToString() == "False")
			{
				vm.Shadow = null!;
			}
		}
	}
}