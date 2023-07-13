namespace Microsoft.Maui.Controls
{
	public partial class Switch
	{
		public static IPropertyMapper<ISwitch, SwitchHandler> ControlsSwitchMapper = new PropertyMapper<Switch, SwitchHandler>(SwitchHandler.Mapper)
		{
#if WINDOWS
			[PlatformConfiguration.WindowsSpecific.Switch.ShowStatusLabelProperty.PropertyName] = MapShowStatusLabel,
#endif
		};

		internal static new void RemapForControls()
		{
			// Enable platform-specifics
			SwitchHandler.Mapper = ControlsSwitchMapper;
		}
	}
}
