using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;
using MauiWebView = Microsoft.Maui.Controls.WebView;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>The WebView handler: the placeholder where there is no engine, and a real web view (WebKitGTK or WebView2) where there is.</summary>
public class WebViewHandlerTests : IDisposable
{
	readonly List<global::Gtk.Window> _windows = new();

	/// <summary>Retires the windows the tests put web views in (on the GTK thread; windows are never destroyed).</summary>
	public void Dispose()
	{
		if (_windows.Count > 0)
			GtkTestHost.Run(() => _windows.ForEach(GtkTestHost.Retire));
	}

	const string PageOne = "data:text/html,<title>one</title><p>one</p>";
	const string PageTwo = "data:text/html,<title>two</title><p>two</p>";

	(MauiWebView View, GtkWebViewHandler Handler) Create(WebViewSource? source = null, string? userAgent = null)
	{
		var view = new MauiWebView();
		if (userAgent is not null)
			view.UserAgent = userAgent;
		if (source is not null)
			view.Source = source;

		var handler = WebTestApp.CreateHandler<GtkWebViewHandler>(view);
		if (WebTestApp.Present(handler) is { } window)
			_windows.Add(window);

		return (view, handler);
	}

	static void WaitForNavigated(List<WebNavigatedEventArgs> navigated, int count = 1) =>
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count >= count, null, 15000), $"expected {count} Navigated event(s), got {navigated.Count}");

	[GtkFact]
	public void PlatformViewMatchesTheBackend() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create(new UrlWebViewSource { Url = PageOne });

		switch (GtkWebBackend.Current)
		{
			case GtkWebBackendKind.WebKitGtk:
				Assert.IsType<global::WebKit.WebView>(handler.PlatformView);
				Assert.IsType<GtkWebKitEngine>(handler.Engine);
				break;

			case GtkWebBackendKind.WebView2:
				Assert.IsType<MauiGtkWebView2Host>(handler.PlatformView);
				Assert.Same(((MauiGtkWebView2Host)handler.PlatformView!).Engine, handler.Engine);
				break;

			default:
				var label = Assert.IsType<GtkLabel>(handler.PlatformView);
				Assert.Contains("WebView is unavailable", label.Text);
				Assert.Contains(GtkWebBackend.UnavailableReason!, label.Text);
				Assert.Null(handler.Engine);
				break;
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

	[WebEngineFact]
	public void HtmlSourceLoadsAndRaisesNavigated() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new HtmlWebViewSource { Html = "<html><head><title>hello</title></head><body>hi</body></html>" };
		WaitForNavigated(navigated);

		Assert.Equal(WebNavigationResult.Success, navigated[0].Result);
		Assert.Equal(WebNavigationEvent.NewPage, navigated[0].NavigationEvent);
		Assert.Equal("hello", handler.Engine!.Title);
	});

	[WebEngineFact]
	public void UrlSourceLoadsTheUrl() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.Equal(WebNavigationResult.Success, navigated[0].Result);
		Assert.StartsWith("data:text/html", navigated[0].Url);
		Assert.Equal("one", handler.Engine!.Title);
	});

	[WebEngineFact]
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
		Assert.NotEqual(PageOne, handler.Engine!.Uri);
	});

	[WebEngineFact]
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

	[WebEngineFact]
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
		Assert.Equal("one", handler.Engine!.Title);
		Assert.True(GtkTestHost.PumpUntil(() => view.CanGoForward, null, 5000), "CanGoForward never became true");

		view.GoForward();
		WaitForNavigated(navigated, 4);
		Assert.Equal(WebNavigationEvent.Forward, navigated[3].NavigationEvent);
		Assert.Equal("two", handler.Engine!.Title);
	});

	[WebEngineFact]
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

	[WebEngineFact]
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

	[WebEngineFact]
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

	[WebEngineFact]
	public void UserAgentIsAppliedAndReadBack() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create(userAgent: "MauiGtkTest/1.0");
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.Equal("MauiGtkTest/1.0", handler.Engine!.UserAgent);
		Assert.Equal("MauiGtkTest/1.0", GtkTestHost.Await(view.EvaluateJavaScriptAsync("navigator.userAgent")));
	});

	[WebEngineFact]
	public void DefaultUserAgentIsReportedToTheVirtualView() => GtkTestHost.Run(() =>
	{
		var (view, _) = Create(new UrlWebViewSource { Url = PageOne });

		Assert.False(string.IsNullOrEmpty(view.UserAgent));
	});

	[WebEngineFact]
	public void DisconnectingStopsRaisingEvents() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		var platformView = handler.PlatformView!;
		((global::Microsoft.Maui.IElement)view).Handler!.DisconnectHandler();
		Assert.Null(handler.Engine);

		if (platformView is global::WebKit.WebView native)
			native.LoadUri(PageOne);
		else
			Assert.Null(((MauiGtkWebView2Host)platformView).Controller);

		GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 1500);

		Assert.Empty(navigated);
	});

	[WebEngineFact]
	public void NavigatingIsRaisedBeforeNavigatedAndCarriesTheUrl() => GtkTestHost.Run(() =>
	{
		var order = new List<string>();
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigating += (s, e) => order.Add("navigating:" + e.Url);
		view.Navigated += (s, e) =>
		{
			order.Add("navigated");
			navigated.Add(e);
		};

		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		// WebKitGTK also reports the app's own load through its navigation policy, so Navigating may repeat; it never follows Navigated.
		Assert.Equal("navigating:" + PageOne, order[0]);
		Assert.Equal("navigated", order[^1]);
		Assert.Single(order, o => o == "navigated");
	});

	[WebEngineFact]
	public void ASourceSetTwiceNavigatesToTheLastOne() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler) = Create();
		view.Navigated += (s, e) => navigated.Add(e);

		view.Source = new UrlWebViewSource { Url = PageOne };
		view.Source = new UrlWebViewSource { Url = PageTwo };
		WaitForNavigated(navigated);
		GtkTestHost.PumpUntil(() => handler.Engine!.Title == "two", null, 5000);

		Assert.Equal("two", handler.Engine!.Title);
	});

	[WebEngineFact]
	public void EvaluateJavaScriptReturnsNumbersBooleansObjectsAndArraysAsJson() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.Equal("true", GtkTestHost.Await(view.EvaluateJavaScriptAsync("1 < 2")));
		Assert.Equal("1.5", GtkTestHost.Await(view.EvaluateJavaScriptAsync("1.5")));
		Assert.Equal("[1,\"a\"]", GtkTestHost.Await(view.EvaluateJavaScriptAsync("[1, 'a']")));
		Assert.Equal("{\"k\":2}", GtkTestHost.Await(view.EvaluateJavaScriptAsync("({k: 2})")));
		// MAUI wraps scripts in JSON.stringify(eval(...)) and only trims the outer quotes: a string result keeps its JSON escapes.
		Assert.Equal("a \\\"b\\\" c", GtkTestHost.Await(view.EvaluateJavaScriptAsync("'a \"b\" c'")));
		Assert.Null(GtkTestHost.Await(view.EvaluateJavaScriptAsync("null")));
	});

	[WebEngineFact]
	public void ScriptsRunInTheOrderTheyWereIssued() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		view.Eval("window.order = []; window.order.push(1)");
		view.Eval("window.order.push(2)");
		view.Eval("window.order.push(3)");

		Assert.Equal("[1,2,3]", GtkTestHost.Await(view.EvaluateJavaScriptAsync("window.order")));
	});

	[WebEngineFact]
	public void TheBackAndForwardFlagsAreFalseOnAFreshView() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Source = new UrlWebViewSource { Url = PageOne };
		WaitForNavigated(navigated);

		Assert.False(view.CanGoBack);
		Assert.False(view.CanGoForward);

		// Nothing to go back to: no navigation, no event.
		view.GoBack();
		view.GoForward();
		GtkTestHost.PumpUntil(() => false, null, 300);
		Assert.Single(navigated);
	});
}
