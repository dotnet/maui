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

using System.Collections.Generic;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public static class LayoutExtensions
	{
		public static void Add(this IList<View> children, AView view, GetDesiredSizeDelegate getDesiredSizeDelegate = null, OnLayoutDelegate onLayoutDelegate = null,
							   OnMeasureDelegate onMeasureDelegate = null)
		{
			children.Add(view.ToView(getDesiredSizeDelegate, onLayoutDelegate, onMeasureDelegate));
		}

		public static View ToView(this AView view, GetDesiredSizeDelegate getDesiredSizeDelegate = null, OnLayoutDelegate onLayoutDelegate = null, OnMeasureDelegate onMeasureDelegate = null)
		{
			return new NativeViewWrapper(view, getDesiredSizeDelegate, onLayoutDelegate, onMeasureDelegate);
		}
	}
}