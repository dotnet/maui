using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class AppleSignInAuthenticator
	{
		static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Options options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
