using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.WebAuthenticator']/Docs" />
	public static partial class WebAuthenticator
	{
		static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
