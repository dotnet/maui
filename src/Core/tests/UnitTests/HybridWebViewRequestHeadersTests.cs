using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class HybridWebViewRequestHeadersTests
	{
		[Theory]
		[InlineData("/")]
		[InlineData("/?child=opaque")]
		[InlineData("/index.html?child=opaque")]
		[InlineData("/sub/page.html?child=opaque")]
		public void LocalDocumentReferersKeepTheirOriginWhenPathsAndQueriesChange(string document)
		{
			Assert.True(HybridWebViewHandler.HasExpectedHeaders(new Dictionary<string, string>
			{
				["X-Maui-Invoke-Token"] = "HybridWebView",
				["Origin"] = "null",
				["Referer"] = HybridWebViewHandler.AppOrigin.TrimEnd('/') + document
			}));
		}

		[Theory]
		[InlineData("https://example.com/")]
		[InlineData("https://0.0.0.1.example.com/")]
		[InlineData("https://0.0.0.1:444/")]
		[InlineData("https://user@0.0.0.1/")]
		[InlineData("http://0.0.0.1/")]
		[InlineData("/relative")]
		[InlineData("null")]
		public void ForeignOrCredentialedReferersDoNotAuthorizeJsonInvocation(string referer)
		{
			Assert.False(HybridWebViewHandler.HasExpectedHeaders(new Dictionary<string, string>
			{
				["X-Maui-Invoke-Token"] = "HybridWebView",
				["Origin"] = "null",
				["Referer"] = referer
			}));
		}

		[Fact]
		public void LocalRefererStillRequiresTheExactInvocationToken()
		{
			var headers = new Dictionary<string, string> { ["Referer"] = HybridWebViewHandler.AppOrigin + "?child=opaque" };
			Assert.False(HybridWebViewHandler.HasExpectedHeaders(headers));
			headers["X-Maui-Invoke-Token"] = "other";
			Assert.False(HybridWebViewHandler.HasExpectedHeaders(headers));
			headers["X-Maui-Invoke-Token"] = "HybridWebView";
			Assert.True(HybridWebViewHandler.HasExpectedHeaders(headers));
		}
	}
}
