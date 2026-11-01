using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Gtk.Web;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>Message framing, script wrapping and the availability decision of the web handlers; no WebKit needed.</summary>
public class WebPureLogicTests
{
	[Fact]
	public void ReceiveRawMessageQuotesTheMessageAsAJavaScriptString()
	{
		var script = GtkWebViewScripts.ReceiveRawMessage("say \"hi\"\nnow");

		const string prefix = "window.external.receiveMessage(";
		Assert.StartsWith(prefix, script);
		Assert.EndsWith(")", script);

		// The argument is a JSON string literal: no raw quote or line break inside, and it reads back as the message.
		var literal = script.Substring(prefix.Length, script.Length - prefix.Length - 1);
		Assert.DoesNotContain("\n", literal);
		Assert.Equal("say \"hi\"\nnow", System.Text.Json.JsonSerializer.Deserialize<string>(literal));
	}

	[Fact]
	public void WrapHybridEvaluateEscapesQuotesAndLineBreaks()
	{
		var script = GtkWebViewScripts.WrapHybridEvaluate("a'b\nc");

		Assert.Equal("try{JSON.stringify(eval('a\\'b\\nc'))}catch(e){'null'};", script);
	}

	[Theory]
	[InlineData("null", null)]
	[InlineData("\"hello\"", "hello")]
	[InlineData("42", "42")]
	[InlineData("{\"a\":1}", "{\"a\":1}")]
	[InlineData(null, null)]
	public void UnwrapHybridResultMapsNullAndTrimsQuotes(string? raw, string? expected) =>
		Assert.Equal(expected, GtkWebViewScripts.UnwrapHybridResult(raw));

	[Fact]
	public void ConvertResultReturnsNullForUndefinedAndNull() =>
		Assert.Null(GtkWebViewScripts.ConvertResult(true, false, () => "undefined", () => "null"));

	[Fact]
	public void ConvertResultReturnsStringsAsTheyAreAndOtherValuesAsJson()
	{
		Assert.Equal("text", GtkWebViewScripts.ConvertResult(false, true, () => "text", () => "\"text\""));
		Assert.Equal("{\"a\":1}", GtkWebViewScripts.ConvertResult(false, false, () => "[object Object]", () => "{\"a\":1}"));
		Assert.Equal("[object Object]", GtkWebViewScripts.ConvertResult(false, false, () => "[object Object]", () => null));
	}

	[Theory]
	[InlineData(WebKit.NavigationType.LinkClicked, WebNavigationEvent.NewPage, WebNavigationEvent.NewPage)]
	[InlineData(WebKit.NavigationType.Other, WebNavigationEvent.NewPage, WebNavigationEvent.NewPage)]
	[InlineData(WebKit.NavigationType.FormSubmitted, WebNavigationEvent.NewPage, WebNavigationEvent.NewPage)]
	[InlineData(WebKit.NavigationType.BackForward, WebNavigationEvent.NewPage, WebNavigationEvent.Back)]
	[InlineData(WebKit.NavigationType.Reload, WebNavigationEvent.NewPage, WebNavigationEvent.Refresh)]
	[InlineData(WebKit.NavigationType.Other, WebNavigationEvent.Forward, WebNavigationEvent.Forward)]
	[InlineData(WebKit.NavigationType.BackForward, WebNavigationEvent.Forward, WebNavigationEvent.Forward)]
	[InlineData(WebKit.NavigationType.Other, WebNavigationEvent.Refresh, WebNavigationEvent.Refresh)]
	public void NavigationEventPrefersWhatTheAppAskedFor(WebKit.NavigationType type, WebNavigationEvent pending, WebNavigationEvent expected) =>
		Assert.Equal(expected, GtkWebViewScripts.ResolveNavigationEvent(type, pending));

	[Fact]
	public void WebKitIsUnavailableOffLinux() =>
		Assert.Contains("Linux", GtkWebKit.Evaluate(isLinux: false, disableValue: null, () => true));

	[Fact]
	public void WebKitCanBeDisabledByEnvironment()
	{
		Assert.Contains(GtkWebKit.DisableVariable, GtkWebKit.Evaluate(true, "1", () => true));
		Assert.Null(GtkWebKit.Evaluate(true, "0", () => true));
		Assert.Null(GtkWebKit.Evaluate(true, "", () => true));
		Assert.Null(GtkWebKit.Evaluate(true, null, () => true));
	}

	[Fact]
	public void WebKitNeedsTheNativeLibrary()
	{
		Assert.Null(GtkWebKit.Evaluate(true, null, () => true));
		Assert.Contains("libwebkitgtk-6.0", GtkWebKit.Evaluate(true, null, () => false));
	}

	[Fact]
	public void WebKitAvailabilityIsConsistent()
	{
		Assert.Equal(GtkWebKit.IsAvailable, GtkWebKit.UnavailableReason is null);

		if (!OperatingSystem.IsLinux())
			Assert.False(GtkWebKit.IsAvailable);
	}
}

/// <summary>The request handling of HybridWebView's app origin, against fakes.</summary>
public class GtkHybridWebViewResponderTests
{
	const string Origin = "app://0.0.0.1/";

	sealed class Fixture
	{
		public readonly Dictionary<string, byte[]> Files = new(StringComparer.Ordinal);
		public readonly List<string> Opened = new();
		public byte[]? InvokeInput;
		public byte[]? InvokeResult = Encoding.UTF8.GetBytes("{\"Result\":5}");
		public string? HybridRoot = "wwwroot";
		public string? DefaultFile = "index.html";
		public byte[]? EmbeddedScript = Encoding.UTF8.GetBytes("/* hybrid */");

		public GtkHybridWebViewResponder Create(Uri? origin = null) => new(
			() => HybridRoot,
			() => DefaultFile,
			body =>
			{
				InvokeInput = body;
				return Task.FromResult(InvokeResult);
			},
			path =>
			{
				var key = path.Replace('\\', '/');
				Opened.Add(key);
				return Task.FromResult<Stream?>(Files.TryGetValue(key, out var bytes) ? new MemoryStream(bytes) : null);
			},
			name => name == "_framework/hybridwebview.js" && EmbeddedScript is not null ? new MemoryStream(EmbeddedScript) : null,
			origin: origin);
	}

	static GtkUriSchemeRequest Get(string url) => new(url, "GET", Array.Empty<byte>(), Origin);

	static string Text(GtkUriSchemeResponse response) => Encoding.UTF8.GetString(response.Body);

	[Fact]
	public async Task TheOriginRootServesTheDefaultFile()
	{
		var fixture = new Fixture();
		fixture.Files["wwwroot/index.html"] = Encoding.UTF8.GetBytes("<html>home</html>");

		var response = await fixture.Create().RespondAsync(Get(Origin));

		Assert.Equal(200, response.Status);
		Assert.Equal("text/html", response.ContentType);
		Assert.Equal("<html>home</html>", Text(response));
	}

	[Fact]
	public async Task StaticFilesAreServedFromTheHybridRootWithTheirContentType()
	{
		var fixture = new Fixture();
		fixture.Files["wwwroot/css/site.css"] = Encoding.UTF8.GetBytes("body{}");

		var response = await fixture.Create().RespondAsync(Get(Origin + "css/site.css?v=3"));

		Assert.Equal(200, response.Status);
		Assert.Equal("text/css", response.ContentType);
		Assert.Equal("body{}", Text(response));
		Assert.Equal(new[] { "wwwroot/css/site.css" }, fixture.Opened);
	}

	[Fact]
	public async Task UnknownFilesAreNotFound()
	{
		var response = await new Fixture().Create().RespondAsync(Get(Origin + "missing.png"));

		Assert.Equal(404, response.Status);
		Assert.Empty(response.Body);
	}

	[Fact]
	public async Task OtherOriginsAreNotServed()
	{
		var fixture = new Fixture();
		fixture.Files["wwwroot/index.html"] = new byte[] { 1 };

		var response = await fixture.Create().RespondAsync(Get("https://example.com/index.html"));

		Assert.Equal(404, response.Status);
		Assert.Empty(fixture.Opened);
	}

	[Fact]
	public async Task WithoutAHybridRootNothingIsServed()
	{
		var fixture = new Fixture { HybridRoot = null };

		var response = await fixture.Create().RespondAsync(Get(Origin + "index.html"));

		Assert.Equal(404, response.Status);
	}

	[Fact]
	public async Task TheBundledScriptIsServed()
	{
		var response = await new Fixture().Create().RespondAsync(Get(Origin + "_framework/hybridwebview.js"));

		Assert.Equal(200, response.Status);
		Assert.Equal("application/javascript", response.ContentType);
		Assert.Equal("/* hybrid */", Text(response));
	}

	[Fact]
	public async Task InvokeDotNetRunsThePostedBody()
	{
		var fixture = new Fixture();
		var body = Encoding.UTF8.GetBytes("{\"MethodName\":\"Add\"}");

		var response = await fixture.Create().RespondAsync(new GtkUriSchemeRequest(Origin + "__hwvInvokeDotNet", "POST", body, Origin));

		Assert.Equal(200, response.Status);
		Assert.Equal("application/json", response.ContentType);
		Assert.Equal("{\"Result\":5}", Text(response));
		Assert.Equal(body, fixture.InvokeInput);
	}

	[Fact]
	public async Task InvokeDotNetOnlyAcceptsPost()
	{
		var response = await new Fixture().Create().RespondAsync(new GtkUriSchemeRequest(Origin + "__hwvInvokeDotNet", "GET", new byte[] { 1 }, Origin));

		Assert.Equal(405, response.Status);
	}

	[Fact]
	public async Task InvokeDotNetNeedsABody()
	{
		var fixture = new Fixture();

		var response = await fixture.Create().RespondAsync(new GtkUriSchemeRequest(Origin + "__hwvInvokeDotNet", "POST", Array.Empty<byte>(), Origin));

		Assert.Equal(400, response.Status);
		Assert.Null(fixture.InvokeInput);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("https://evil.example/page.html")]
	[InlineData("not a uri")]
	public async Task InvokeDotNetIsRefusedFromOutsideTheAppOrigin(string? pageUri)
	{
		var fixture = new Fixture();

		var response = await fixture.Create().RespondAsync(new GtkUriSchemeRequest(Origin + "__hwvInvokeDotNet", "POST", new byte[] { 1 }, pageUri));

		Assert.Equal(400, response.Status);
		Assert.Null(fixture.InvokeInput);
	}

	[Fact]
	public async Task InvokeDotNetWithoutResultIsNotFound()
	{
		var fixture = new Fixture { InvokeResult = null };

		var response = await fixture.Create().RespondAsync(new GtkUriSchemeRequest(Origin + "__hwvInvokeDotNet", "POST", new byte[] { 1 }, Origin));

		Assert.Equal(404, response.Status);
	}

	[Theory]
	[InlineData("app://0.0.0.1/", true)]
	[InlineData("app://0.0.0.1/sub/page.html", true)]
	[InlineData("app://0.0.0.2/", false)]
	[InlineData("https://0.0.0.1/", false)]
	[InlineData(null, false)]
	public void OnlyPagesOfTheAppOriginAreTrusted(string? pageUri, bool expected) =>
		Assert.Equal(expected, GtkHybridWebViewResponder.IsFromAppOrigin(pageUri));

	const string WebView2Origin = "https://0.0.0.1/";

	static GtkUriSchemeRequest WebView2Request(string url, string method, byte[] body, params (string Name, string Value)[] headers) =>
		new(url, method, body, null, headers.Select(h => new KeyValuePair<string, string>(h.Name, h.Value)).ToList());

	[Fact]
	public async Task TheWebView2OriginServesFilesAndRejectsTheWebKitOrigin()
	{
		var fixture = new Fixture();
		fixture.Files["wwwroot/index.html"] = Encoding.UTF8.GetBytes("<html/>");
		var responder = fixture.Create(GtkHybridWebViewResponder.WebView2OriginUri);

		var root = await responder.RespondAsync(WebView2Request(WebView2Origin, "GET", Array.Empty<byte>()));
		var script = await responder.RespondAsync(WebView2Request(WebView2Origin + "_framework/hybridwebview.js", "GET", Array.Empty<byte>()));
		var foreign = await responder.RespondAsync(Get(Origin + "index.html"));

		Assert.Equal(200, root.Status);
		Assert.Equal("text/html", root.ContentType);
		Assert.Equal("<html/>", Encoding.UTF8.GetString(root.Body));
		Assert.Equal(200, script.Status);
		Assert.Equal(404, foreign.Status);
	}

	[Fact]
	public async Task InvokeDotNetOnWebView2NeedsTheInvokeTokenHeader()
	{
		var fixture = new Fixture();
		var responder = fixture.Create(GtkHybridWebViewResponder.WebView2OriginUri);
		var url = WebView2Origin + "__hwvInvokeDotNet";

		// The page URI is unknown to WebView2 requests: the headers decide.
		var withoutToken = await responder.RespondAsync(WebView2Request(url, "POST", new byte[] { 1 }, ("Origin", "https://0.0.0.1")));
		var wrongOrigin = await responder.RespondAsync(WebView2Request(url, "POST", new byte[] { 1 }, ("X-Maui-Invoke-Token", "HybridWebView"), ("Origin", "https://evil.example")));
		var accepted = await responder.RespondAsync(WebView2Request(url, "POST", new byte[] { 1 }, ("X-Maui-Invoke-Token", "HybridWebView"), ("Origin", "https://0.0.0.1")));
		var referer = await responder.RespondAsync(WebView2Request(url, "POST", new byte[] { 2 }, ("X-Maui-Invoke-Token", "HybridWebView"), ("Referer", "https://0.0.0.1/")));

		Assert.Equal(400, withoutToken.Status);
		Assert.Equal(400, wrongOrigin.Status);
		Assert.Equal(200, accepted.Status);
		Assert.Equal(200, referer.Status);
		Assert.Equal(new byte[] { 2 }, fixture.InvokeInput);
	}

	[Fact]
	public async Task InvokeDotNetOnWebView2StillNeedsAPostWithABody()
	{
		var responder = new Fixture().Create(GtkHybridWebViewResponder.WebView2OriginUri);
		var url = WebView2Origin + "__hwvInvokeDotNet";
		var headers = new[] { ("X-Maui-Invoke-Token", "HybridWebView"), ("Origin", "https://0.0.0.1") };

		Assert.Equal(405, (await responder.RespondAsync(WebView2Request(url, "GET", Array.Empty<byte>(), headers))).Status);
		Assert.Equal(400, (await responder.RespondAsync(WebView2Request(url, "POST", Array.Empty<byte>(), headers))).Status);
	}

	[Theory]
	[InlineData("https://0.0.0.1/", true)]
	[InlineData("https://0.0.0.1/sub/page.html", true)]
	[InlineData("https://0.0.0.2/", false)]
	[InlineData("app://0.0.0.1/", false)]
	[InlineData(null, false)]
	public void PageTrustFollowsTheGivenOrigin(string? pageUri, bool expected) =>
		Assert.Equal(expected, GtkHybridWebViewResponder.IsFromOrigin(GtkHybridWebViewResponder.WebView2OriginUri, pageUri));

	[Theory]
	[InlineData("plain")]
	[InlineData("a b%c+d&e=é€")]
	[InlineData("nul\u0000inside")]
	public void WebView2MessagesSurviveTheUrlEncodingRoundTrip(string message)
	{
		var encoded = GtkHybridWebView2Bridge.EncodeMessage(message);

		Assert.DoesNotContain('\u0000', encoded);
		Assert.Equal(message, GtkHybridWebView2Bridge.DecodeMessage(encoded));
		// What hybridwebview.js posts is encodeURIComponent(message).
		Assert.Equal(message, GtkHybridWebView2Bridge.DecodeMessage(Uri.EscapeDataString(message)));
	}

	[Fact]
	public void WebView2ResponseHeadersCarryLengthAndTypeWhenKnown()
	{
		Assert.Equal(
			"Content-Length: 3\r\nContent-Type: text/html\r\n",
			GtkHybridWebView2Bridge.BuildHeaders(GtkUriSchemeResponse.Ok("text/html", new byte[3])));
		Assert.Equal(string.Empty, GtkHybridWebView2Bridge.BuildHeaders(GtkUriSchemeResponse.NotFound()));
		Assert.Equal("Content-Length: 1\r\n", GtkHybridWebView2Bridge.BuildHeaders(GtkUriSchemeResponse.Ok(null, new byte[1])));
	}
}
