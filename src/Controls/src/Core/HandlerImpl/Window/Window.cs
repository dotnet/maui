using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		public static IPropertyMapper<IWindow, WindowHandler> ControlsLabelMapper = new PropertyMapper<IWindow, WindowHandler>(WindowHandler.WindowMapper)
		{
#if ANDROID || WINDOWS
			[nameof(IWindow.Content)] = MapContent
#endif
		};

		internal static void RemapForControls()
		{
			WindowHandler.WindowMapper = ControlsLabelMapper;
		}
	}
}
