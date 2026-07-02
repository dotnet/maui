using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class WebViewDomainAllowlistTests
	{
		[Fact]
		public void NullAllowedDomainsAllowsAll()
		{
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com", allowedDomains: null));
		}

		[Fact]
		public void EmptyAllowedDomainsAllowsAll()
		{
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com", new List<string>()));
		}

		[Fact]
		public void ExactDomainMatchIsAllowed()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains));
		}

		[Fact]
		public void SubdomainMatchIsAllowed()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://sub.example.com/page", domains));
		}

		[Fact]
		public void DeepSubdomainMatchIsAllowed()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://a.b.c.example.com/page", domains));
		}

		[Fact]
		public void DifferentDomainIsBlocked()
		{
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("https://evil.com/page", domains));
		}

		[Fact]
		public void PartialDomainMatchIsBlocked()
		{
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("https://notexample.com/page", domains));
		}

		[Fact]
		public void DomainMatchIsCaseInsensitive()
		{
			var domains = new List<string> { "Example.COM" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://EXAMPLE.com/page", domains));
		}

		[Fact]
		public void MultipleDomains_FirstMatches()
		{
			var domains = new List<string> { "example.com", "test.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains));
		}

		[Fact]
		public void MultipleDomains_SecondMatches()
		{
			var domains = new List<string> { "example.com", "test.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://test.com/page", domains));
		}

		[Fact]
		public void MultipleDomains_NoneMatch()
		{
			var domains = new List<string> { "example.com", "test.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("https://evil.com/page", domains));
		}

		[Theory]
		[InlineData("data:text/html,<h1>Hello</h1>")]
		[InlineData("about:blank")]
		[InlineData("blob:https://example.com/abc")]
		public void SpecialSchemesAreAlwaysAllowed(string url)
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed(url, domains));
		}

		[Fact]
		public void JavaScriptSchemeIsBlockedWhenAllowlistActive()
		{
			// 'javascript:' navigations execute arbitrary script and are a known allowlist-bypass vector.
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("javascript:void(0)", domains));
		}

		[Fact]
		public void JavaScriptSchemeIsAllowedWhenNoAllowlist()
		{
			// With no allowlist configured, behavior is unchanged (everything is allowed).
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("javascript:void(0)", allowedDomains: null));
		}

		[Fact]
		public void IdnUnicodeAllowlistMatchesUnicodeUrl()
		{
			var domains = new List<string> { "münchen.de" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://münchen.de/page", domains));
		}

		[Fact]
		public void IdnUnicodeAllowlistMatchesPunycodeUrl()
		{
			var domains = new List<string> { "münchen.de" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://xn--mnchen-3ya.de/page", domains));
		}

		[Fact]
		public void IdnPunycodeAllowlistMatchesUnicodeUrl()
		{
			var domains = new List<string> { "xn--mnchen-3ya.de" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://münchen.de/page", domains));
		}

		[Fact]
		public void IdnSubdomainMatchIsAllowed()
		{
			var domains = new List<string> { "münchen.de" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://shop.xn--mnchen-3ya.de/page", domains));
		}

		[Fact]
		public void IdnDifferentDomainIsBlocked()
		{
			var domains = new List<string> { "münchen.de" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("https://evil.de/page", domains));
		}

		[Fact]
		public void AppOriginIsAlwaysAllowed()
		{
			var domains = new List<string> { "example.com" };
			var appOrigin = new Uri("app://0.0.0.0/");
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("app://0.0.0.0/index.html", domains, appOrigin));
		}

		[Fact]
		public void AppOriginSubPathIsAllowed()
		{
			var domains = new List<string> { "example.com" };
			var appOrigin = new Uri("https://0.0.0.1/");
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://0.0.0.1/counter", domains, appOrigin));
		}

		[Fact]
		public void NullUrlIsBlocked()
		{
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed(null, domains));
		}

		[Fact]
		public void EmptyUrlIsBlocked()
		{
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("", domains));
		}

		[Fact]
		public void WhitespaceUrlIsBlocked()
		{
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("   ", domains));
		}

		[Fact]
		public void RelativeUrlIsBlocked()
		{
			var domains = new List<string> { "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("/page/test", domains));
		}

		[Fact]
		public void HttpSchemeIsChecked()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("http://example.com/page", domains));
		}

		[Fact]
		public void FtpSchemeIsChecked()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("ftp://example.com/file", domains));
		}

		[Fact]
		public void EmptyDomainEntryIsSkipped()
		{
			var domains = new List<string> { "", "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains));
		}

		[Fact]
		public void NullDomainEntryIsSkipped()
		{
			var domains = new List<string> { null!, "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains));
		}

		[Fact]
		public void WhitespacePaddedDomainEntryMatches()
		{
			// Entries with surrounding whitespace should be trimmed so they still match.
			var domains = new List<string> { "  example.com  " };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains));
		}

		[Fact]
		public void WhitespaceOnlyDomainEntryIsSkipped()
		{
			// A whitespace-only entry must not accidentally allow everything.
			var domains = new List<string> { "   ", "example.com" };
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("https://evil.com/page", domains));
		}

		[Fact]
		public void UrlWithPortIsChecked()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com:8443/page", domains));
		}

		[Fact]
		public void UrlWithQueryStringIsChecked()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page?q=test", domains));
		}

		[Fact]
		public void UrlWithFragmentIsChecked()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page#section", domains));
		}

		[Fact]
		public void NullAppOriginDoesNotCrash()
		{
			var domains = new List<string> { "example.com" };
			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains, null));
		}

		[Fact]
		public void LargeAllowlistStillMatches()
		{
			// Exercises the snapshot path with a bigger list.
			var domains = new List<string>();
			for (int i = 0; i < 200; i++)
				domains.Add($"domain{i}.com");
			domains.Add("example.com");

			Assert.True(WebViewDomainAllowlist.IsUrlAllowed("https://sub.example.com/page", domains));
			Assert.False(WebViewDomainAllowlist.IsUrlAllowed("https://evil.com/page", domains));
		}

		[Fact]
		public void ConcurrentMutationDoesNotThrow()
		{
			// Platform sub-resource callbacks (e.g. Android ShouldInterceptRequest) run on a background
			// thread while the app may mutate AllowedDomains from another thread. The defensive snapshot
			// must let matching iterate safely; without it, reading the live list throws on the race.
			var domains = new List<string> { "example.com", "test.com", "cdn.example.com" };
			var stop = 0;

			var mutator = Task.Run(() =>
			{
				var n = 0;
				while (Volatile.Read(ref stop) == 0)
				{
					domains.Add($"d{n++}.com");
					if (domains.Count > 4)
						domains.RemoveAt(domains.Count - 1);
				}
			});

			try
			{
				for (int i = 0; i < 50_000; i++)
				{
					// Must not throw regardless of concurrent mutation on the other thread.
					WebViewDomainAllowlist.IsUrlAllowed("https://example.com/page", domains);
					WebViewDomainAllowlist.IsUrlAllowed("https://evil.com/page", domains);
				}
			}
			finally
			{
				Volatile.Write(ref stop, 1);
				mutator.Wait();
			}
		}
	}
}
