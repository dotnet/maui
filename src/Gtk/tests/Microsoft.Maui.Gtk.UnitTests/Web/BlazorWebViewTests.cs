using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>A component that counts button clicks: the smallest thing that proves Blazor renders and receives events.</summary>
public class CounterComponent : ComponentBase
{
	int _count;

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenElement(0, "button");
		builder.AddAttribute(1, "id", "counter");
		builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () => _count++));
		builder.AddContent(3, "Count: " + _count);
		builder.CloseElement();
	}
}

/// <summary>Everything GtkWebViewManager and the file provider do that does not need a web view.</summary>
public class GtkWebViewManagerTests : IDisposable
{
	const string Origin = "app://0.0.0.1/";

	sealed class FakeTransport : IGtkBlazorTransport
	{
		public readonly List<string> Loaded = new();
		public readonly List<string> Messages = new();

		public void LoadUri(string uri) => Loaded.Add(uri);

		public void PostMessage(string message) => Messages.Add(message);
	}

	readonly string _package = Path.Combine(Path.GetTempPath(), "maui-gtk-blazor-" + Guid.NewGuid().ToString("N"));
	readonly FakeTransport _transport = new();

	public GtkWebViewManagerTests()
	{
		Directory.CreateDirectory(Path.Combine(_package, "wwwroot", "css"));
		File.WriteAllText(Path.Combine(_package, "wwwroot", "index.html"), "<html><body><div id=\"app\"></div></body></html>");
		File.WriteAllText(Path.Combine(_package, "wwwroot", "css", "site.css"), "body{color:red}");
		File.WriteAllText(Path.Combine(_package, "secret.txt"), "outside the content root");

		// In an app this file is copied next to the executable from the static web assets (see the build targets).
		Directory.CreateDirectory(Path.Combine(_package, "wwwroot", "_framework"));
		File.WriteAllText(Path.Combine(_package, "wwwroot", "_framework", "blazor.webview.js"), "/* blazor */");
	}

	public void Dispose()
	{
		try
		{
			Directory.Delete(_package, recursive: true);
		}
		catch (IOException)
		{
		}
	}

	GtkWebViewManager CreateManager()
	{
		var services = new ServiceCollection().AddBlazorWebView().BuildServiceProvider();
		return new GtkWebViewManager(
			_transport,
			services,
			Dispatcher.CreateDefault(),
			new GtkMauiAssetFileProvider("wwwroot", _package),
			new JSComponentConfigurationStore(),
			"wwwroot",
			"index.html");
	}

	static GtkUriSchemeRequest Get(string url) => new(url, "GET", Array.Empty<byte>(), Origin);

	[Fact]
	public void NavigateLoadsTheAppOrigin()
	{
		CreateManager().Navigate("/");

		Assert.Equal(new[] { Origin }, _transport.Loaded);
	}

	[Fact]
	public void MessagesAreDispatchedThroughTheCallbackTheScriptRegistered()
	{
		var script = GtkWebViewManager.DispatchMessageScript("__bwv:[\"a\"]\n");

		Assert.StartsWith("__dispatchMessageCallback(\"", script);
		Assert.EndsWith("\")", script);
		Assert.DoesNotContain("\n", script);
		Assert.DoesNotContain("\"a\"]", script);
	}

	[Fact]
	public async Task TheHostPageIsServedForTheOriginRoot()
	{
		var response = await CreateManager().RespondAsync(Get(Origin));

		Assert.Equal(200, response.Status);
		Assert.StartsWith("text/html", response.ContentType);
		Assert.Contains("<div id=\"app\">", System.Text.Encoding.UTF8.GetString(response.Body));
	}

	[Fact]
	public async Task RoutesWithoutAnExtensionFallBackOnTheHostPage()
	{
		var response = await CreateManager().RespondAsync(Get(Origin + "counter/5?x=1"));

		Assert.Equal(200, response.Status);
		Assert.Contains("<div id=\"app\">", System.Text.Encoding.UTF8.GetString(response.Body));
	}

	[Fact]
	public async Task StaticFilesAreServedWithTheirContentType()
	{
		var response = await CreateManager().RespondAsync(Get(Origin + "css/site.css"));

		Assert.Equal(200, response.Status);
		Assert.StartsWith("text/css", response.ContentType);
		Assert.Equal("body{color:red}", System.Text.Encoding.UTF8.GetString(response.Body));
	}

	[Fact]
	public async Task TheBlazorScriptIsServedFromTheStaticWebAssets()
	{
		var response = await CreateManager().RespondAsync(Get(Origin + "_framework/blazor.webview.js"));

		Assert.Equal(200, response.Status);
		Assert.Contains("javascript", response.ContentType);
		Assert.Equal("/* blazor */", System.Text.Encoding.UTF8.GetString(response.Body));
	}

	[Fact]
	public async Task MissingFilesWithAnExtensionAreNotFound()
	{
		var response = await CreateManager().RespondAsync(Get(Origin + "missing.png"));

		Assert.Equal(404, response.Status);
		Assert.Empty(response.Body);
	}

	[Fact]
	public async Task FilesOutsideTheContentRootAreNotServed()
	{
		var response = await CreateManager().RespondAsync(Get(Origin + "..%2fsecret.txt"));

		Assert.NotEqual("outside the content root", System.Text.Encoding.UTF8.GetString(response.Body));
	}

	[Fact]
	public async Task MessagesFromOtherOriginsAreIgnored()
	{
		await using var manager = CreateManager();

		manager.MessageReceivedInternal(new Uri("https://example.com/"), "__bwv:[\"anything\"]");

		Assert.Empty(_transport.Messages);
	}

	[Fact]
	public async Task AddingARootComponentAttachesItToTheDocument()
	{
		await using var manager = CreateManager();
		manager.Navigate("/");

		await manager.AddRootComponentAsync(typeof(CounterComponent), "#app", ParameterView.Empty);

		// The page announces itself once its script runs; only then are root components attached.
		manager.MessageReceivedInternal(new Uri(Origin), "__bwv:[\"AttachPage\",\"" + Origin + "\",\"" + Origin + "\"]");

		Assert.Contains(_transport.Messages, m => m.Contains("AttachToDocument", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("index.html", true)]
	[InlineData("css/site.css", true)]
	[InlineData("css\\site.css", true)]
	[InlineData("/css/site.css", true)]
	[InlineData("../secret.txt", false)]
	[InlineData("css/../../secret.txt", false)]
	[InlineData("", false)]
	public void TheFileProviderStaysInsideItsRoot(string subpath, bool exists)
	{
		var provider = new GtkMauiAssetFileProvider("wwwroot", _package);

		Assert.Equal(exists, provider.GetFileInfo(subpath).Exists);
	}

	[Fact]
	public void TheFileProviderReadsFiles()
	{
		var file = new GtkMauiAssetFileProvider("wwwroot", _package).GetFileInfo("css/site.css");

		using var reader = new StreamReader(file.CreateReadStream());
		Assert.Equal("body{color:red}", reader.ReadToEnd());
		Assert.Equal("site.css", file.Name);
		Assert.False(file.IsDirectory);
	}
}

/// <summary>The WebView2 side of the Blazor transport that does not need a web view: the contract between the page, the shim and the host.</summary>
public class WebView2BlazorProtocolTests
{
	[Fact]
	public void RequestsOfTheHttpsAppOriginAreIntercepted()
	{
		Assert.Equal("https://0.0.0.1/*", WebView2BlazorProtocol.RequestFilter);
		Assert.Equal("https", GtkWebViewManager.WebView2AppOriginUri.Scheme);
		Assert.Equal("app", GtkWebViewManager.AppOriginUri.Scheme);
	}

	[Fact]
	public void TheShimMapsWindowExternalOntoChromeWebview()
	{
		var script = WebView2BlazorProtocol.InitScript;

		Assert.Contains("window.external", script);
		Assert.Contains("sendMessage", script);
		Assert.Contains("receiveMessage", script);
		Assert.Contains("window.chrome.webview.postMessage(message)", script);
		Assert.Contains("window.chrome.webview.addEventListener('message'", script);
		Assert.Contains("e.data", script);

		// The host page loads blazor.webview.js with autostart="false": the shim starts it once the document is parsed.
		Assert.Contains("DOMContentLoaded", script);
		Assert.Contains("window.Blazor.start()", script);
		Assert.DoesNotContain("webkit.messageHandlers", script);
	}

	[Theory]
	[InlineData("text/html", 200, "OK")]
	[InlineData("text/css", 200, "OK")]
	[InlineData(null, 404, "Not Found")]
	public void ResponseHeadersCarryTheContentTypeAndForbidCaching(string? contentType, int status, string reason)
	{
		var headers = WebView2BlazorProtocol.BuildResponseHeaders(new GtkUriSchemeResponse(status, reason, contentType, new byte[] { 1 }));

		if (contentType is null)
			Assert.DoesNotContain("Content-Type", headers);
		else
			Assert.Contains("Content-Type: " + contentType + "\r\n", headers);

		Assert.Contains("Cache-Control: no-cache", headers);
		Assert.Contains("Access-Control-Allow-Origin: *", headers);
		Assert.False(headers.EndsWith("\n", StringComparison.Ordinal), "the header block must not end with a blank line");
	}

	[Theory]
	[InlineData("https://0.0.0.1/", true)]
	[InlineData("https://0.0.0.1/counter/5", true)]
	[InlineData("https://0.0.0.1/css/site.css", true)]
	[InlineData("https://0.0.0.1/missing.png", false)]
	public async Task AManagerOnTheHttpsOriginServesTheApp(string url, bool found)
	{
		var package = Path.Combine(Path.GetTempPath(), "maui-gtk-blazor-" + Guid.NewGuid().ToString("N"));
		try
		{
			Directory.CreateDirectory(Path.Combine(package, "wwwroot", "css"));
			File.WriteAllText(Path.Combine(package, "wwwroot", "index.html"), "<html><body><div id=\"app\"></div></body></html>");
			File.WriteAllText(Path.Combine(package, "wwwroot", "css", "site.css"), "body{}");

			var services = new ServiceCollection().AddBlazorWebView().BuildServiceProvider();
			var loaded = new List<string>();
			await using var manager = new GtkWebViewManager(
				new RecordingTransport(loaded),
				services,
				Dispatcher.CreateDefault(),
				new GtkMauiAssetFileProvider("wwwroot", package),
				new JSComponentConfigurationStore(),
				"wwwroot",
				"index.html",
				null,
				GtkWebViewManager.WebView2AppOriginUri);

			manager.Navigate("/");

			Assert.Equal(GtkWebViewManager.WebView2AppOriginUri, manager.AppOrigin);
			Assert.Equal(new[] { "https://0.0.0.1/" }, loaded);

			var response = await manager.RespondAsync(new GtkUriSchemeRequest(url, "GET", Array.Empty<byte>(), null));

			Assert.Equal(found ? 200 : 404, response.Status);
		}
		finally
		{
			Directory.Delete(package, recursive: true);
		}
	}

	sealed class RecordingTransport : IGtkBlazorTransport
	{
		readonly List<string> _loaded;

		public RecordingTransport(List<string> loaded) => _loaded = loaded;

		public void LoadUri(string uri) => _loaded.Add(uri);

		public void PostMessage(string message)
		{
		}
	}
}

/// <summary>The BlazorWebView handler: the placeholder where there is no engine, and a rendered, interactive Blazor app on WebKitGTK (Linux) or WebView2 (Windows).</summary>
public class BlazorWebViewHandlerTests : IDisposable
{
	// Like an app: wwwroot next to the executable; _framework/blazor.webview.js is copied there by the Blazor GTK build targets.
	const string Root = "wwwroot";
	const string HostPage = "gtk_test_index.html";

	readonly List<global::Gtk.Window> _windows = new();

	/// <summary>Retires the windows the tests put web views in (on the GTK thread; windows are never destroyed).</summary>
	public void Dispose()
	{
		if (_windows.Count > 0)
			GtkTestHost.Run(() => _windows.ForEach(GtkTestHost.Retire));
	}

	static void WriteRoot()
	{
		var dir = Path.Combine(AppContext.BaseDirectory, Root);
		Directory.CreateDirectory(dir);
		File.WriteAllText(Path.Combine(dir, HostPage), @"<!DOCTYPE html>
<html><head><title>blazor</title><base href=""/"" /></head>
<body><div id=""app"">loading</div>
<script src=""_framework/blazor.webview.js"" autostart=""false""></script>
</body></html>");
	}

	(BlazorWebView View, GtkBlazorWebViewHandler Handler) Create()
	{
		WriteRoot();

		var view = new BlazorWebView { HostPage = Root + "/" + HostPage };
		view.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(CounterComponent) });

		var handler = WebTestApp.CreateHandler<GtkBlazorWebViewHandler>(view);
		if (WebTestApp.Present(handler) is { } window)
			_windows.Add(window);

		return (view, handler);
	}

	// The engine-neutral way to run a script in the page: the WebKit view, or the engine of the WebView2 host.
	static string? Evaluate(GtkBlazorWebViewHandler handler, string script)
	{
		var request = new EvaluateJavaScriptAsyncRequest(script);
		if (handler.PlatformView is global::WebKit.WebView webView)
			GtkWebViewScripting.Evaluate(webView, script, request);
		else
			((MauiGtkWebView2Host)handler.PlatformView!).Engine.EvaluateScript(script, request);

		return GtkTestHost.Await(request.Task);
	}

	static string? CurrentUri(GtkBlazorWebViewHandler handler) =>
		handler.PlatformView is global::WebKit.WebView webView ? webView.Uri : ((MauiGtkWebView2Host)handler.PlatformView!).Engine.Uri;

	static string ExpectedOrigin =>
		GtkWebBackend.Current == GtkWebBackendKind.WebView2 ? "https://0.0.0.1/" : "app://0.0.0.1/";

	static string? CounterText(GtkBlazorWebViewHandler handler) =>
		Evaluate(handler, "(document.getElementById('counter') || {}).innerText");

	[GtkFact]
	public void PlatformViewMatchesTheBackend() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();

		switch (GtkWebBackend.Current)
		{
			case GtkWebBackendKind.WebKitGtk:
				Assert.IsType<global::WebKit.WebView>(handler.PlatformView);
				break;

			case GtkWebBackendKind.WebView2:
				Assert.IsType<MauiGtkWebView2Host>(handler.PlatformView);
				break;

			default:
				var label = Assert.IsType<GtkLabel>(handler.PlatformView);
				Assert.Contains("BlazorWebView is unavailable", label.Text);
				Assert.Contains(GtkWebBackend.UnavailableReason!, label.Text);
				break;
		}
	});

	[GtkFact]
	public void TheGtkHandlerIsTheOneRegistered() => GtkTestHost.Run(() =>
	{
		var (view, _) = Create();

		Assert.IsType<GtkBlazorWebViewHandler>(view.Handler);
	});

	[WebEngineFact]
	public void TheBlazorAppRendersAndHandlesClicks() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();

		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 0", null, 30000), "Blazor never rendered; text: " + CounterText(handler));

		Evaluate(handler, "document.getElementById('counter').click()");
		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 1", null, 15000), "the click never reached the component; text: " + CounterText(handler));

		Evaluate(handler, "document.getElementById('counter').click()");
		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 2", null, 15000), "the second click never reached the component; text: " + CounterText(handler));
	});

	[WebEngineFact]
	public void TheBlazorAppIsServedFromTheAppOrigin() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();

		Assert.True(GtkTestHost.PumpUntil(() => CurrentUri(handler)?.StartsWith(ExpectedOrigin, StringComparison.Ordinal) == true, null, 15000), "uri: " + CurrentUri(handler));
		Assert.True(GtkTestHost.PumpUntil(() => Evaluate(handler, "document.title") == "blazor", null, 15000), "the host page was not served");
	});

	[WebEngineFact]
	public void LinksOutsideTheAppAreOfferedToUrlLoading() => GtkTestHost.Run(() =>
	{
		var (view, handler) = Create();
		var requested = new List<Uri>();
		view.UrlLoading += (_, e) =>
		{
			// The app's own pages (the initial load on WebKit) keep the default strategy; only the foreign link is cancelled.
			if (e.Url.Host != "example.com")
				return;

			requested.Add(e.Url);
			e.UrlLoadingStrategy = UrlLoadingStrategy.CancelLoad;
		};

		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 0", null, 30000), "Blazor never rendered");

		Evaluate(handler, "window.location.href = 'https://example.com/elsewhere'");

		Assert.True(GtkTestHost.PumpUntil(() => requested.Count > 0, null, 15000), "UrlLoading was not raised");
		Assert.Equal("https://example.com/elsewhere", requested[0].AbsoluteUri);

		// CancelLoad keeps the web view on the app.
		GtkTestHost.PumpUntil(() => false, null, 500);
		Assert.StartsWith(ExpectedOrigin, CurrentUri(handler), StringComparison.Ordinal);
		Assert.Equal("Count: 0", CounterText(handler));
	});

	[WebEngineFact]
	public void DisconnectingAfterRenderingDoesNotThrow() => GtkTestHost.Run(() =>
	{
		var (view, handler) = Create();

		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 0", null, 30000), "Blazor never rendered");

		var exception = Record.Exception(() => ((IElement)view).Handler?.DisconnectHandler());

		Assert.Null(exception);
	});

	[WebView2Fact]
	public void ARerealizedWebView2ControlLoadsTheAppOncePastTheShim() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();
		var host = (MauiGtkWebView2Host)handler.PlatformView!;
		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 0", null, 30000), "Blazor never rendered");

		// Move the widget to another window: its controller is closed and a new one created.
		var first = _windows[0];
		first.Child = null;
		var oldController = host.Controller;
		var starts = 0;
		var log = new List<string>();
		host.CoreWebView2Initialized += (_, core) => core.NavigationStarting += (_, e) => { starts++; log.Add(e.Uri); };
		var second = new global::Gtk.Window { Decorated = false, DefaultWidth = 800, DefaultHeight = 600, Child = host };
		_windows.Add(second);
		second.Present();
		GtkTestHost.Resize(second, 800, 600);

		Assert.True(GtkTestHost.PumpUntil(() => CounterText(handler) == "Count: 0", second, 30000), "Blazor did not render in the new control; text: " + CounterText(handler));
		GtkTestHost.PumpUntil(() => false, second, 1500);

		// One load, past the shim: no page without window.external followed by a second load of the start page.
		Assert.NotSame(oldController, host.Controller);
		Assert.True(starts == 1, starts + " navigations: " + string.Join(" ; ", log));
		Assert.Equal("Count: 0", CounterText(handler));
	});
}
