using System;

namespace Microsoft.Caboodle
{
    public static partial class Platform
    {
        public static void BeginInvokeOnMainThread(Action action) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
