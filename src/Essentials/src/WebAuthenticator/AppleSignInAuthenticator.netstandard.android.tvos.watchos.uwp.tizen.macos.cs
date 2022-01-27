using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static partial class AppleSignInAuthenticator
	{
		static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Options options) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
