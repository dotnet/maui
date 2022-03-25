using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	partial class AppleSignInAuthenticatorImplementation : IAppleSignInAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
