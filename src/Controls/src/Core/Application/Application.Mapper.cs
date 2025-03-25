#nullable disable
using System;
using Microsoft.Maui.Controls.Compatibility;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Application legacy behaviors
#if ANDROID
			// There is also a mapper on Window for this property since this property is relevant at the window level for
			// Android not the application level
			ApplicationHandler.Mapper.ReplaceMapping<Application, ApplicationHandler>(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName, MapWindowSoftInputModeAdjust);
#endif
		}
	}
}
