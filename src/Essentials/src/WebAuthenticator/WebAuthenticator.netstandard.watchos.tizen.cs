using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class WebAuthenticator
	{
		static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Uri url, Uri callbackUrl)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
