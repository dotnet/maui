using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.UnitTests.Handlers
{
	[Category(TestCategory.Core)]
	public class HybridWebViewHandlerTests
	{
		[Fact]
		public void HasExpectedHeaders_AcceptsRefererWithinAppOrigin()
		{
			var headers = HeadersWithToken("Referer", $"{HybridWebViewHandler.AppOrigin}home");

			Assert.True(HybridWebViewHandler.HasExpectedHeaders(headers));
		}

		[Fact]
		public void HasExpectedHeaders_RejectsRefererOutsideAppOrigin()
		{
			var appOriginUri = HybridWebViewHandler.AppOriginUri;
			var headers = HeadersWithToken("Referer", $"{appOriginUri.Scheme}://{appOriginUri.Host}.evil.example/home");

			Assert.False(HybridWebViewHandler.HasExpectedHeaders(headers));
		}

		[Fact]
		public void HasExpectedHeaders_RequiresTokenForRoutedReferer()
		{
			var headers = new[]
			{
				new KeyValuePair<string, string>("Referer", $"{HybridWebViewHandler.AppOrigin}home")
			};

			Assert.False(HybridWebViewHandler.HasExpectedHeaders(headers));
		}

		[Fact]
		public void HasExpectedHeaders_AcceptsExactOrigin()
		{
			var headers = HeadersWithToken("Origin", HybridWebViewHandler.AppOrigin.TrimEnd('/'));

			Assert.True(HybridWebViewHandler.HasExpectedHeaders(headers));
		}

		[Fact]
		public void HasExpectedHeaders_RejectsRoutedOrigin()
		{
			var headers = HeadersWithToken("Origin", $"{HybridWebViewHandler.AppOrigin}home");

			Assert.False(HybridWebViewHandler.HasExpectedHeaders(headers));
		}

		static KeyValuePair<string, string>[] HeadersWithToken(string name, string value) =>
			new[]
			{
				new KeyValuePair<string, string>(HybridWebViewHandler.InvokeDotNetTokenHeaderName, HybridWebViewHandler.InvokeDotNetTokenHeaderValue),
				new KeyValuePair<string, string>(name, value)
			};
	}
}
