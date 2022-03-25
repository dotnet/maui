using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public partial class AppleSignInAuthenticatorImplementation : IAppleSignInAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
