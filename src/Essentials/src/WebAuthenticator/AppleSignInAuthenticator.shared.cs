using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IAppleSignInAuthenticator
	{
		Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options options = null);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static class AppleSignInAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Options options = null)
			=> Current.AuthenticateAsync(options ?? new AppleSignInAuthenticator.Options());

		public class Options
		{
			public bool IncludeFullNameScope { get; set; } = false;

			public bool IncludeEmailScope { get; set; } = false;
		}

#nullable enable
		static IAppleSignInAuthenticator? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IAppleSignInAuthenticator Current =>
			currentImplementation ??= new AppleSignInAuthenticatorImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IAppleSignInAuthenticator? implementation) =>
			currentImplementation = implementation;
#nullable disable
	}
}
