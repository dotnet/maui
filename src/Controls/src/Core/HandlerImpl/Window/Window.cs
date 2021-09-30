using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		public static IPropertyMapper<IWindow, WindowHandler> ControlsLabelMapper = new PropertyMapper<IWindow, WindowHandler>(WindowHandler.WindowMapper)
		{
#if __ANDROID__
			[nameof(Toolbar)] = MapToolbar
#endif
		};

		public static void RemapForControls()
		{
			WindowHandler.WindowMapper = ControlsLabelMapper;

#if __ANDROID__

			// TODO MAUI: We don't really have a better place to tap into the window handler
			// The factory is on the ViewHandler level not the Element Handler level
			WindowHandler.WindowMapper.PrependToMapping(nameof(IWindow.Content), (handler, view) =>
			{
				if (handler.NavigationRootManager == null)
				{
					handler.NavigationRootManager = new Platform.ControlsNavigationRootManager(handler.MauiContext);
				}
			});
#endif
		}
	}
}
