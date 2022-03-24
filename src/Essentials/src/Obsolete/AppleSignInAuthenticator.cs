#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static partial class AppleSignInAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		[Obsolete($"Use {nameof(AppleSignInAuthenticator)}.{nameof(Default)} instead.", true)]
		public static Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions? options = null)
			=> Default.AuthenticateAsync(options ?? new AppleSignInAuthenticatorOptions());
	}
}
