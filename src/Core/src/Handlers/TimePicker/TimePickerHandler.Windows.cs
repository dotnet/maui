using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, TimePicker>
	{
		protected override TimePicker CreateNativeView() => new TimePicker();

		[MissingMapper]
		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.NativeView?.UpdateTime(timePicker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapFont(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker) { }
	}
}