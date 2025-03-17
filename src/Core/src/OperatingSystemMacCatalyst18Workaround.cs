using System;

namespace Microsoft.Maui.Platform
{
#if PLATFORM
	internal static partial class OperatingSystemMacCatalyst18Workaround
	{
        public static bool IsMacCatalystVersionAtLeast18()
        {
#if !MACCATALYST
            return false;
#else
            // Delete all uses of this once this is merged
            // /https://github.com/xamarin/xamarin-macios/issues/21390


            return Environment.OSVersion.Version.Major >= 18;
#endif
        }
    }
#endif
}