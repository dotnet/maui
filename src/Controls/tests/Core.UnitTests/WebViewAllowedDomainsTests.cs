using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class WebViewAllowedDomainsTests : BaseTestFixture
	{
		[Fact]
		public void AllowedDomainsDefaultIsNull()
		{
			var webView = new WebView();
			Assert.Null(webView.AllowedDomains);
		}

		[Fact]
		public void AllowedDomainsCanBeSet()
		{
			var webView = new WebView();
			var domains = new List<string> { "example.com", "test.com" };
			webView.AllowedDomains = domains;
			Assert.Equal(domains, webView.AllowedDomains);
		}

		[Fact]
		public void AllowedDomainsCanBeSetToNull()
		{
			var webView = new WebView();
			webView.AllowedDomains = new List<string> { "example.com" };
			webView.AllowedDomains = null;
			Assert.Null(webView.AllowedDomains);
		}

		[Fact]
		public void AllowedDomainsPropertyChangedFires()
		{
			var webView = new WebView();
			bool signaled = false;
			webView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == nameof(WebView.AllowedDomains))
					signaled = true;
			};
			Assert.False(signaled);
			webView.AllowedDomains = new List<string> { "example.com" };
			Assert.True(signaled);
		}

		[Fact]
		public void AllowedDomainsBindablePropertyExists()
		{
			Assert.NotNull(WebView.AllowedDomainsProperty);
			Assert.Equal(nameof(WebView.AllowedDomains), WebView.AllowedDomainsProperty.PropertyName);
		}

		[Fact]
		public void HybridWebViewAllowedDomainsDefaultIsNull()
		{
			var hybridWebView = new HybridWebView();
			Assert.Null(hybridWebView.AllowedDomains);
		}

		[Fact]
		public void HybridWebViewAllowedDomainsCanBeSet()
		{
			var hybridWebView = new HybridWebView();
			var domains = new List<string> { "example.com", "test.com" };
			hybridWebView.AllowedDomains = domains;
			Assert.Equal(domains, hybridWebView.AllowedDomains);
		}

		[Fact]
		public void HybridWebViewAllowedDomainsPropertyChangedFires()
		{
			var hybridWebView = new HybridWebView();
			bool signaled = false;
			hybridWebView.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == nameof(HybridWebView.AllowedDomains))
					signaled = true;
			};
			Assert.False(signaled);
			hybridWebView.AllowedDomains = new List<string> { "example.com" };
			Assert.True(signaled);
		}

		[Fact]
		public void HybridWebViewAllowedDomainsBindablePropertyExists()
		{
			Assert.NotNull(HybridWebView.AllowedDomainsProperty);
			Assert.Equal(nameof(HybridWebView.AllowedDomains), HybridWebView.AllowedDomainsProperty.PropertyName);
		}
	}
}
