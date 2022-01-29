using System;
using Android.OS;

namespace Microsoft.Maui.Platform
{
	public static partial class NativeVersion
	{
		public static bool IsAtLeast(BuildVersionCodes buildVersionCode) => OperatingSystem.IsAndroidVersionAtLeast((int)buildVersionCode);

		public static bool IsAtLeast(int apiLevel) => OperatingSystem.IsAndroidVersionAtLeast(apiLevel);

		public static bool Supports(int nativeApi) => OperatingSystem.IsAndroidVersionAtLeast(nativeApi);
	}

	public static class NativeApis
	{
		public const int BlendModeColorFilter = 29;
		public const int SeekBarSetMin = 26;
		public const int LaunchAdjacent = 24;
	}
}