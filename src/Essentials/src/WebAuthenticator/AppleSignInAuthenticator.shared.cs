using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Authentication;

namespace Microsoft.Maui.Authentication
{
	public interface IAppleSignInAuthenticator
	{
		Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions options = null);
	}

	public class AppleSignInAuthenticatorOptions
	{
		public bool IncludeFullNameScope { get; set; } = false;

		public bool IncludeEmailScope { get; set; } = false;
	}
}

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static class AppleSignInAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions options = null)
			=> Current.AuthenticateAsync(options ?? new AppleSignInAuthenticatorOptions());

#nullable enable
		static IAppleSignInAuthenticator? currentImplementation;

		public static IAppleSignInAuthenticator Current =>
			currentImplementation ??= new AppleSignInAuthenticatorImplementation();

		internal static void SetCurrent(IAppleSignInAuthenticator? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
