using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
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
			var r = await WebAuthenticator.AuthenticateAsync(
				new Uri($"{urlBase}?access_token={accessToken}&refresh_token={refreshToken}&expires={expires}"),
				new Uri($"{callbackScheme}://"));

			Assert.Equal(accessToken, r?.AccessToken);
			Assert.Equal(refreshToken, r?.RefreshToken);
			Assert.NotNull(r?.ExpiresIn);
			Assert.True(r?.ExpiresIn > DateTime.UtcNow);
		}

		[Theory]
		[InlineData("xamarinessentials://#access_token=blah&refresh_token=blah2&expires=1", "blah", "blah2", "1")]
		[InlineData("xamarinessentials://?access_token=blah&refresh_token=blah2&expires=1", "blah", "blah2", "1")]
		[Trait(Traits.InteractionType, Traits.InteractionTypes.Human)]
		public void ParseQueryString(string url, string accessToken, string refreshToken, string expires)
		{
			var r = WebUtils.ParseQueryString(url);

			Assert.Equal(accessToken, r?["access_token"]);
			Assert.Equal(refreshToken, r?["refresh_token"]);
			Assert.Equal(expires, r?["expires"]);
		}
	}
}
