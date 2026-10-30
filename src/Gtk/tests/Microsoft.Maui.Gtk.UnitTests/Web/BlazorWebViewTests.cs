using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
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
		public readonly List<string> Scripts = new();

		public void LoadUri(string uri) => Loaded.Add(uri);

		public void RunScript(string script) => Scripts.Add(script);
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

		Assert.Empty(_transport.Scripts);
	}

	[Fact]
	public async Task AddingARootComponentAttachesItToTheDocument()
	{
		await using var manager = CreateManager();
		manager.Navigate("/");

		await manager.AddRootComponentAsync(typeof(CounterComponent), "#app", ParameterView.Empty);

		// The page announces itself once its script runs; only then are root components attached.
		manager.MessageReceivedInternal(new Uri(Origin), "__bwv:[\"AttachPage\",\"" + Origin + "\",\"" + Origin + "\"]");

		Assert.Contains(_transport.Scripts, s => s.StartsWith("__dispatchMessageCallback(\"", StringComparison.Ordinal) && s.Contains("AttachToDocument", StringComparison.Ordinal));
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

/// <summary>The BlazorWebView handler: the placeholder where there is no WebKitGTK, and a rendered, interactive Blazor app where there is.</summary>
public class BlazorWebViewHandlerTests
{
	// Like an app: wwwroot next to the executable; _framework/blazor.webview.js is copied there by the Blazor GTK build targets.
	const string Root = "wwwroot";
	const string HostPage = "gtk_test_index.html";

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

	static (BlazorWebView View, GtkBlazorWebViewHandler Handler) Create()
	{
		WriteRoot();

		var view = new BlazorWebView { HostPage = Root + "/" + HostPage };
		view.RootComponents.Add(new RootComponent { Selector = "#app", ComponentType = typeof(CounterComponent) });

		return (view, WebTestApp.CreateHandler<GtkBlazorWebViewHandler>(view));
	}

	static string? Evaluate(global::WebKit.WebView webView, string script)
	{
		var request = new EvaluateJavaScriptAsyncRequest(script);
		GtkWebViewScripting.Evaluate(webView, script, request);
		return GtkTestHost.Await(request.Task);
	}

	[GtkFact]
	public void PlatformViewMatchesWebKitAvailability() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();

		if (GtkWebKit.IsAvailable)
		{
			Assert.IsType<global::WebKit.WebView>(handler.PlatformView);
		}
		else
		{
			var label = Assert.IsType<GtkLabel>(handler.PlatformView);
			Assert.Contains("BlazorWebView is unavailable", label.Text);
		}
	});

	[GtkFact]
	public void TheGtkHandlerIsTheOneRegistered() => GtkTestHost.Run(() =>
	{
		var (view, _) = Create();

		Assert.IsType<GtkBlazorWebViewHandler>(view.Handler);
	});

	[WebKitFact]
	public void TheBlazorAppRendersAndHandlesClicks() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();
		var webView = (global::WebKit.WebView)handler.PlatformView!;

		string? Text() => Evaluate(webView, "(document.getElementById('counter') || {}).innerText");

		Assert.True(GtkTestHost.PumpUntil(() => Text() == "Count: 0", null, 30000), "Blazor never rendered; text: " + Text());

		Evaluate(webView, "document.getElementById('counter').click()");
		Assert.True(GtkTestHost.PumpUntil(() => Text() == "Count: 1", null, 15000), "the click never reached the component; text: " + Text());

		Evaluate(webView, "document.getElementById('counter').click()");
		Assert.True(GtkTestHost.PumpUntil(() => Text() == "Count: 2", null, 15000), "the second click never reached the component; text: " + Text());
	});

	[WebKitFact]
	public void TheBlazorAppIsServedFromTheAppScheme() => GtkTestHost.Run(() =>
	{
		var (_, handler) = Create();
		var webView = (global::WebKit.WebView)handler.PlatformView!;

		Assert.True(GtkTestHost.PumpUntil(() => webView.Uri is not null && webView.Uri.StartsWith("app://0.0.0.1/", StringComparison.Ordinal), null, 15000), "uri: " + webView.Uri);
	});
}
