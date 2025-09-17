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
				// Wait a bit for the page to load
				await Task.Delay(2000);
				var result = await TestWebView.EvaluateJavaScriptAsync("document.documentElement.innerHTML");
				InnerHTMLResult.Text = result != null ? $"innerHTML length: {result.Length}" : "NULL";
			}
			catch (Exception ex)
			{
				InnerHTMLResult.Text = $"Error: {ex.Message}";
			}
		}
	}
}