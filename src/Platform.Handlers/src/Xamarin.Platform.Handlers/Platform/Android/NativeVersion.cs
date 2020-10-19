using Android.OS;

namespace Xamarin.Platform
{
	public static partial class NativeVersion
	{
		static readonly BuildVersionCodes BuildVersion = Build.VERSION.SdkInt;

		public static bool IsAtLeast(BuildVersionCodes buildVersionCode)
		{
			return buildVersionCode >= BuildVersion;
		}

		internal static int ApiLevel { get; } = (int)BuildVersion;

		public static bool IsAtLeast(int apiLevel)
		{
			return ApiLevel >= apiLevel;
		}

		public static bool Supports(int nativeApi)
		{
			return IsAtLeast(nativeApi);
		}
	}

	public static class NativeApis
	{
		public const int PowerSaveMode = 21;
		public const int BlendModeColorFilter = 29;
		public const int SeekBarSetMin = 26;
	}
}