using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4891, "[Android] WebView Navigating Cancel property not working with custom scheme", PlatformAffected.Android)]
	public class Issue4891 : TestContentPage // or TestFlyoutPage, etc ...
	{
		Button _back;
		WebView _myWebView;
		Label _log;
		ScrollView _logScrollView;

		protected override void Init()
		{

			_back = new Button
			{
				Text = "Back"
			};
			_back.Clicked += Back_Clicked;

			_myWebView = new WebView
			{
				HorizontalOptions = LayoutOptions.StartAndExpand,
				VerticalOptions = LayoutOptions.Start,
				HeightRequest = 240
			};
			_myWebView.Navigating += MyWebView_Navigating;
			_myWebView.Navigated += MyWebView_Navigated;
			_myWebView.Source = new HtmlWebViewSource()
			{
				Html = "<html><body>Click on the link below. Expected results:<br/>1. Navigating event logged.<br/>2. Navigated event NOT logged.<br/>3. This page stays loaded in the WebView control.<br/><br/><a href='xamforms4223://custom'>Navigate to Custom xamforms4223 scheme</a></body></html>"
			};

			_log = new Label
			{
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Text = ""
			};

			_logScrollView = new ScrollView
			{
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Content = _log
			};

			Content = new StackLayout
			{
				Children =
				{
					_myWebView,
					_back,
					_logScrollView
				}
			};
		}


		void MyWebView_Navigating(object sender, WebNavigatingEventArgs e)
		{
			LogToScreen($"Navigating: {e.Url}");
			if (e.Url.StartsWith("xamforms4223", StringComparison.OrdinalIgnoreCase))
			{
				LogToScreen("Caught custom scheme, cancelling navigation.");
				e.Cancel = true;
			}
		}

		void MyWebView_Navigated(object sender, WebNavigatedEventArgs e)
		{
			LogToScreen($"Navigated: ({e.Result}) {e.Url}");
		}

		void LogToScreen(string text)
		{
			_log.Text += $"{text}\n";

			InvalidateMeasure();
			_logScrollView.ScrollToAsync(_log, ScrollToPosition.End, false);
		}

		void Back_Clicked(object sender, EventArgs e)
		{
			if (_myWebView.CanGoBack)
			{
				_myWebView.GoBack();
			}
		}

	}
}