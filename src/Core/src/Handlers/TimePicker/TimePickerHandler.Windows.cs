using System;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : AbstractViewHandler<ITimePicker, Microsoft.UI.Xaml.Controls.TimePicker>
	{
		protected override Microsoft.UI.Xaml.Controls.TimePicker CreateNativeView() => new Microsoft.UI.Xaml.Controls.TimePicker();

		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }
		public static void MapTime(TimePickerHandler handler, ITimePicker view) { }
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }
	}
}