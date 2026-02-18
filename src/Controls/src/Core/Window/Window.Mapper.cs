#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		static Window()
		{
			// Force Element's static constructor to run first so base-level
			// mapper remappings are applied before these Control-specific ones.
#if DEBUG
			RemappingDebugHelper.AssertBaseClassForRemapping(typeof(Window), typeof(Element));
#endif
			Element.s_forceStaticConstructor = true;

#if ANDROID
			// This property is also on the Application Mapper since that's where the attached property exists
			WindowHandler.Mapper.ReplaceMapping<IWindow, IWindowHandler>(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName, MapWindowSoftInputModeAdjust);
#endif

#if WINDOWS
			WindowHandler.Mapper.PrependToMapping<Window, IWindowHandler>(nameof(ITitledElement.Title), MapTitle);
#endif
		}
	}
}
