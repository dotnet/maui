using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static partial class AppleSignInAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Options options = null)
			=> PlatformAuthenticateAsync(options ?? new Options());

		public class Options
		{
			public bool IncludeFullNameScope { get; set; } = false;

			public bool IncludeEmailScope { get; set; } = false;
		}
	}
}
