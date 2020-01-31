using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class AppleSignInAuthenticator
    {
        static Task<WebAuthenticatorResult> PlatformAuthenticateAsync(AppleSignInOptions options) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
