using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;
using MauiWebView = Microsoft.Maui.Controls.WebView;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>
/// The WebView2 engine and host against the real WebView2 Runtime (Windows only; skipped where it is not installed): the parts of the
/// backend that the engine-neutral <see cref="WebViewHandlerTests"/> do not reach.
/// </summary>
public class WebView2EngineTests : IDisposable
{
	const string PageOne = "data:text/html,<title>one</title><p>one</p>";

	readonly List<global::Gtk.Window> _windows = new();

	public void Dispose()
	{
		if (_windows.Count > 0)
			GtkTestHost.Run(() => _windows.ForEach(GtkTestHost.Retire));
	}

	delegate bool EnumChildProc(IntPtr hwnd, IntPtr lParam);

	[DllImport("user32.dll")]
	static extern bool EnumChildWindows(IntPtr parent, EnumChildProc callback, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	static extern int GetClassName(IntPtr hwnd, StringBuilder name, int count);

	[DllImport("user32.dll")]
	static extern bool PostMessage(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

	[DllImport("user32.dll")]
	static extern bool IsChild(IntPtr parent, IntPtr child);

	static (MauiWebView View, GtkWebViewHandler Handler, MauiGtkWebView2Host Host) Create(WebViewSource? source = null)
	{
		var view = new MauiWebView();
		if (source is not null)
			view.Source = source;

		var handler = WebTestApp.CreateHandler<GtkWebViewHandler>(view);
		return (view, handler, (MauiGtkWebView2Host)handler.PlatformView!);
	}

	global::Gtk.Window Show(MauiGtkWebView2Host host)
	{
		var window = new global::Gtk.Window { Decorated = false, DefaultWidth = 640, DefaultHeight = 480, Child = host };
		_windows.Add(window);
		window.Present();
		GtkTestHost.Resize(window, 640, 480);
		Assert.True(GtkTestHost.PumpUntil(() => host.Engine.Core is not null, window, 20000), "the WebView2 controller was not created");
		return window;
	}

	[WebView2Fact]
	public void OperationsIssuedBeforeTheHostIsInAWindowRunWhenTheControllerIsReady() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler, host) = Create(new UrlWebViewSource { Url = PageOne });
		view.Navigated += (s, e) => navigated.Add(e);
		var title = handler.Engine!;

		// Not realized yet: nothing is loaded, but the request is kept.
		GtkTestHost.Pump(null, 3);
		Assert.Empty(navigated);
		Assert.Null(host.Controller);

		var evaluation = view.EvaluateJavaScriptAsync("1 + 1");
		Show(host);

		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 15000));
		Assert.Equal(WebNavigationResult.Success, navigated[0].Result);
		Assert.Equal("one", title.Title);
		Assert.Equal("2", GtkTestHost.Await(evaluation));
	});

	[WebView2Fact]
	public void TheControllerSitsOnTheWidgetAllocationAndFollowsVisibility() => GtkTestHost.Run(() =>
	{
		var (_, _, host) = Create();
		var window = Show(host);

		var controller = host.Controller!.Controller!;
		var expected = host.ComputeNativeBounds()!.Value;
		Assert.Equal(new System.Drawing.Rectangle(expected.X, expected.Y, expected.Width, expected.Height), controller.Bounds);
		Assert.True(expected.Width > 0 && expected.Height > 0);
		Assert.True(controller.IsVisible);

		host.Visible = false;
		GtkTestHost.Pump(window);
		Assert.False(controller.IsVisible);

		host.Visible = true;
		GtkTestHost.Pump(window);
		Assert.True(controller.IsVisible);

		GtkTestHost.Resize(window, 800, 520);
		GtkTestHost.Pump(window);
		var after = host.ComputeNativeBounds()!.Value;
		Assert.NotEqual(expected, after);
		Assert.Equal(new System.Drawing.Rectangle(after.X, after.Y, after.Width, after.Height), controller.Bounds);
	});

	[WebView2Fact]
	public void TheWebViewIsAChildWindowOfTheToplevelAndReceivesInput() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _, host) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		Show(host);

		view.Source = new HtmlWebViewSource { Html = "<body style='margin:0'><button style='width:100vw;height:100vh' onclick=\"document.title='clicked'\">x</button></body>" };
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 15000));

		var top = host.ToplevelHandle;
		Assert.NotEqual(IntPtr.Zero, top);

		IntPtr render = IntPtr.Zero;
		EnumChildWindows(top, (hwnd, l) =>
		{
			var name = new StringBuilder(256);
			GetClassName(hwnd, name, 256);
			if (name.ToString() == "Chrome_RenderWidgetHostHWND")
			{
				render = hwnd;
				return false;
			}

			return true;
		}, IntPtr.Zero);
		Assert.NotEqual(IntPtr.Zero, render);
		Assert.True(IsChild(top, render));

		// Window messages to the render widget: the path real mouse input takes (a real click would depend on what is on top of the window).
		var lParam = (IntPtr)((20 << 16) | 20);
		PostMessage(render, 0x0200, IntPtr.Zero, lParam);
		PostMessage(render, 0x0201, (IntPtr)1, lParam);
		PostMessage(render, 0x0202, IntPtr.Zero, lParam);

		Assert.True(GtkTestHost.PumpUntil(() => host.CoreWebView2!.DocumentTitle == "clicked", null, 5000), "the click did not reach the page");
	});

	[WebView2Fact]
	public void ARealizedAgainHostShowsTheAddressTheClosedControllerShowed() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler, host) = Create(new UrlWebViewSource { Url = PageOne });
		view.Navigated += (s, e) => navigated.Add(e);
		var window = Show(host);
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count == 1, null, 15000));
		var first = host.CoreWebView2;

		window.Child = null;
		GtkTestHost.Pump(window);
		Assert.Null(host.Controller);
		Assert.Null(handler.Engine!.Uri);

		window.Child = host;
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count == 2, window, 15000), "the page was not shown again");

		Assert.NotSame(first, host.CoreWebView2);
		Assert.Equal("one", handler.Engine.Title);
		Assert.Equal(WebNavigationResult.Success, navigated[1].Result);
	});

	[WebView2Fact]
	public void TheVirtualViewCookiesAreSentForANavigationIssuedBeforeTheControllerExists() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler, host) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Cookies = new CookieContainer();
		view.Cookies.Add(new Uri("https://early.maui-gtk.test/"), new Cookie("sid", "7", "/", "early.maui-gtk.test"));

		// Not realized yet: the engine has no cookie store when the navigation is issued.
		view.Source = new UrlWebViewSource { Url = "https://early.maui-gtk.test/page" };
		Assert.Null(handler.Engine!.Cookies);
		Show(host);
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 20000));

		var cookies = GtkTestHost.Await(handler.Engine.Cookies!.GetAsync(new Uri("https://early.maui-gtk.test/")));
		Assert.Contains(cookies.Cast<Cookie>(), c => c.Name == "sid" && c.Value == "7");
	});

	[WebView2Fact]
	public void AScriptIssuedWhileTheHostIsUnrealizedDoesNotPreventShowingTheClosedAddressAgain() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler, host) = Create(new UrlWebViewSource { Url = PageOne });
		view.Navigated += (s, e) => navigated.Add(e);
		var window = Show(host);
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count == 1, null, 15000));

		window.Child = null;
		GtkTestHost.Pump(window);
		Assert.Null(host.Controller);

		view.Eval("document.title = 'scripted'");
		var result = view.EvaluateJavaScriptAsync("1 + 1");

		window.Child = host;
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count == 2, window, 15000), "the page was not shown again");
		Assert.Equal("2", GtkTestHost.Await(result));
	});

	[WebView2Fact]
	public void TheHostRaisesCoreWebView2InitializedBeforeTheFirstNavigation() => GtkTestHost.Run(() =>
	{
		var events = new List<string>();
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _, host) = Create(new UrlWebViewSource { Url = PageOne });
		view.Navigated += (s, e) => navigated.Add(e);
		host.CoreWebView2Initialized += (s, core) =>
		{
			events.Add("initialized");
			core.NavigationStarting += (_, __) => events.Add("starting");
		};

		Show(host);
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 15000));

		Assert.Equal("initialized", events[0]);
		Assert.Contains("starting", events);
	});

	[WebView2Fact]
	public void DisconnectingTheHandlerClosesTheControllerAndDetachesTheEngine() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler, host) = Create(new UrlWebViewSource { Url = PageOne });
		view.Navigated += (s, e) => navigated.Add(e);
		Show(host);

		((IElement)view).Handler!.DisconnectHandler();

		Assert.Null(handler.Engine);
		Assert.Null(host.Controller);
		Assert.Null(host.CoreWebView2);
		GtkTestHost.Pump(null, 5);
	});

	[WebView2Fact]
	public void CookiesAreWrittenAndReadThroughTheBrowserStore() => GtkTestHost.Run(() =>
	{
		var (_, handler, host) = Create();
		Show(host);
		var store = handler.Engine!.Cookies!;
		var uri = new Uri("https://cookies.maui-gtk.test/");

		store.Set(new CookieCollection { new Cookie("session", "abc", "/", "cookies.maui-gtk.test") });
		var read = GtkTestHost.Await(store.GetAsync(uri));

		Assert.Contains(read.Cast<Cookie>(), c => c.Name == "session" && c.Value == "abc");

		var expired = new Cookie("session", "abc", "/", "cookies.maui-gtk.test") { Expired = true };
		store.Set(new CookieCollection { expired });
		// The browser applies the deletion asynchronously.
		Assert.True(GtkTestHost.PumpUntil(() => !GtkTestHost.Await(store.GetAsync(uri)).Cast<Cookie>().Any(c => c.Name == "session"), null, 5000), "the cookie was not deleted");
	});

	[WebView2Fact]
	public void TheVirtualViewCookiesAreSentToTheBrowserBeforeANavigation() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, handler, host) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		view.Cookies = new CookieContainer();
		view.Cookies.Add(new Uri("https://sync.maui-gtk.test/"), new Cookie("sid", "42", "/", "sync.maui-gtk.test"));
		Show(host);

		// The navigation itself fails (no such host); the cookies are written when it starts.
		view.Source = new UrlWebViewSource { Url = "https://sync.maui-gtk.test/page" };
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 20000));

		var cookies = GtkTestHost.Await(handler.Engine!.Cookies!.GetAsync(new Uri("https://sync.maui-gtk.test/")));
		Assert.Contains(cookies.Cast<Cookie>(), c => c.Name == "sid" && c.Value == "42");
	});

	[WebView2Fact]
	public void ReloadBeforeTheControllerExistsRaisesNothingAndLeavesNoStaleRefresh() => GtkTestHost.Run(() =>
	{
		var navigating = new List<WebNavigationEvent>();
		var navigated = new List<WebNavigatedEventArgs>();
		var (view, _, host) = Create(new HtmlWebViewSource { Html = "<p>first</p>" });
		view.Navigating += (s, e) => navigating.Add(e.NavigationEvent);
		view.Navigated += (s, e) => navigated.Add(e);

		view.Reload();
		Assert.Empty(navigating);

		Show(host);
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count > 0, null, 20000));
		Assert.DoesNotContain(WebNavigationEvent.Refresh, navigating);
		Assert.All(navigated, e => Assert.NotEqual(WebNavigationEvent.Refresh, e.NavigationEvent));
	});

	[WebView2Fact]
	public void ALinkThatOpensANewWindowNavigatesTheSameViewAndCanBeCancelled() => GtkTestHost.Run(() =>
	{
		var navigated = new List<WebNavigatedEventArgs>();
		var navigating = new List<string>();
		var (view, handler, host) = Create();
		view.Navigated += (s, e) => navigated.Add(e);
		Show(host);
		view.Source = new HtmlWebViewSource { Html = "<a id='a' target='_blank' href='data:text/html,<title>other</title>'>go</a>" };
		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count == 1, null, 15000));

		view.Navigating += (s, e) => navigating.Add(e.Url);
		view.Eval("document.getElementById('a').click()");

		Assert.True(GtkTestHost.PumpUntil(() => navigated.Count == 2, null, 15000), "the new-window link was not loaded in the same view");
		Assert.Single(navigating);
		Assert.StartsWith("data:text/html", navigating[0]);
		Assert.Equal("other", handler.Engine!.Title);
	});

	[WindowsGtkFact]
	public void AToplevelMoveIsReportedThroughTheLocationWatch() => GtkTestHost.Run(() =>
	{
		var window = new global::Gtk.Window { DefaultWidth = 300, DefaultHeight = 200 };
		_windows.Add(window);
		window.Present();
		GtkTestHost.Pump(window);

		var hwnd = GtkWebView2HostServices.Default.GetToplevelHandle(window);
		Assert.NotEqual(IntPtr.Zero, hwnd);

		var moves = 0;
		using var watch = GtkWebView2HostServices.Default.WatchLocation(hwnd, () => moves++);
		SetWindowPos(hwnd, IntPtr.Zero, 150, 160, 0, 0, 0x0001 | 0x0004 | 0x0010);

		Assert.True(GtkTestHost.PumpUntil(() => moves > 0, window, 5000), "no location change was reported");
	});

	[WindowsGtkFact]
	public void WithoutTheRuntimeTheHandlerCreatesAPlaceholderThatNamesTheDownload() => GtkTestHost.Run(() =>
	{
		GtkWebView2Environment.ProbeOverride = () => null;
		GtkWebView2Environment.ResetProbe();
		try
		{
			Assert.Equal(GtkWebBackendKind.None, GtkWebBackend.Current);

			var view = new MauiWebView { Source = new UrlWebViewSource { Url = PageOne } };
			var handler = WebTestApp.CreateHandler<GtkWebViewHandler>(view);

			var label = Assert.IsType<GtkLabel>(handler.PlatformView);
			Assert.Contains("WebView is unavailable", label.Text);
			Assert.Contains("WebView2 Runtime not installed", label.Text);
			Assert.Contains("https://go.microsoft.com/fwlink/p/?LinkId=2124703", label.Text);
			Assert.Null(handler.Engine);

			// A placeholder still takes every command without throwing.
			view.GoBack();
			view.Reload();
			view.Eval("1");
			view.Source = new UrlWebViewSource { Url = PageOne };
		}
		finally
		{
			GtkWebView2Environment.ProbeOverride = null;
			GtkWebView2Environment.ResetProbe();
		}
	});
}
