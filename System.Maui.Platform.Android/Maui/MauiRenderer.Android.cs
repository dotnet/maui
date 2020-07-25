using System;
using AView = Android.Views.View;
using Android.Graphics.Drawables;
using System.Maui.Platform.Android;

namespace System.Maui.Platform {
	public partial class MauiRenderer {
		public static void MapPropertyIsEnabled (IMauiRenderer renderer, IView view)
		{
			var nativeView = renderer.NativeView as AView;
			if (nativeView != null)
				nativeView.Enabled = view.IsEnabled;
		}

		public static void MapBackgroundColor (IMauiRenderer renderer, IView view)
		{
			var aview = renderer.NativeView as AView;
			Color backgroundColor = view.BackgroundColor;
			if (backgroundColor.IsDefault)
				aview.Background = null;
			else
				aview.Background = new ColorDrawable { Color = backgroundColor.ToAndroid() };
		}
	}
}
