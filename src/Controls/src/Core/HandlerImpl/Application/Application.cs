namespace Microsoft.Maui.Controls
{
	public partial class Application
	{
		public static IPropertyMapper<IApplication, ApplicationHandler> ControlsApplicationMapper =
			new PropertyMapper<Application, ApplicationHandler>(ApplicationHandler.Mapper)
			{
#if ANDROID
				[PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName] = MapWindowSoftInputModeAdjust,
#endif
			};

		internal static void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Application legacy behaviors
			ApplicationHandler.Mapper = ControlsApplicationMapper;
		}
	}
}
