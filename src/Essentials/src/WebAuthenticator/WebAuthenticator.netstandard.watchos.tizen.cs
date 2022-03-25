using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.WebAuthenticator']/Docs" />
	partial class WebAuthenticatorImplementation : IWebAuthenticator
	{
		public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
