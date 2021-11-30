using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		public static IPropertyMapper<IWindow, WindowHandler> ControlsLabelMapper = new PropertyMapper<IWindow, WindowHandler>(WindowHandler.WindowMapper)
		{
#if __ANDROID__ || WINDOWS
			[nameof(IWindow.Content)] = MapContent,
			[nameof(Toolbar)] = MapToolbar,
#endif
		};

		public static void RemapForControls()
		{
			WindowHandler.WindowMapper = ControlsLabelMapper;
		}


#if ANDROID || WINDOWS
		public static void MapToolbar(WindowHandler handler, IWindow view)
		{
			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(handler.MauiContext)} null");

			if (view is IToolbarElement tb && tb.Toolbar != null)
			{
				_ = tb.Toolbar.ToNative(handler.MauiContext);
			}
		}

		public static void MapContent(WindowHandler handler, IWindow view)
		{
			if (view.Content is not Shell)
			{
				WindowHandler.MapContent(handler, view);
				return;
			}
#if ANDROID
			var nativeContent = view.Content.ToContainerView(handler.MauiContext!);
			handler.NativeView.SetContentView(nativeContent);
#else
			if (handler.NativeView.Content is UI.Xaml.Controls.Panel panel)
			{
				var nativeContent = view.Content.ToNative(handler.MauiContext!);
				panel.Children.Clear();
				panel.Children.Add(nativeContent);

			}
#endif
		}
#endif

	}
}
