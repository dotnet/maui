using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;
using MauiWebView = Microsoft.Maui.Controls.WebView;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>The WebView handler: the placeholder where there is no WebKitGTK, and a real web view where there is.</summary>
public class WebViewHandlerTests
{
	const string PageOne = "data:text/html,<title>one</title><p>one</p>";
	const string PageTwo = "data:text/html,<title>two</title><p>two</p>";

	static (MauiWebView View, GtkWebViewHandler Handler) Create(WebViewSource? source = null, string? userAgent = null)
	{
		var view = new MauiWebView();
		if (userAgent is not null)
			view.UserAgent = userAgent;
		if (source is not null)
			view.Source = source;

		return (view, WebTestApp.CreateHandler<GtkWebViewHandler>(view));
	}

	static void WaitForNavigated(List<WebNavigatedEventArgs> navigated, int count = 1) =>
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count >= count, null, 15000), $"expected {count} Navigated event(s), got {navigated.Count}");

	[GtkFact]
	public void PlatformViewMatchesWebKitAvailability() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create(new UrlWebViewSource { Url = PageOne });

		if (GtkWebKit.IsAvailable)
		{
			Assert.IsType<global::WebKit.WebView>(handler.PlatformView);
		}
		else
		{
			var label = Assert.IsType<GtkLabel>(handler.PlatformView);
			Assert.Contains("WebView is unavailable", label.Text);
			Assert.Contains(GtkWebKit.UnavailableReason!, label.Text);
		}
	});

	[GtkFact]
	public void WebViewKeepsWorkingWithoutWebKit() => GtkTestHost.Run(() =>
	{
		// Every command and property reaches a placeholder (or a real view) without throwing.
		var (view, handler) = Create(new HtmlWebViewSource { Html = "<p>x</p>" }, userAgent: "agent");

		view.GoBack();
		view.GoForward();
		view.Reload();
		view.Eval("1+1");
		view.Source = new UrlWebViewSource { Url = PageTwo };

		Assert.NotNull(handler.PlatformView);
	});

	[WebKitFact]
	public void HtmlSourceLoadsAndRaisesNavigated() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new HtmlWebViewSource { Html = "<html><head><title>hello</title></head><body>hi</body></html>" };
		WaitForNavigated(navigated);

		Assert.Equal(WebNavigationResult.Success, navigated[0].Result);
		Assert.Equal(WebNavigationEvent.NewPage, navigated[0].NavigationEvent);
		Assert.Equal("hello", ((global::WebKit.WebView)handler.PlatformView!).Title);
	});

	[WebKitFact]
	public void UrlSourceLoadsTheUrl() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.Equal(WebNavigationResult.Success, navigated[0].Result);
		Assert.StartsWith("data:text/html", navigated[0].Url);
		Assert.Equal("one", ((global::WebKit.WebView)handler.PlatformView!).Title);
	});

	[WebKitFact]
	public void NavigatingCanCancelTheNavigation() => GtkTestHost.Run(() =>
	{
		var navigating = new List<WebNavigatingEventArgs>();
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigating += (s, e) =>
		{
			navigating.Add(e);
			e.Cancel = true;
		};
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new UrlWebViewSource { Url = PageOne };
		Assert.True(GtkTestHost.PumpUntil(() => navigating.Count > 0, null, 15000), "Navigating was not raised");
		GtkTestHost.PumpUntil(() => false, null, 500);

		Assert.Empty(navigated);
		Assert.NotEqual(PageOne, ((global::WebKit.WebView)handler.PlatformView!).Uri);
	});

	[WebKitFact]
	public void LinksClickedInThePageRaiseNavigatingAndCanBeCancelled() => GtkTestHost.Run(() =>
	{
		var navigating = new List<WebNavigatingEventArgs>();
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new HtmlWebViewSource { Html = "<a id='a' href='https://example.invalid/next'>go</a>" };
		WaitForNavigated(navigated, 1);

		view.Navigating += (s, e) =>
		{
			navigating.Add(e);
			e.Cancel = true;
		};
		view.Eval("document.getElementById('a').click()");

		Assert.True(GtkTestHost.PumpUntil(() => navigating.Count > 0, null, 15000), "Navigating was not raised for the link");
		Assert.StartsWith("https://example.invalid/next", navigating[0].Url);
		Assert.Equal(WebNavigationEvent.NewPage, navigating[0].NavigationEvent);
		GtkTestHost.PumpUntil(() => false, null, 500);
		Assert.Single(navigated);
	});

	[WebKitFact]
	public void GoBackAndForwardNavigateTheHistory() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated, 1);
		view.Source = new UrlWebViewSource { Url = PageTwo };
		WaitForNavigated(navigated, 2);
		Assert.True(GtkTestHost.PumpUntil(() => view.CanGoBack, null, 5000), "CanGoBack never became true");
		Assert.False(view.CanGoForward);

		view.GoBack();
		WaitForNavigated(navigated, 3);
		Assert.Equal(WebNavigationEvent.Back, navigated[2].NavigationEvent);
		Assert.Equal("one", ((global::WebKit.WebView)handler.PlatformView!).Title);
		Assert.True(GtkTestHost.PumpUntil(() => view.CanGoForward, null, 5000), "CanGoForward never became true");

		view.GoForward();
		WaitForNavigated(navigated, 4);
		Assert.Equal(WebNavigationEvent.Forward, navigated[3].NavigationEvent);
		Assert.Equal("two", ((global::WebKit.WebView)handler.PlatformView!).Title);
	});

	[WebKitFact]
	public void ReloadReportsARefresh() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated, 1);

		view.Reload();
		WaitForNavigated(navigated, 2);

		Assert.Equal(WebNavigationEvent.Refresh, navigated[1].NavigationEvent);
	});

	[WebKitFact]
	public void EvaluateJavaScriptReturnsStringsAndJson() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.Equal("3", GtkTestHost.Await(view.EvaluateJavaScriptAsync("1 + 2")));
		Assert.Equal("hello", GtkTestHost.Await(view.EvaluateJavaScriptAsync("'hel' + 'lo'")));
		Assert.Equal("one", GtkTestHost.Await(view.EvaluateJavaScriptAsync("document.title")));
		Assert.Null(GtkTestHost.Await(view.EvaluateJavaScriptAsync("undefined")));
		Assert.Null(GtkTestHost.Await(view.EvaluateJavaScriptAsync("throw new Error('x')")));
	});

	[WebKitFact]
	public void EvalRunsAScript() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		view.Eval("document.title = 'evaluated'");

		Assert.True(GtkTestHost.PumpUntil(() => GtkTestHost.Await(view.EvaluateJavaScriptAsync("document.title")) == "evaluated", null, 5000));
	});

	[WebKitFact]
	public void UserAgentIsAppliedAndReadBack() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create(userAgent: "MauiGtkTest/1.0");
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.Equal("MauiGtkTest/1.0", ((global::WebKit.WebView)handler.PlatformView!).Settings.UserAgent);
		Assert.Equal("MauiGtkTest/1.0", GtkTestHost.Await(view.EvaluateJavaScriptAsync("navigator.userAgent")));
	});

	[WebKitFact]
	public void DefaultUserAgentIsReportedToTheVirtualView() => GtkTestHost.Run(() =>
	{
		var (view, _) = Create(new UrlWebViewSource { Url = PageOne });

		Assert.False(string.IsNullOrEmpty(view.UserAgent));
	});

	[WebKitFact]
	public void DisconnectingStopsRaisingEvents() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		var native = (global::WebKit.WebView)handler.PlatformView!;
		((global::Microsoft.Maui.IElement)view).Handler!.DisconnectHandler();
		native.LoadUri(PageOne);
		GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 1500);

		Assert.Empty(navigated);
	});
}
