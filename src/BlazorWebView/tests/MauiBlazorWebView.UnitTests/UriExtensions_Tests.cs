using Microsoft.AspNetCore.Components.WebView.Maui;

namespace Microsoft.Maui.MauiBlazorWebView.UnitTests;

public class UriExtensions_Tests
{
	private readonly Uri _baseUri = new("https://example.com/");

	[Theory]
	[InlineData("https://example.com/page", true)]
	[InlineData("page/subpage", false)]
	[InlineData("this is not a uri!", false)]
	[InlineData("https://example.com/", true)]
	[InlineData("https://example.com/page/", true)]
	[InlineData("https://example.com/page?weight=62.5", true)]
	[InlineData("https://example.com/page#section.1", true)]
	[InlineData("https://example.com/file.txt", false)]
	[InlineData("https://example.com/page.json?foo=bar", false)]
	[InlineData("ftp://example.com/page", false)]
	[InlineData("/relative/path", false)]
	[InlineData("", false)]
	[InlineData(null, false)]
	[InlineData("https://example.com/test", true)]
	[InlineData("https://example.com/folder/subfolder/", true)]
	[InlineData("https://example.com/folder/file.exe", false)]
	[InlineData("https://subdomain.example.com/page", false)]
	[InlineData("https://example.com/path/with/.dot/segment", true)]
	[InlineData("https://example.com/path/with space", true)]
	[InlineData("https://example.com/path/with%20encoded%20space", true)]
	[InlineData("https://example.com/page?param=value&param2=value2", true)]
	[InlineData("https://example.com/page.html", false)]
	[InlineData("HTTPS://EXAMPLE.COM/PAGE", true)]
	[InlineData("https://subdomain.example.com/", false)]
	[InlineData("https://subdomain.example.com/page/", false)]
	[InlineData("https://subdomain.example.com/page?weight=62.5", false)]
	[InlineData("https://subdomain.example.com/page#section.1", false)]
	[InlineData("https://subdomain.example.com/file.txt", false)]
	[InlineData("https://subdomain.example.com/page.json?foo=bar", false)]
	[InlineData("ftp://subdomain.example.com/page", false)]
	[InlineData("https://subdomain.example.com/test", false)]
	[InlineData("https://subdomain.example.com/folder/subfolder/", false)]
	[InlineData("https://subdomain.example.com/folder/file.exe", false)]
	[InlineData("https://subdomain.example.com/path/with/.dot/segment", false)]
	[InlineData("https://subdomain.example.com/path/with space", false)]
	[InlineData("https://subdomain.example.com/path/with%20encoded%20space", false)]
	[InlineData("https://subdomain.example.com/page?param=value&param2=value2", false)]
	[InlineData("https://subdomain.example.com/page.html", false)]
	[InlineData("HTTPS://SUBDOMAIN.EXAMPLE.COM/PAGE", false)]
	public void IsBaseOfPage_HandlesVariousUris(string? uriString, bool expected)
	{
		var result = _baseUri.IsBaseOfPage(uriString);
		Assert.Equal(expected, result);
	}

	// Regression test for https://github.com/dotnet/maui/issues/25689
	// A URL with a dot in the query parameter (e.g. ?weight=62.5) must not be
	// treated as a file-extension path and must be allowed to fall back to the host page.
	[Fact]
	public void IsBaseOfPage_DoesNotTreatDotInQueryAsExtension()
	{
		var baseUri = new Uri("https://example.com");
		var urlWithDotInQuery = "https://example.com/customer?weight=62.5";
		Assert.True(baseUri.IsBaseOfPage(urlWithDotInQuery));
	}

	// Regression tests using the actual BlazorWebView platform app origins (https://github.com/dotnet/maui/issues/25689).
	// Android/Windows use https://0.0.0.1/ and iOS/MacCatalyst use app://0.0.0.1/ as the host origin,
	// so the original ?weight=62.5 bug must be verified against those real origins, not just a generic host.
	[Theory]
	[InlineData("https://0.0.0.1/", "https://0.0.0.1/customer?weight=62.5", true)]  // Android/Windows: dot in query
	[InlineData("https://0.0.0.1/", "https://0.0.0.1/customer#section.1", true)]    // Android/Windows: dot in fragment
	[InlineData("https://0.0.0.1/", "https://0.0.0.1/customer.json", false)]        // Android/Windows: real file extension
	[InlineData("app://0.0.0.1/", "app://0.0.0.1/customer?weight=62.5", true)]      // iOS/MacCatalyst: dot in query
	[InlineData("app://0.0.0.1/", "app://0.0.0.1/customer#section.1", true)]        // iOS/MacCatalyst: dot in fragment
	[InlineData("app://0.0.0.1/", "app://0.0.0.1/customer.json", false)]            // iOS/MacCatalyst: real file extension
	public void IsBaseOfPage_HandlesPlatformAppOrigins(string baseUri, string uriString, bool expected)
	{
		var result = new Uri(baseUri).IsBaseOfPage(uriString);
		Assert.Equal(expected, result);
	}
}
