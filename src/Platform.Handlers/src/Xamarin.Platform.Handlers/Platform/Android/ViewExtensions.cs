using System;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Xamarin.Forms;
using AView = Android.Views.View;

namespace Xamarin.Platform
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this AView nativeView, IView view)
		{			
			if (nativeView != null)
				nativeView.Enabled = view.IsEnabled;
		}
			

		public static void UpdateBackgroundColor(this AView nativeView, IView view)
		{
			var backgroundColor = view.BackgroundColor;
			if (!backgroundColor.IsDefault)
				nativeView?.SetBackgroundColor(backgroundColor.ToNative());
		}
	}
}