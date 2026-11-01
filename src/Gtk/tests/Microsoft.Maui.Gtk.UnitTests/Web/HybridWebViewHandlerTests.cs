using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;
using MauiHybridWebView = Microsoft.Maui.Controls.HybridWebView;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>The HybridWebView handler: the placeholder where there is no engine, and the app origin, messaging and .NET invocation on the platform engine (WebKitGTK on Linux, WebView2 on Windows).</summary>
public class HybridWebViewHandlerTests : IDisposable
{
	const string Root = "hybrid_gtk_test";

	public sealed class Target
	{
		public int Add(int a, int b) => a + b;

		public string Greet(string name) => "hello " + name;

		public void Fail() => throw new InvalidOperationException("boom");
	}

	static void WriteRoot()
	{
		var dir = Path.Combine(AppContext.BaseDirectory, Root);
		Directory.CreateDirectory(dir);

		File.WriteAllText(Path.Combine(dir, "index.html"), @"<!DOCTYPE html>
<html><head><title>hybrid</title>
<script src=""_framework/hybridwebview.js""></script>
<script src=""app.js""></script>
</head><body>hybrid</body></html>");

		File.WriteAllText(Path.Combine(dir, "app.js"), @"
window.addEventListener('HybridWebViewMessageReceived', function (e) { HybridWebView.SendRawMessage('echo:' + e.detail.message); });
function multiply(a, b) { return a * b; }
async function failing() { throw new Error('js failure'); }
window.addEventListener('load', async function () {
  try {
    const sum = await HybridWebView.InvokeDotNet('Add', [2, 3]);
    const greeting = await HybridWebView.InvokeDotNet('Greet', ['gtk']);
    HybridWebView.SendRawMessage('ready:' + sum + ':' + greeting);
  } catch (e) {
    HybridWebView.SendRawMessage('error:' + e.message);
  }
});
");
	}

	(MauiHybridWebView View, GtkHybridWebViewHandler Handler, List<string> Messages) Create()
	{
		WriteRoot();

		var messages = new List<string>();
		var view = new MauiHybridWebView { HybridRoot = Root };
		view.SetInvokeJavaScriptTarget(new Target());
		view.RawMessageReceived += (s, e) => messages.Add(e.Message ?? "<null>");

		var handler = WebTestApp.CreateHandler<GtkHybridWebViewHandler>(view);
		if (WebTestApp.Present(handler) is { } window)
			_windows.Add(window);

		return (view, handler, messages);
	}

	readonly List<global::Gtk.Window> _windows = new();

	/// <summary>Retires the windows the tests put web views in (on the GTK thread; windows are never destroyed).</summary>
	public void Dispose()
	{
		if (_windows.Count > 0)
			GtkTestHost.Run(() => _windows.ForEach(GtkTestHost.Retire));
	}

	static void WaitFor(List<string> messages, Func<string, bool> match, string what)
	{
		Assert.True(
			GtkTestHost.PumpUntil(() => messages.Exists(m => match(m)), null, 20000),
			$"no message for {what}; got: [{string.Join(", ", messages)}]");
	}

	[GtkFact]
	public void PlatformViewMatchesTheBackend() => GtkTestHost.Run(() =>
	{
		var (_, handler, _) = Create();

		switch (GtkWebBackend.Current)
		{
			case GtkWebBackendKind.WebKitGtk:
				Assert.IsType<global::WebKit.WebView>(handler.PlatformView);
				Assert.Null(handler.Engine);
				break;

			case GtkWebBackendKind.WebView2:
				Assert.IsType<MauiGtkWebView2Host>(handler.PlatformView);
				Assert.Same(((MauiGtkWebView2Host)handler.PlatformView!).Engine, handler.Engine);
				break;

			default:
				var label = Assert.IsType<GtkLabel>(handler.PlatformView);
				Assert.Contains("HybridWebView is unavailable", label.Text);
				Assert.Null(handler.Engine);
				break;
		}
	});

	[GtkFact]
	public void CommandsReachThePlatformViewWithoutThrowing() => GtkTestHost.Run(() =>
	{
		var (view, _, _) = Create();

		view.SendRawMessage("noop");
		_ = view.EvaluateJavaScriptAsync("1");
	});

	[WebEngineFact]
	public void ThePageLoadsFromTheAppPackageAndCallsDotNet() => GtkTestHost.Run(() =>
	{
		var (_, _, messages) = Create();

		// index.html -> hybridwebview.js + app.js through app://0.0.0.1/, then fetch(__hwvInvokeDotNet) -> Target -> raw message.
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page's InvokeDotNet calls");
		Assert.Contains("ready:5:hello gtk", messages);
	});

	[WebEngineFact]
	public void RawMessagesTravelBothWays() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		view.SendRawMessage("ping \"quoted\" é");

		WaitFor(messages, m => m.StartsWith("echo:", StringComparison.Ordinal), "the echo");
		Assert.Contains("echo:ping \"quoted\" é", messages);
	});

	[WebEngineFact]
	public void EvaluateJavaScriptAsyncReturnsTheResult() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		Assert.Equal("12", GtkTestHost.Await(view.EvaluateJavaScriptAsync("multiply(3, 4)")));
		Assert.Equal("hybrid", GtkTestHost.Await(view.EvaluateJavaScriptAsync("document.title")));
		Assert.Null(GtkTestHost.Await(view.EvaluateJavaScriptAsync("undefined")));
	});

	[WebEngineFact]
	public void MessagesFromOutsideTheAppOriginAreIgnored() => GtkTestHost.Run(() =>
	{
		var (_, handler, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");
		var count = messages.Count;

		var loaded = false;
		if (handler.PlatformView is global::WebKit.WebView view)
		{
			view.LoadChanged += (o, a) => loaded |= a.LoadEvent == global::WebKit.LoadEvent.Finished;
			view.LoadHtml("<script>window.webkit.messageHandlers.webwindowinterop.postMessage('__RawMessage|spoofed')</script>", "https://example.com/");
		}
		else
		{
			var engine = handler.Engine!;
			engine.Navigated += (o, a) => loaded = true;
			engine.LoadHtml("<script>window.chrome.webview.postMessage('__RawMessage|spoofed')</script>", "https://example.com/");
		}

		Assert.True(GtkTestHost.PumpUntil(() => loaded, null, 15000), "the foreign page did not load");
		GtkTestHost.PumpUntil(() => false, null, 500);

		Assert.Equal(count, messages.Count);
	});

	static int FetchStatus(MauiHybridWebView view, string path, string init = "")
	{
		_ = GtkTestHost.Await(view.EvaluateJavaScriptAsync("window.__status = undefined; fetch('" + path + "'" + init + ").then(function (r) { window.__status = r.status; }, function () { window.__status = -1; }); 0"));

		string? status = null;
		Assert.True(
			GtkTestHost.PumpUntil(
				() =>
				{
					status = GtkTestHost.Await(view.EvaluateJavaScriptAsync("window.__status"));
					return status is not null;
				},
				null,
				15000),
			"the fetch of " + path + " did not finish");
		return int.Parse(status!, System.Globalization.CultureInfo.InvariantCulture);
	}

	[WebEngineFact]
	public void AppPackageFilesAreServedAndMissingOnesAreNotFound() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		Assert.Equal(200, FetchStatus(view, "app.js"));
		Assert.Equal(200, FetchStatus(view, "_framework/hybridwebview.js"));
		Assert.Equal(404, FetchStatus(view, "missing.txt"));
	});

	[WebEngineFact]
	public void InvokeDotNetNeedsAPostWithABodyFromTheApp() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		// The token/origin headers (WebView2) or the app page origin (WebKit) are present: the verb and the body are what is wrong.
		Assert.Equal(405, FetchStatus(view, "__hwvInvokeDotNet", ", { headers: { 'X-Maui-Invoke-Token': 'HybridWebView' } }"));
		Assert.Equal(400, FetchStatus(view, "__hwvInvokeDotNet", ", { method: 'POST', headers: { 'X-Maui-Invoke-Token': 'HybridWebView' } }"));
	});

	[WebView2Fact]
	public void InvokeDotNetWithoutTheInvokeTokenIsRejectedOnWebView2() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		Assert.Equal(400, FetchStatus(view, "__hwvInvokeDotNet", ", { method: 'POST', body: '{}' }"));
	});

	[WebEngineFact]
	public void InvokeJavaScriptAsyncCallsThePageAndReturnsTheTypedResult() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		var product = GtkTestHost.Await(view.InvokeJavaScriptAsync(
			"multiply",
			HybridTestJsonContext.Default.Int32,
			new object?[] { 6, 7 },
			new JsonTypeInfo?[] { HybridTestJsonContext.Default.Int32, HybridTestJsonContext.Default.Int32 }));

		Assert.Equal(42, product);
	});

	[WebEngineFact]
	public void RawMessagesKeepSpecialCharactersInBothDirections() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		var text = "a%20b+c&d=e/ü€ \u0000 tail";
		view.SendRawMessage(text);

		WaitFor(messages, m => m.StartsWith("echo:", StringComparison.Ordinal), "the echo");
		Assert.Contains("echo:" + text, messages);
	});

	[WebEngineFact]
	public void DisconnectingStopsTheBridgeAndCommandsBecomeNoOps() => GtkTestHost.Run(() =>
	{
		var (view, handler, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		((IElementHandler)handler).DisconnectHandler();

		Assert.Null(handler.Engine);
		view.SendRawMessage("after disconnect");
		GtkTestHost.PumpUntil(() => false, null, 300);
		Assert.DoesNotContain(messages, m => m.StartsWith("echo:after", StringComparison.Ordinal));
	});
}

[JsonSerializable(typeof(int))]
internal partial class HybridTestJsonContext : JsonSerializerContext
{
}
