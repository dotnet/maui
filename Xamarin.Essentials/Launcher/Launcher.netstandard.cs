using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) => throw new NotImplementedInReferenceAssemblyException();

        static Task PlatformOpenAsync(Uri uri) => throw new NotImplementedInReferenceAssemblyException();
    }
}
