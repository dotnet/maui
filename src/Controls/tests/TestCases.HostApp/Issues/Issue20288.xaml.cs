using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20288, "Evaluating javascript in MAUI WebView on IOS returns NULL", PlatformAffected.iOS)]
	public partial class Issue20288 : ContentPage
	{
		public Issue20288()
		{
			InitializeComponent();
			
			// Load a simple HTML page that's guaranteed to work
			var htmlSource = new HtmlWebViewSource
			{
				Html = @"<html>
<head><title>Test Page</title></head>
<body>
<h1>Test WebView Page</h1>
<p>This is a test page for JavaScript evaluation.</p>
<div id='test'>Test content</div>
</body>
</html>"
			};
			TestWebView.Source = htmlSource;
		}

		private async void OnTestStringClicked(object sender, EventArgs e)
		{
			try
			{
				var result = await TestWebView.EvaluateJavaScriptAsync("'Hello World'");
				StringResult.Text = $"String: '{result ?? "NULL"}'";
			}
			catch (Exception ex)
			{
				StringResult.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnTestNumberClicked(object sender, EventArgs e)
		{
			try
			{
				var result = await TestWebView.EvaluateJavaScriptAsync("42");
				NumberResult.Text = $"Number: '{result ?? "NULL"}'";
			}
			catch (Exception ex)
			{
				NumberResult.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnTestBooleanClicked(object sender, EventArgs e)
		{
			try
			{
				var result = await TestWebView.EvaluateJavaScriptAsync("true");
				BooleanResult.Text = $"Boolean: '{result ?? "NULL"}'";
			}
			catch (Exception ex)
			{
				BooleanResult.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnTestNullClicked(object sender, EventArgs e)
		{
			try
			{
				var result = await TestWebView.EvaluateJavaScriptAsync("null");
				NullResult.Text = $"Null: '{result ?? "NULL"}'";
			}
			catch (Exception ex)
			{
				NullResult.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnTestObjectClicked(object sender, EventArgs e)
		{
			try
			{
				var result = await TestWebView.EvaluateJavaScriptAsync("({name: 'test', value: 123})");
				ObjectResult.Text = $"Object: '{result ?? "NULL"}'";
			}
			catch (Exception ex)
			{
				ObjectResult.Text = $"Error: {ex.Message}";
			}
		}

		private async void OnTestInnerHTMLClicked(object sender, EventArgs e)
		{
			try
			{
				InnerHTMLResult.Text = "Loading...";
				
				// Wait longer for the page to load and check if it loaded
				await Task.Delay(5000);
				
				// First check if document exists
				var docCheck = await TestWebView.EvaluateJavaScriptAsync("typeof document !== 'undefined'");
				if (docCheck != "true")
				{
					InnerHTMLResult.Text = "Document not available";
					return;
				}
				
				// Try to get innerHTML
				var result = await TestWebView.EvaluateJavaScriptAsync("document.documentElement.innerHTML");
				if (result != null)
				{
					InnerHTMLResult.Text = $"innerHTML length: {result.Length}";
				}
				else
				{
					InnerHTMLResult.Text = "NULL";
				}
			}
			catch (Exception ex)
			{
				InnerHTMLResult.Text = $"Error: {ex.Message}";
			}
		}
	}
}