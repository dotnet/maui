#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
#if ANDROID
			// This property is also on the Application Mapper since that's where the attached property exists
			WindowHandler.Mapper.ReplaceMappingForControls<IWindow, IWindowHandler>(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName, MapWindowSoftInputModeAdjust);
#endif

#if WINDOWS
			WindowHandler.Mapper.PrependToMappingForControls<Window, IWindowHandler>(nameof(ITitledElement.Title), MapTitle);
#endif
		}
	}
}
