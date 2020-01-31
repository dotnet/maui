using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class WebAuthenticator
    {
        public static Task<WebAuthenticatorResult> AuthenticateAsync(Uri url, Uri callbackUrl)
            => PlatformAuthenticateAsync(url, callbackUrl);
    }
}
