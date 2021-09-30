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

		private static void MapToolbar(WindowHandler handler, IWindow view)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} null");

			// TODO MAUI: We don't really have a better place to tap into the window handler
			// The factory is on the ViewHandler level not the Element Handler level
			if (handler.NavigationRootManager == null)
			{
				handler.NavigationRootManager = new Platform.ControlsNavigationRootManager(handler.MauiContext);
			}

			if (view is Window window && window.Toolbar != null && window.Toolbar.Handler == null)
			{
				_ = window.Toolbar.ToNative(handler.MauiContext);
			}
		}
	}
}