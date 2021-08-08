using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class WebAuthenticator
	{
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
			=> PlatformAuthenticateAsync(new WebAuthenticatorOptions { Url = url, CallbackUrl = callbackUrl });

		public static Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> PlatformAuthenticateAsync(webAuthenticatorOptions);
	}

	public class WebAuthenticatorOptions
	{
		public Uri Url { get; set; }

		public Uri CallbackUrl { get; set; }

		public bool PrefersEphemeralWebBrowserSession { get; set; }
	}
}
