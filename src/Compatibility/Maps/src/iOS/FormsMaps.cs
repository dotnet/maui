//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

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