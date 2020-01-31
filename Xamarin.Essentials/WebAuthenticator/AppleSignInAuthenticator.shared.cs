using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class AppleSignInAuthenticator
    {
        public static Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInOptions options = null)
            => PlatformAuthenticateAsync(options ?? new AppleSignInOptions());

        public class AppleSignInOptions
        {
            public bool IncludeFullNameScope { get; set; } = false;

            public bool IncludeEmailScope { get; set; } = false;
        }
    }
}
