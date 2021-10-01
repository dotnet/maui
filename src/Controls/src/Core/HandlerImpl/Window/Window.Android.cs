#nullable enable
using System;
using Android.App;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal Activity NativeActivity =>
			(Handler?.NativeView as Activity) ?? throw new InvalidOperationException("Window should have an Activity set.");

		public static void MapToolbar(WindowHandler handler, IWindow view)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} null");

			if (view is Window window && window.Toolbar != null)
			{
				_ = window.Toolbar.ToNative(handler.MauiContext);
			}
		}

		public static void MapContent(WindowHandler handler, IWindow view)
		{
			if(view.Content is not Shell shell)
			{
				WindowHandler.MapContent(handler, view);
				return;
			}

			var nativeContent = view.Content.ToContainerView(handler.MauiContext!);
			handler.NativeView.SetContentView(nativeContent);
		}
	}
}