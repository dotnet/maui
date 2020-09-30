using System.Collections.Generic;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
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