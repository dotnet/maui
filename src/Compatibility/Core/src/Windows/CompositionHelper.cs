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

using System;
using System.Linq;
using System.Reflection;
using Windows.UI.Composition;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	static class CompositionHelper
	{
		static bool SetTypePresent;
		static bool IsTypePresent;

		public static bool IsCompositionGeometryTypePresent
		{
			get
			{
				if (!SetTypePresent)
				{
					var compositorMethods = typeof(Compositor).GetMethods();
					var createGeometricClipExists = compositorMethods.Any(m => m.Name == "CreateGeometricClip");
					var createEllipseGeometryExists = compositorMethods.Any(m => m.Name == "CreateEllipseGeometry");

					IsTypePresent = createGeometricClipExists && createEllipseGeometryExists;
					SetTypePresent = true;
				}

				return IsTypePresent;
			}
		}
	}
}
