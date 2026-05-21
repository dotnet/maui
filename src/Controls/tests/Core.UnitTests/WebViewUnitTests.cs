using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.Maui.Graphics;
using Xunit;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class WebViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestSourceImplicitConversion()
		{
			var web = new WebView();
			Assert.Null(web.Source);
			web.Source = "http://www.google.com";
			Assert.NotNull(web.Source);
			Assert.True(web.Source is UrlWebViewSource);
			Assert.Equal("http://www.google.com", ((UrlWebViewSource)web.Source).Url);
		}

		[Fact]
		public void TestSourceChangedPropagation()
		{
			var source = new UrlWebViewSource { Url = "http://www.google.com" };
			var web = new WebView { Source = source };
			bool signaled = false;
			web.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == WebView.SourceProperty.PropertyName)
					signaled = true;
			};
			Assert.False(signaled);
			source.Url = "http://www.xamarin.com";
			Assert.True(signaled);
		}

		[Fact]
		public void TestSourceDisconnected()
		{
			var source = new UrlWebViewSource { Url = "http://www.google.com" };
			var web = new WebView { Source = source };
			web.Source = new UrlWebViewSource { Url = "Foo" };
			bool signaled = false;
			web.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == WebView.SourceProperty.PropertyName)
					signaled = true;
			};
			Assert.False(signaled);
			source.Url = "http://www.xamarin.com";
			Assert.False(signaled);
		}

		class ViewModel
		{
			public string HTML { get; set; } = "<html><body><p>This is a WebView!</p></body></html>";

			public string URL { get; set; } = "http://xamarin.com";

		}

		[Fact]
		public void TestBindingContextPropagatesToSource()
		{
			var htmlWebView = new WebView
			{
			};
			var urlWebView = new WebView
			{
			};

			var htmlSource = new HtmlWebViewSource();
			htmlSource.SetBinding(HtmlWebViewSource.HtmlProperty, "HTML");
			htmlWebView.Source = htmlSource;

			var urlSource = new UrlWebViewSource();
			urlSource.SetBinding(UrlWebViewSource.UrlProperty, "URL");
			urlWebView.Source = urlSource;

			var viewModel = new ViewModel();

			var container = new StackLayout
			{
				BindingContext = viewModel,
				Padding = new Size(20, 20),
				Children = {
					htmlWebView,
					urlWebView
				}
			};

			Assert.Equal("<html><body><p>This is a WebView!</p></body></html>", htmlSource.Html);
			Assert.Equal("http://xamarin.com", urlSource.Url);
		}

		[Fact]
		public void TestAndroidMixedContent()
		{
			var defaultWebView = new WebView();

			var mixedContentWebView = new WebView();
			mixedContentWebView.On<Android>().SetMixedContentMode(MixedContentHandling.AlwaysAllow);

			Assert.Equal(MixedContentHandling.NeverAllow, defaultWebView.On<Android>().MixedContentMode());
			Assert.Equal(MixedContentHandling.AlwaysAllow, mixedContentWebView.On<Android>().MixedContentMode());
		}

		[Fact]
		public void TestEnableZoomControls()
		{
			var defaultWebView = new WebView();

			var enableZoomControlsWebView = new WebView();
			enableZoomControlsWebView.On<Android>().SetEnableZoomControls(true);

			Assert.False(defaultWebView.On<Android>().ZoomControlsEnabled());
			Assert.True(enableZoomControlsWebView.On<Android>().ZoomControlsEnabled());
		}

		[Fact]
		public void TestEnableJavaScript()
		{
			var defaultWebView = new WebView();

			var enableJavaScriptWebView = new WebView();
			enableJavaScriptWebView.On<Android>().SetJavaScriptEnabled(false);

			Assert.True(defaultWebView.On<Android>().IsJavaScriptEnabled());
			Assert.False(enableJavaScriptWebView.On<Android>().IsJavaScriptEnabled());
		}

		[Fact]
		public void TestDisplayZoomControls()
		{
			var defaultWebView = new WebView();

			var displayZoomControlsWebView = new WebView();
			displayZoomControlsWebView.On<Android>().SetDisplayZoomControls(false);

			Assert.True(defaultWebView.On<Android>().ZoomControlsDisplayed());
			Assert.False(displayZoomControlsWebView.On<Android>().ZoomControlsDisplayed());
		}

		[Fact]
		public void TestWindowsSetAllowJavaScriptAlertsFlag()
		{
			var defaultWebView = new WebView();

			var jsAlertsAllowedWebView = new WebView();
			jsAlertsAllowedWebView.On<WindowsOS>().SetIsJavaScriptAlertEnabled(true);

			Assert.False(defaultWebView.On<WindowsOS>().IsJavaScriptAlertEnabled());
			Assert.True(jsAlertsAllowedWebView.On<WindowsOS>().IsJavaScriptAlertEnabled());
		}

		[Fact]
		public void TestSettingOfCookie()
		{
			var defaultWebView = new WebView();
			var CookieContainer = new CookieContainer();

			CookieContainer.Add(new Cookie("TestCookie", "My Test Cookie...", "/", "microsoft.com"));

			defaultWebView.Cookies = CookieContainer;
			defaultWebView.Source = "http://xamarin.com";

			Assert.NotNull(defaultWebView.Cookies);
		}

		// A shared WebViewSource must not keep WebViews alive after their handler is disconnected.
		[Fact]
		public void SharedHtmlSourceDoesNotPreventWebViewGC()
		{
			var sharedSource = new HtmlWebViewSource { Html = "<html/>" };
			var weakRef = CreateWebViewWithSharedSource(sharedSource);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(weakRef.IsAlive, "WebView should be GC-eligible after handler disconnect even when a shared HtmlWebViewSource is still alive.");
			GC.KeepAlive(sharedSource); // ensure JIT does not elide sharedSource before the GC cycle
		}

		// A shared UrlWebViewSource must not keep WebViews alive after their handler is disconnected.
		[Fact]
		public void SharedUrlSourceDoesNotPreventWebViewGC()
		{
			var sharedSource = new UrlWebViewSource { Url = "https://microsoft.com" };
			var weakRef = CreateWebViewWithSharedSource(sharedSource);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.False(weakRef.IsAlive, "WebView should be GC-eligible after handler disconnect even when a shared UrlWebViewSource is still alive.");
			GC.KeepAlive(sharedSource); // ensure JIT does not elide sharedSource before the GC cycle
		}

		// Multiple WebViews sharing one source must all be GC-eligible after their handlers are disconnected.
		[Fact]
		public void MultipleWebViewsWithSharedSourceAreAllGCEligible()
		{
			var sharedSource = new HtmlWebViewSource { Html = "<html/>" };
			var weakRefs = CreateMultipleWebViewsWithSharedSource(sharedSource);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			for (int i = 0; i < weakRefs.Length; i++)
			{
				Assert.False(weakRefs[i].IsAlive, $"WebView[{i}] should be GC-eligible after handler disconnect even when a shared WebViewSource is still alive.");
			}
			GC.KeepAlive(sharedSource); // ensure JIT does not elide sharedSource before the GC cycle
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateWebViewWithSharedSource(WebViewSource source)
		{
			var webView = new WebView { Source = source };

			// Simulate a full page lifecycle (attach handler, then disconnect on page pop).
			// The retention path under test is created by "Source = source": the propertyChanged
			// callback subscribes WebView.OnSourceChanged to the shared source's SourceChanged
			// event. Attaching and then nulling the handler mirrors what the framework does when
			// a page is pushed and subsequently popped, giving the WebView a realistic lifetime
			// before GC eligibility is asserted.
			webView.Handler = new HandlerStub();
			webView.Handler = null;

			return new WeakReference(webView);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference[] CreateMultipleWebViewsWithSharedSource(WebViewSource source)
		{
			var weakRefs = new WeakReference[5];
			for (int i = 0; i < weakRefs.Length; i++)
			{
				var webView = new WebView { Source = source };

				// Simulate page pop for each WebView — see CreateWebViewWithSharedSource for details.
				webView.Handler = new HandlerStub();
				webView.Handler = null;

				weakRefs[i] = new WeakReference(webView);
			}
			return weakRefs;
		}
	}
}
