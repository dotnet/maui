#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static class AppleSignInAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions? options = null)
			=> Current.AuthenticateAsync(options ?? new AppleSignInAuthenticatorOptions());

		static IAppleSignInAuthenticator Current => Authentication.AppleSignInAuthenticator.Default;
	}
}
