using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class WebViewUnitTests : BaseTestFixture
	{
		[Test]
		public void TestSourceImplicitConversion ()
		{
			var web = new WebView ();
			Assert.Null (web.Source);
			web.Source = "http://www.google.com";
			Assert.NotNull (web.Source);
			Assert.True (web.Source is UrlWebViewSource);
			Assert.AreEqual ("http://www.google.com", ((UrlWebViewSource)web.Source).Url);
		}

		[Test]
		public void TestSourceChangedPropagation ()
		{
			var source = new UrlWebViewSource {Url ="http://www.google.com"};
			var web = new WebView { Source = source };
			bool signaled = false;
			web.PropertyChanged += (sender, args) => {
				if (args.PropertyName == WebView.SourceProperty.PropertyName)
					signaled = true;
			};
			Assert.False (signaled);
			source.Url = "http://www.xamarin.com";
			Assert.True (signaled);
		}

		[Test]
		public void TestSourceDisconnected ()
		{
			var source = new UrlWebViewSource {Url="http://www.google.com"};
			var web = new WebView { Source = source };
			web.Source = new UrlWebViewSource {Url="Foo"};
			bool signaled = false;
			web.PropertyChanged += (sender, args) => {
				if (args.PropertyName == WebView.SourceProperty.PropertyName)
					signaled = true;
			};
			Assert.False (signaled);
			source.Url = "http://www.xamarin.com";
			Assert.False (signaled);
		}

		class ViewModel
		{
			public string HTML { get; set; } = "<html><body><p>This is a WebView!</p></body></html>";

			public string URL { get; set; } = "http://xamarin.com";

		}

		[Test]
		public void TestBindingContextPropagatesToSource ()
		{
			var htmlWebView = new WebView {
			};
			var urlWebView = new WebView {
			};

			var htmlSource = new HtmlWebViewSource ();
			htmlSource.SetBinding (HtmlWebViewSource.HtmlProperty, "HTML");
			htmlWebView.Source = htmlSource;

			var urlSource = new UrlWebViewSource ();
			urlSource.SetBinding (UrlWebViewSource.UrlProperty, "URL");
			urlWebView.Source = urlSource;

			var viewModel = new ViewModel ();

			var container = new StackLayout {
				BindingContext = viewModel,
				Padding = new Size (20, 20),
				Children = {
					htmlWebView,
					urlWebView
				}
			};

			Assert.AreEqual ("<html><body><p>This is a WebView!</p></body></html>", htmlSource.Html);
			Assert.AreEqual ("http://xamarin.com", urlSource.Url);
		}

		[Test]
		public void TestWindowsSetAllowJavaScriptAlertsFlag()
		{
			var defaultWebView = new WebView();

			var jsAlertsAllowedWebView = new WebView();
			jsAlertsAllowedWebView.On<Windows>().SetIsJavaScriptAlertEnabled(true);

			Assert.AreEqual(defaultWebView.On<Windows>().IsJavaScriptAlertEnabled(), false);
			Assert.AreEqual(jsAlertsAllowedWebView.On<Windows>().IsJavaScriptAlertEnabled(), true);
		}
	}
}
