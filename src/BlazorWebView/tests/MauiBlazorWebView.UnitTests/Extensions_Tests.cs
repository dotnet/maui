using Microsoft.AspNetCore.Components.WebView.Maui;
namespace MauiBlazorWebView.UnitTests;

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
	[InlineData("https://subdomain.example.com/page", true)] 
	[InlineData("https://example.com/path/with/.dot/segment", true)]
	[InlineData("https://example.com/path/with space", true)]
	[InlineData("https://example.com/path/with%20encoded%20space", true)]
	[InlineData("https://example.com/page?param=value&param2=value2", true)]
	[InlineData("https://example.com/page.html", false)]
	[InlineData("HTTPS://EXAMPLE.COM/PAGE", true)]
	[InlineData("https://subdomain.example.com/", true)]
	[InlineData("https://subdomain.example.com/page/", true)]
	[InlineData("https://subdomain.example.com/page?weight=62.5", true)]
	[InlineData("https://subdomain.example.com/page#section.1", true)]
	[InlineData("https://subdomain.example.com/file.txt", false)]
	[InlineData("https://subdomain.example.com/page.json?foo=bar", false)]
	[InlineData("ftp://subdomain.example.com/page", false)]
	[InlineData("https://subdomain.example.com/test", true)]
	[InlineData("https://subdomain.example.com/folder/subfolder/", true)]
	[InlineData("https://subdomain.example.com/folder/file.exe", false)]
	[InlineData("https://subdomain.example.com/path/with/.dot/segment", true)]
	[InlineData("https://subdomain.example.com/path/with space", true)]
	[InlineData("https://subdomain.example.com/path/with%20encoded%20space", true)]
	[InlineData("https://subdomain.example.com/page?param=value&param2=value2", true)]
	[InlineData("https://subdomain.example.com/page.html", false)]
	[InlineData("HTTPS://SUBDOMAIN.EXAMPLE.COM/PAGE", true)]	
	public void IsBaseOfPage_HandlesVariousUris(string? uriString, bool expected)
	{
		var result = _baseUri.IsBaseOfPage(uriString);
		Assert.Equal(expected, result);
	}

	[Fact]
	public void IsBaseOfPage_ReturnsFalse_WhenPathHasFileExtension()
	{
		var uriWithExtension = "https://example.com/assets/image.png";
		Assert.False(_baseUri.IsBaseOfPage(uriWithExtension));
	}

	[Fact]
	public void IsBaseOfPage_ReturnsTrue_WhenUriIsBaseItself()
	{
		var uri = "https://example.com/";
		Assert.True(_baseUri.IsBaseOfPage(uri));
	}

	[Fact]
	public void IsBaseOfPage_IgnoresDotInQuery_AndReturnsFalse_WhenNotBase()
	{
		var uri = "https://example.com/page?foo=1.2.3";
		Assert.True(_baseUri.IsBaseOfPage(uri));
	}

	[Fact]
	public void IsBaseOfPage_ReturnsFalse_WhenSchemeIsNotHttpOrHttps()
	{
		var uri = "ftp://example.com/page";
		Assert.False(_baseUri.IsBaseOfPage(uri));
	}

	[Fact]
	public void IsBaseOfPage_DoesNotTreatDotInQueryAsExtension()
	{
		var baseUri = new Uri("https://example.com");
		var urlWithDotInQuery = "https://example.com/customer?weight=62.5";
		Assert.True(baseUri.IsBaseOfPage(urlWithDotInQuery));
	}

	[Fact]
	public void IsBaseOfPage_ReturnsFalse_WhenPathHasDotExtension()
	{
		var baseUri = new Uri("https://example.com");
		var urlWithExtension = "https://example.com/customer.json";
		Assert.False(baseUri.IsBaseOfPage(urlWithExtension));
	}

	[Fact]
	public void IsBaseOfPage_TreatsFragmentWithDotAsNoExtension()
	{
		var baseUri = new Uri("https://example.com");
		var urlWithDotInFragment = "https://example.com/customer#section.1";
		Assert.True(baseUri.IsBaseOfPage(urlWithDotInFragment));
	}
}