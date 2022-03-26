using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	partial class AppleSignInAuthenticatorImplementation : IAppleSignInAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
