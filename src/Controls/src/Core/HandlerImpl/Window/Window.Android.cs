#nullable enable
using System;
using Android.App;
using Android.Views;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal Activity NativeActivity =>
			(Handler?.PlatformView as Activity) ?? throw new InvalidOperationException("Window should have an Activity set.");

		public static void MapContent(WindowHandler handler, IWindow view)
		{
			if (view.Content is not Shell)
			{
				WindowHandler.MapContent(handler, view);
				return;
			}

			var nativeContent = view.Content.ToContainerView(handler.MauiContext!);
			handler.PlatformView.SetContentView(nativeContent);

			if (view is Window w)
				handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
		}
	}
}