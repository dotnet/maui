namespace Microsoft.Maui.Controls
{
	public partial class Picker
	{
		public static IPropertyMapper<IPicker, PickerHandler> ControlsPickerMapper =
			new PropertyMapper<Picker, PickerHandler>(PickerHandler.Mapper)
			{
				[nameof(Text)] = MapText,
				[nameof(TextTransform)] = MapText,
			};

		internal static new void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Picker legacy behaviors
			PickerHandler.Mapper = ControlsPickerMapper;
		}
	}
}
