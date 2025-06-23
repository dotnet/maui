using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
			var r = await authenticationTask.ConfigureAwait(false);
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
			var r = await authenticationTask.ConfigureAwait(false);
			Assert.Equal(accessToken, r?.AccessToken);
			Assert.Equal(refreshToken, r?.RefreshToken);
			Assert.NotNull(r?.ExpiresIn);
			Assert.True(r?.ExpiresIn > DateTime.UtcNow);
			Assert.Equal(1, responseDecoder.CallCount);
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
		public async Task Redirect_WithCancellation(string urlBase, string callbackScheme, string accessToken, string refreshToken, int expires)
		{
#pragma warning disable CA1416 // Validate platform compatibility: Not supported on Windows
			using var cts = new CancellationTokenSource();
			var authenticationTask = WebAuthenticator.AuthenticateAsync(
				new Uri($"{urlBase}?access_token={accessToken}&refresh_token={refreshToken}&expires={expires}"),
				new Uri($"{callbackScheme}://"),
				cts.Token);
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
		public async Task RedirectWithResponseDecoder_WithCancellation(string urlBase, string callbackScheme, string accessToken, string refreshToken, int expires)
		{
			var responseDecoder = new TestResponseDecoder();
#pragma warning disable CA1416 // Validate platform compatibility: Not supported on Windows
			using var cts = new CancellationTokenSource();
			var authenticationTask = WebAuthenticator.AuthenticateAsync(new WebAuthenticatorOptions
			{
				Url = new Uri($"{urlBase}?access_token={accessToken}&refresh_token={refreshToken}&expires={expires}"),
				CallbackUrl = new Uri($"{callbackScheme}://"),
				ResponseDecoder = responseDecoder
			}, cts.Token);
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
