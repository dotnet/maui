using System;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, Microsoft.UI.Xaml.Controls.TimePicker>
	{
		protected override Microsoft.UI.Xaml.Controls.TimePicker CreateNativeView() => new Microsoft.UI.Xaml.Controls.TimePicker();

		[MissingMapper]
		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapTime(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapFont(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapForeground(TimePickerHandler handler, ITimePicker timePicker) { }
	}
}