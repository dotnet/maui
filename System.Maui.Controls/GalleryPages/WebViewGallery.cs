using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class WebViewGallery : ContentPage
	{
		class ViewModel
		{
			public string Html { get; set; } = "<html><body><p>This is a WebView!</p></body></html>";

			public string Url { get; set; } = "http://xamarin.com";

		}

		public WebViewGallery ()
		{
			var htmlWebView = new WebView {
				HeightRequest = 40,
				//Source = new HtmlWebViewSource {Html ="<html><body><p>This is a WebView!</p></body></html>"}
			};
			var urlWebView = new WebView {
				VerticalOptions = LayoutOptions.FillAndExpand,
				//Source = new UrlWebViewSource {Url = "http://xamarin.com/"}
			};

			var htmlSource = new HtmlWebViewSource ();
			htmlSource.SetBinding (HtmlWebViewSource.HtmlProperty, "HTML");
			htmlWebView.Source = htmlSource;

			var urlSource = new UrlWebViewSource ();
			urlSource.SetBinding (UrlWebViewSource.UrlProperty, "URL");
			urlWebView.Source = urlSource;

			var viewModel = new ViewModel ();
			BindingContext = viewModel;

			Content = new StackLayout {
				Padding = new Size (20, 20),
				Children = {
					htmlWebView,
					urlWebView
				}
			};
		}
	}
}
