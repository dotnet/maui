#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		[Obsolete("Use ApplicationHandler.Mapper instead.")]
		public static IPropertyMapper<IApplication, ApplicationHandler> ControlsApplicationMapper =
			new PropertyMapper<Application, ApplicationHandler>(ApplicationHandler.Mapper);

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
