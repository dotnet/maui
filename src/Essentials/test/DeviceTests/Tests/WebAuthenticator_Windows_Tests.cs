#if WINDOWS
using System;
using Microsoft.Maui.Authentication;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("WebAuthenticator")]
	public class WebAuthenticator_Windows_Tests
	{
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
#endif
