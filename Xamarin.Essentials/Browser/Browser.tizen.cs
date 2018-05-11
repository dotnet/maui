using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Browser
    {
        static Task PlatformOpenAsync(Uri uri, BrowserLaunchType launchType) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
