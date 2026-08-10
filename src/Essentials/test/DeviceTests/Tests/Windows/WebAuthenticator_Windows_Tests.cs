using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("WebAuthenticator")]
	public class WebAuthenticator_Windows_Tests
	{
		// Pre-canceled authentication should eventually behave consistently across all
		// WebAuthenticator platforms. This regression test is currently Windows-only
		// because the other platforms still start their native browser session.
		[Fact]
		public async Task AuthenticateAsyncWithPreCanceledTokenStopsBeforePlatformValidation()
		{
			var options = new WebAuthenticatorOptions
			{
				Url = new Uri("https://example.com/auth"),
				CallbackUrl = new Uri("https://example.com/callback"),
			};

			var cancellationToken = new CancellationToken(canceled: true);

			var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
				WebAuthenticator.Default.AuthenticateAsync(options, cancellationToken));

			Assert.Equal(cancellationToken, exception.CancellationToken);
		}

		[Theory]
		[InlineData("maui-auth://", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		[InlineData("MAUI-AUTH://", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		[InlineData("maui-auth://callback", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		[InlineData("maui-auth://other/path?code=123", "Microsoft.Maui.WebAuthenticator:maui-auth")]
		public void CreateCallbackRouteKeyUsesSchemeOnly(string callbackUrl, string expected)
		{
			var routeKey = WebAuthenticatorImplementation.CreateCallbackRouteKey(new Uri(callbackUrl));

			Assert.Equal(expected, routeKey);
		}

		[Theory]
		[InlineData(null, true)]
		[InlineData("", true)]
		[InlineData("Maui.App", false)]
		[InlineData("Microsoft.Maui.WebAuthenticator:maui-auth", true)]
		[InlineData("Microsoft.Maui.WebAuthenticator:other-auth", true)]
		[InlineData("Microsoft.Maui.WebAuthenticator2:maui-auth", false)]
		public void CanRegisterCallbackRoutePreservesApplicationKeys(string currentKey, bool expected)
		{
			var actual = WebAuthenticatorImplementation.CanRegisterCallbackRoute(currentKey);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("maui-auth://callback", "maui-auth://callback", true)]
		[InlineData("maui-auth://callback", "maui-auth://other/path", true)]
		[InlineData("MAUI-AUTH://callback", "maui-auth://callback", true)]
		[InlineData("maui-auth://callback", "other-auth://callback", false)]
		public void IsSameCallbackRouteUsesSchemeOnly(string expectedCallbackUrl, string callbackUrl, bool expected)
		{
			var actual = WebAuthenticatorImplementation.IsSameCallbackRoute(
				new Uri(expectedCallbackUrl),
				new Uri(callbackUrl));

			Assert.Equal(expected, actual);
		}
	}
}
