#if __MOBILE__
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Maps.iOS;
#else
using Microsoft.Maui.Controls.Compatibility.Maps.MacOS;
#endif

namespace Microsoft.Maui.Controls
{
	public static class FormsMaps
	{
		static bool IsInitialized;
#if __MOBILE__
		static bool? IsiOs8OrNewer;
		static bool? IsiOs9OrNewer;
		static bool? IsiOs10OrNewer;

		internal static bool IsiOs8OrNewer
		{
			get
			{
				if (!IsiOs8OrNewer.HasValue)
					IsiOs8OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);
				return IsiOs8OrNewer.Value;
			}
		}

		internal static bool IsiOs9OrNewer
		{
			get
			{
				if (!IsiOs9OrNewer.HasValue)
					IsiOs9OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(9, 0);
				return IsiOs9OrNewer.Value;
			}
		}

		internal static bool IsiOs10OrNewer
		{
			get
			{
				if (!IsiOs10OrNewer.HasValue)
					IsiOs10OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(10, 0);
				return IsiOs10OrNewer.Value;
			}
		}
#endif
		public static void Init()
		{
			if (IsInitialized)
				return;
			GeocoderBackend.Register();
			IsInitialized = true;
		}
	}
}