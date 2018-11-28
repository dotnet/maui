using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task<bool> PlatformOpenAsync(Uri uri, BrowserLaunchMode launchMode) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
