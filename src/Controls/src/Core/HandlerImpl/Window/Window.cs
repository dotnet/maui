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
		}
	}
}
