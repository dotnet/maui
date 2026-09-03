using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	partial class WebAuthenticatorImplementation : IWebAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
		{
			throw new PlatformNotSupportedException("This implementation of WebAuthenticator does not support Windows. See https://github.com/microsoft/WindowsAppSDK/issues/441 for more details.");
		}
		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions, CancellationToken cancellationToken)
		{
			throw new PlatformNotSupportedException("This implementation of WebAuthenticator does not support Windows. See https://github.com/microsoft/WindowsAppSDK/issues/441 for more details.");
		}
	}
}
