using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		public static IPropertyMapper<IWindow, WindowHandler> ControlsLabelMapper = new PropertyMapper<IWindow, WindowHandler>(WindowHandler.WindowMapper)
		{
#if WINDOWS
			[nameof(IWindow.Content)] = MapContent,
#endif
		};

		public static void RemapForControls()
		{
			WindowHandler.WindowMapper = ControlsLabelMapper;
		}


#if WINDOWS

		public static void MapContent(WindowHandler handler, IWindow view)
		{
			if (view.Content is not Shell)
			{
				WindowHandler.MapContent(handler, view);
				return;
			}

			if (handler.NativeView.Content is UI.Xaml.Controls.Panel panel)
			{
				var nativeContent = view.Content.ToNative(handler.MauiContext!);
				panel.Children.Clear();
				panel.Children.Add(nativeContent);

			}
		}
#endif

	}
}