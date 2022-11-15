using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		[Obsolete("ControlsWindowMapper is obsolete as of .NET 8. Use Mapper instead")]
		public static IPropertyMapper<IWindow, WindowHandler> ControlsWindowMapper =
			new PropertyMapper<IWindow, WindowHandler>(WindowHandler.Mapper);

		// ControlsWindowMapper is incorrectly typed to WindowHandler
		public static IPropertyMapper<IWindow, IWindowHandler> Mapper =
#pragma warning disable CS0618 // Type or member is obsolete
			new PropertyMapper<IWindow, IWindowHandler>(ControlsWindowMapper)
#pragma warning restore CS0618 // Type or member is obsolete
			{
#if ANDROID
				// This property is also on the Application Mapper since that's where the attached property exists				
				//[PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName] = MapWindowSoftInputModeAdjust,
#endif
			};

		internal static void RemapForControls()
		{
			WindowHandler.Mapper = Mapper;
		}
	}
}
