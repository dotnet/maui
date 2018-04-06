using System;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        public static void BeginInvokeOnMainThread(Action action) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
