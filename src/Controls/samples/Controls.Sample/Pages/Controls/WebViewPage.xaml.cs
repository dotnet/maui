using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class WebViewPage
	{
		public WebViewPage()
		{
			InitializeComponent();

			JavaScriptWebView.Source = LoadHTMLFileFromResource();
		}

		void OnGoBackClicked(object sender, EventArgs args)
		{
			Debug.WriteLine($"CanGoBack {MauiWebView.CanGoBack}");

			if (MauiWebView.CanGoBack)
			{
				MauiWebView.GoBack();
			}
		}

		void OnGoForwardClicked(object sender, EventArgs args)
		{
			Debug.WriteLine($"CanGoForward {MauiWebView.CanGoForward}");

			if (MauiWebView.CanGoForward)
			{
				MauiWebView.GoForward();
			}
		}

		void OnReloadClicked(object sender, EventArgs args)
		{
			MauiWebView.Reload();
		}

		void OnEvalClicked(object sender, EventArgs args)
		{
			MauiWebView.Eval("alert('text')");
		}

		HtmlWebViewSource LoadHTMLFileFromResource()
		{
			var source = new HtmlWebViewSource();

			// Load the HTML file embedded as a resource 
			var assembly = typeof(WebViewPage).GetTypeInfo().Assembly;
			var stream = assembly.GetManifestResourceStream("Maui.Controls.Sample.index.html");
			using (var reader = new StreamReader(stream))
			{
				source.Html = reader.ReadToEnd();
			}
			return source;
		}

		async void OnCallJavaScriptButtonClicked(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(NumberEntry.Text))
			{
				return;
			}

			int number = int.Parse(NumberEntry.Text);
			string result = await JavaScriptWebView.EvaluateJavaScriptAsync($"factorial({number})");
			ResultLabel.Text = $"Factorial of {number} is {result}.";
		}
	}
}