using System;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests;

[Category("WebAuthenticator")]
public class WebUtils_Tests
{
	[Theory]
	[InlineData("xamarinessentials://#access_token=blah&refresh_token=blah2&expires=1", "blah", "blah2", "1")]
	[InlineData("xamarinessentials://?access_token=blah&refresh_token=blah2&expires=1", "blah", "blah2", "1")]
	[InlineData("xamarinessentials://?access_token=access+token+with+spaces&refresh_token=refresh%20token%20with%20spaces&expires=1", "access token with spaces", "refresh token with spaces", "1")]
	public void ParseQueryString(string url, string accessToken, string refreshToken, string expires)
	{
		var r = WebUtils.ParseQueryString(new Uri(url));

		Assert.Equal(accessToken, r?["access_token"]);
		Assert.Equal(refreshToken, r?["refresh_token"]);
		Assert.Equal(expires, r?["expires"]);
	}
}
