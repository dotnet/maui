#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			// Adjust the mappings to preserve Controls.Application legacy behaviors
#if ANDROID
			// There is also a mapper on Window for this property since this property is relevant at the window level for
			// Android not the application level
			ApplicationHandler.Mapper.ReplaceMappingForControls<Application, ApplicationHandler>(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName, MapWindowSoftInputModeAdjust);
#endif
		}
	}
}
