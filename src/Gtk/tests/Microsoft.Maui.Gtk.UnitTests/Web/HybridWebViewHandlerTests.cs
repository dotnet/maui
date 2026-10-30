using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;
using MauiHybridWebView = Microsoft.Maui.Controls.HybridWebView;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>The HybridWebView handler: the placeholder where there is no WebKitGTK, and the app origin, messaging and .NET invocation where there is.</summary>
public class HybridWebViewHandlerTests
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

	static (MauiHybridWebView View, GtkHybridWebViewHandler Handler, List<string> Messages) Create()
	{
		WriteRoot();

		var messages = new List<string>();
		var view = new MauiHybridWebView { HybridRoot = Root };
		view.SetInvokeJavaScriptTarget(new Target());
		view.RawMessageReceived += (s, e) => messages.Add(e.Message ?? "<null>");

		return (view, WebTestApp.CreateHandler<GtkHybridWebViewHandler>(view), messages);
	}

	static void WaitFor(List<string> messages, Func<string, bool> match, string what)
	{
		Assert.True(
			GtkTestHost.PumpUntil(() => messages.Exists(m => match(m)), null, 20000),
			$"no message for {what}; got: [{string.Join(", ", messages)}]");
	}

	[GtkFact]
	public void PlatformViewMatchesWebKitAvailability() => GtkTestHost.Run(() =>
	{
		var (_, handler, _) = Create();

		if (GtkWebKit.IsAvailable)
		{
			Assert.IsType<global::WebKit.WebView>(handler.PlatformView);
		}
		else
		{
			var label = Assert.IsType<GtkLabel>(handler.PlatformView);
			Assert.Contains("HybridWebView is unavailable", label.Text);
		}
	});

	[GtkFact]
	public void CommandsReachThePlatformViewWithoutThrowing() => GtkTestHost.Run(() =>
	{
		var (view, _, _) = Create();

		view.SendRawMessage("noop");
		_ = view.EvaluateJavaScriptAsync("1");
	});

	[WebKitFact]
	public void ThePageLoadsFromTheAppPackageAndCallsDotNet() => GtkTestHost.Run(() =>
	{
		var (_, _, messages) = Create();

		// index.html -> hybridwebview.js + app.js through app://0.0.0.1/, then fetch(__hwvInvokeDotNet) -> Target -> raw message.
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page's InvokeDotNet calls");
		Assert.Contains("ready:5:hello gtk", messages);
	});

	[WebKitFact]
	public void RawMessagesTravelBothWays() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		view.SendRawMessage("ping \"quoted\" é");

		WaitFor(messages, m => m.StartsWith("echo:", StringComparison.Ordinal), "the echo");
		Assert.Contains("echo:ping \"quoted\" é", messages);
	});

	[WebKitFact]
	public void EvaluateJavaScriptAsyncReturnsTheResult() => GtkTestHost.Run(() =>
	{
		var (view, _, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");

		Assert.Equal("12", GtkTestHost.Await(view.EvaluateJavaScriptAsync("multiply(3, 4)")));
		Assert.Equal("hybrid", GtkTestHost.Await(view.EvaluateJavaScriptAsync("document.title")));
		Assert.Null(GtkTestHost.Await(view.EvaluateJavaScriptAsync("undefined")));
	});

	[WebKitFact]
	public void MessagesFromOutsideTheAppOriginAreIgnored() => GtkTestHost.Run(() =>
	{
		var (_, handler, messages) = Create();
		WaitFor(messages, m => m.StartsWith("ready:", StringComparison.Ordinal), "the page to load");
		var count = messages.Count;

		var view = (global::WebKit.WebView)handler.PlatformView!;
		var loaded = false;
		view.LoadChanged += (o, a) => loaded |= a.LoadEvent == global::WebKit.LoadEvent.Finished;
		view.LoadHtml("<script>window.webkit.messageHandlers.webwindowinterop.postMessage('__RawMessage|spoofed')</script>", "https://example.com/");
		Assert.True(GtkTestHost.PumpUntil(() => loaded, null, 15000), "the foreign page did not load");
		GtkTestHost.PumpUntil(() => false, null, 500);

		Assert.Equal(count, messages.Count);
	});
}
