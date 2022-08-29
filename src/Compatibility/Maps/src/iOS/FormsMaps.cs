#if __MOBILE__
using Microsoft.Maui.Controls.Compatibility.Maps.iOS;
#else
using Microsoft.Maui.Controls.Compatibility.Maps.MacOS;
#endif

namespace Microsoft.Maui.Controls
{
	public static class FormsMaps
	{
		static bool s_isInitialized;

		public static void Init()
		{
			if (s_isInitialized)
				return;

			s_isInitialized = true;
		}
	}
}