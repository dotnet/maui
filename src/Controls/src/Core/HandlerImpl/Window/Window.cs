using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		public static IPropertyMapper<IWindow, WindowHandler> ControlsWindowMapper = new PropertyMapper<IWindow, WindowHandler>(WindowHandler.Mapper);

		internal static void RemapForControls()
		{
			WindowHandler.Mapper = ControlsWindowMapper;
		}
	}
}
