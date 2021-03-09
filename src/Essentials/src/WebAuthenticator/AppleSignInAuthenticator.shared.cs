using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class AppleSignInAuthenticator
	{
		public static Task<WebAuthenticatorResult> AuthenticateAsync(Options options = null)
			=> PlatformAuthenticateAsync(options ?? new Options());

		public class Options
		{
			public bool IncludeFullNameScope { get; set; } = false;

			public bool IncludeEmailScope { get; set; } = false;
		}
	}
}
