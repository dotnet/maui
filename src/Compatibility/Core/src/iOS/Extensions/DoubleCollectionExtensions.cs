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
using ObjCRuntime;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class DoubleCollectionExtensions
	{
		public static nfloat[] ToArray(this DoubleCollection doubleCollection)
		{
			if (doubleCollection == null || doubleCollection.Count == 0)
				return new nfloat[0];
			else
			{

				nfloat[] array = new nfloat[doubleCollection.Count];

				for (int i = 0; i < doubleCollection.Count; i++)
					array[i] = (nfloat)doubleCollection[i];

				return array;
			}
		}
	}
}