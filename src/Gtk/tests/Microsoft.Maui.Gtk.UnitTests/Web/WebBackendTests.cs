using System;
using Microsoft.Maui.Gtk.Web;
using Xunit;
using GtkLabel = global::Gtk.Label;

namespace Microsoft.Maui.Gtk.UnitTests.Web;

/// <summary>The engine choice, the WebView2 availability decision and the script-result conversion: pure logic, no web view.</summary>
public class WebBackendTests
{
	[Theory]
	[InlineData(true, false, true, false, "WebKitGtk")]
	[InlineData(true, false, false, false, "None")]
	[InlineData(true, false, false, true, "None")]
	[InlineData(false, true, false, true, "WebView2")]
	[InlineData(false, true, true, true, "WebView2")]
	[InlineData(false, true, true, false, "None")]
	[InlineData(false, false, true, true, "None")]
	public void TheBackendFollowsTheOperatingSystemAndAvailability(bool isLinux, bool isWindows, bool webKit, bool webView2, string expected) =>
		Assert.Equal(expected, GtkWebBackend.Select(isLinux, isWindows, webKit, webView2).ToString());

	[Fact]
	public void PlaceholderTextNamesTheControlAndTheReason()
	{
		Assert.Equal("WebView is unavailable: because", GtkWebBackend.PlaceholderText("WebView", "because"));
		Assert.Contains("BlazorWebView is unavailable", GtkWebBackend.PlaceholderText("BlazorWebView", null));
	}

	[Fact]
	public void WebView2IsUnavailableOffWindows() =>
		Assert.Contains("Windows", GtkWebView2Environment.Evaluate(false, null, () => "1.0", out _));

	[Theory]
	[InlineData("1")]
	[InlineData("true")]
	public void WebView2CanBeDisabledByEnvironment(string value) =>
		Assert.Contains(GtkWebView2Environment.DisableVariable, GtkWebView2Environment.Evaluate(true, value, () => "1.0", out _));

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("0")]
	public void WebView2DisableVariableOffOrEmptyIsIgnored(string? value)
	{
		Assert.Null(GtkWebView2Environment.Evaluate(true, value, () => "120.0.1", out var version));
		Assert.Equal("120.0.1", version);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	public void AMissingRuntimeNamesTheDownload(string? version)
	{
		var reason = GtkWebView2Environment.Evaluate(true, null, () => version, out var found);

		Assert.Contains("WebView2 Runtime not installed", reason);
		Assert.Contains("https://go.microsoft.com/fwlink/p/?LinkId=2124703", reason);
		Assert.Null(found);
	}

	[Fact]
	public void AProbeThatThrowsMeansNoRuntime()
	{
		var reason = GtkWebView2Environment.Evaluate(true, null, () => throw new InvalidOperationException("no runtime"), out _);

		Assert.Contains("WebView2 Runtime not installed", reason);
	}

	[Fact]
	public void AMissingLoaderIsReportedWithItsMessage()
	{
		var reason = GtkWebView2Environment.Evaluate(true, null, () => throw new DllNotFoundException("WebView2Loader.dll"), out _);

		Assert.Contains("WebView2 Runtime not installed", reason);
		Assert.Contains("WebView2Loader.dll", reason);
	}

	[Fact]
	public void TheUserDataFolderIsUnderLocalAppDataUnlessTheVariableIsSet()
	{
		Assert.Equal(System.IO.Path.Combine("C:\\Local", "myapp", "WebView2"), GtkWebView2Environment.DefaultUserDataFolder(null, "C:\\Local", "myapp"));
		Assert.Equal(System.IO.Path.Combine("C:\\Local", "myapp", "WebView2"), GtkWebView2Environment.DefaultUserDataFolder("", "C:\\Local", "myapp"));
		// WebView2 reads WEBVIEW2_USER_DATA_FOLDER itself when no folder is passed.
		Assert.Null(GtkWebView2Environment.DefaultUserDataFolder("D:\\data", "C:\\Local", "myapp"));
	}

	[Theory]
	[InlineData(null, null)]
	[InlineData("", null)]
	[InlineData("null", null)]
	[InlineData("\"hello\"", "hello")]
	[InlineData("\"\"", "")]
	[InlineData("3", "3")]
	[InlineData("true", "true")]
	[InlineData("1.5", "1.5")]
	[InlineData("{\"a\":[1,2]}", "{\"a\":[1,2]}")]
	[InlineData("[1,\"x\"]", "[1,\"x\"]")]
	[InlineData("\"say \\\"hi\\\"\\n\"", "say \"hi\"\n")]
	[InlineData("\"caf\\u00e9\"", "café")]
	public void WebView2ScriptResultsBecomeStringsOrJson(string? json, string? expected) =>
		Assert.Equal(expected, GtkWebViewScripts.ConvertWebView2Result(json));

	[Fact]
	public void AMalformedQuotedResultIsReturnedAsItIs() =>
		Assert.Equal("\"unterminated", GtkWebViewScripts.ConvertWebView2Result("\"unterminated"));

	[GtkFact]
	public void AWebViewWithoutAnEngineIsAPlaceholderThatSaysWhy() => GtkTestHost.Run(() =>
	{
		// The platform view is the engine's widget, or a label with the reason; never an exception.
		var widget = GtkWebBackend.CreatePlatformView("WebView");

		if (GtkWebBackend.Current == GtkWebBackendKind.None)
		{
			var label = Assert.IsType<GtkLabel>(widget);
			Assert.StartsWith("WebView is unavailable: ", label.Text);
			Assert.Null(GtkWebBackend.CreateEngine(widget));
		}
		else
		{
			Assert.IsNotType<GtkLabel>(widget);
			Assert.NotNull(GtkWebBackend.CreateEngine(widget));
			Assert.Null(GtkWebBackend.UnavailableReason);
		}
	});
}
