using Android.App;
using Android.Content.PM;

namespace Microsoft.Maui.Platform
{
	static class Rtl
	{
		/// <summary>
		/// True if /manifest/application@android:supportsRtl="true"
		/// </summary>
		public static readonly bool IsSupported =
			(Application.Context?.ApplicationInfo?.Flags & ApplicationInfoFlags.SupportsRtl) != 0;
	}
}
