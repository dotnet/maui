#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Authentication;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.WebAuthenticator']/Docs" />
	public static class WebAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync'][1]/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
			=> Current.AuthenticateAsync(url, callbackUrl);

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync'][2]/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions)
			=> Current.AuthenticateAsync(webAuthenticatorOptions);

		static IWebAuthenticator Current => Authentication.WebAuthenticator.Default;
	}
}
