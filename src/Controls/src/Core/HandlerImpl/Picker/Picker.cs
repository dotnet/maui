#nullable disable
namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static IPropertyMapper<IPicker, PickerHandler> ControlsPickerMapper = new PropertyMapper<Picker, PickerHandler>(PickerHandler.Mapper)
		{
#if IOS
			[PlatformConfiguration.iOSSpecific.Picker.UpdateModeProperty.PropertyName] = MapUpdateMode,
#endif
			[nameof(Picker.ItemsSource)] = (handler, _) => handler.UpdateValue(nameof(IPicker.Items))
		};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Picker legacy behaviors
			PickerHandler.Mapper = ControlsPickerMapper;
		}
	}
}