using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		public static IPropertyMapper<IWindow, WindowHandler> ControlsWindowMapper =
			new PropertyMapper<IWindow, WindowHandler>(WindowHandler.Mapper)
			{
#if ANDROID
				// This property is also on the Application Mapper since that's where the attached property exists				
				[PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName] = MapWindowSoftInputModeAdjust,
#endif
			};

		internal static void RemapForControls()
		{
			WindowHandler.Mapper = ControlsWindowMapper;
		}
	}
}
