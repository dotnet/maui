using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("WebAuthenticator")]
	public class WebAuthenticator_Tests
	{
		[Theory]
		[InlineData(
			"https://xamarin-essentials-auth-sample.azurewebsites.net/redirect",
			"xamarinessentials",
			"testtokenvalue",
			"testrefreshvalue",
			99)]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task Redirect(string urlBase, string callbackScheme, string accessToken, string refreshToken, int expires)
		{
#pragma warning disable CA1416 // Validate platform compatibility: Not supported on Windows
			var authenticationTask = WebAuthenticator.AuthenticateAsync(
				new Uri($"{urlBase}?access_token={accessToken}&refresh_token={refreshToken}&expires={expires}"),
				new Uri($"{callbackScheme}://"));
#pragma warning restore CA1416 // Validate platform compatibility

#if WINDOWS
			var exception = await Assert.ThrowsAsync<PlatformNotSupportedException>(async () => await authenticationTask);
#else
			var r = await authenticationTask;
			Assert.Equal(accessToken, r?.AccessToken);
			Assert.Equal(refreshToken, r?.RefreshToken);
			Assert.NotNull(r?.ExpiresIn);
			Assert.True(r?.ExpiresIn > DateTime.UtcNow);
#endif
		}

		[Theory]
		[InlineData(
			"https://xamarin-essentials-auth-sample.azurewebsites.net/redirect",
			"xamarinessentials",
			"testtokenvalue",
			"testrefreshvalue",
			99)]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public async Task RedirectWithResponseDecoder(string urlBase, string callbackScheme, string accessToken, string refreshToken, int expires)
		{
			var responseDecoder = new TestResponseDecoder();
#pragma warning disable CA1416 // Validate platform compatibility: Not supported on Windows
			var authenticationTask = WebAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions
			{
				Url = new Uri($"{urlBase}?access_token={accessToken}&refresh_token={refreshToken}&expires={expires}"),
				CallbackUrl = new Uri($"{callbackScheme}://"),
				ResponseDecoder = responseDecoder
			});
#pragma warning restore CA1416 // Validate platform compatibility
#if WINDOWS
			var exception = await Assert.ThrowsAsync<PlatformNotSupportedException>(async () => await authenticationTask);
#else
			var r = await authenticationTask;
			Assert.Equal(accessToken, r?.AccessToken);
			Assert.Equal(refreshToken, r?.RefreshToken);
			Assert.NotNull(r?.ExpiresIn);
			Assert.True(r?.ExpiresIn > DateTime.UtcNow);
			Assert.Equal(1, responseDecoder.CallCount);
#endif
		}

		[Theory]
		[InlineData("xamarinessentials://#access_token=blah&refresh_token=blah2&expires=1", "blah", "blah2", "1")]
		[InlineData("xamarinessentials://?access_token=blah&refresh_token=blah2&expires=1", "blah", "blah2", "1")]
		[InlineData("xamarinessentials://?access_token=access+token+with+spaces&refresh_token=refresh%20token%20with%20spaces&expires=1", "access token with spaces", "refresh token with spaces", "1")]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public void ParseQueryString(string url, string accessToken, string refreshToken, string expires)
		{
			var r = WebUtils.ParseQueryString(new Uri(url));

			Assert.Equal(accessToken, r?["access_token"]);
			Assert.Equal(refreshToken, r?["refresh_token"]);
			Assert.Equal(expires, r?["expires"]);
		}

		internal class TestResponseDecoder : IWebAuthenticatorResponseDecoder
		{
			internal int CallCount = 0;

			public IDictionary<string, string> DecodeResponse(Uri uri)
			{
				CallCount++;
				return WebUtils.ParseQueryString(uri);
			}
		}
	}
}
