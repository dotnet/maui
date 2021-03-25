using System;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : AbstractViewHandler<ITimePicker, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }
		public static void MapTime(TimePickerHandler handler, ITimePicker view) { }
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }
		public static void MapFont(TimePickerHandler handler, ITimePicker view) { }
	}
}