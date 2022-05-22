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
		internal Activity PlatformActivity =>
			(Handler?.PlatformView as Activity) ?? throw new InvalidOperationException("Window should have an Activity set.");

		public static void MapContent(IWindowHandler handler, IWindow view)
		{
			UpdatePlatformContent(handler, view, handler.PlatformView);
		}

		internal static void UpdatePlatformContent(IWindowHandler handler, IWindow view, object addTo)
		{
			if (view.Content is not Shell)
			{
				WindowHandler.MapContent(handler, view);
				return;
			}

			var nativeContent = view.Content.ToContainerView(handler.MauiContext!);

			if (addTo is Activity activity)
			{
				activity.SetContentView(nativeContent);
			}
			else if (addTo is ViewGroup vg)
			{
				vg.RemoveAllViews();
				vg.AddView(nativeContent);
			}

			if (view is Window _)
				handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
		}
	}
}