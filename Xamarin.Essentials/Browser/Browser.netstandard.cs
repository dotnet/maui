using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task PlatformOpenAsync(Uri uri, BrowserLaunchMode launchMode) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
