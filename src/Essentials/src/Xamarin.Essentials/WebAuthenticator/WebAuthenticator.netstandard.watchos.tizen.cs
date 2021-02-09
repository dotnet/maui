using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class WebAuthenticator
	{
		static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
