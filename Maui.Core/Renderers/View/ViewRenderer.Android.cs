using System;
using AView = Android.Views.View;
using Android.Graphics.Drawables;

namespace System.Maui.Platform {
	public partial class ViewRenderer {
		public static void MapPropertyIsEnabled (IViewRenderer renderer, IView view)
		{
			var nativeView = renderer.NativeView as AView;
			if (nativeView != null)
				nativeView.Enabled = view.IsEnabled;
		}
		public static void MapBackgroundColor (IViewRenderer renderer, IView view)
		{
			var aview = renderer.NativeView as AView;
			var backgroundColor = view.BackgroundColor;
			if (backgroundColor.IsDefault)
				aview.Background = null;
			else
				aview.Background = new ColorDrawable { Color = backgroundColor.ToNative() };
		}
	}
}
