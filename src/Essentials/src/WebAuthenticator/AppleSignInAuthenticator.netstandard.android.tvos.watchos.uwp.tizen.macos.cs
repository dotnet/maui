using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public partial class AppleSignInAuthenticatorImplementation : IAppleSignInAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
