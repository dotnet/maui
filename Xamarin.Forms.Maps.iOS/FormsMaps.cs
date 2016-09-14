using UIKit;
using Xamarin.Forms.Maps.iOS;

namespace Xamarin
{
	public static class FormsMaps
	{
		static bool s_isInitialized;
		static bool? s_isiOs8OrNewer;

		internal static bool IsiOs8OrNewer
		{
			get
			{
				if (!s_isiOs8OrNewer.HasValue)
					s_isiOs8OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);
				return s_isiOs8OrNewer.Value;
			}
		}

		public static void Init()
		{
			if (s_isInitialized)
				return;
			GeocoderBackend.Register();
			s_isInitialized = true;
		}
	}
}