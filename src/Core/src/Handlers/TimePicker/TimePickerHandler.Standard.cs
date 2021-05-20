using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : WidgetHandler<ITimePicker, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();
		//public override Size GetDesiredSize(double widthConstraint, double heightConstraint) => throw new NotImplementedException();

		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }
		public static void MapTime(TimePickerHandler handler, ITimePicker view) { }
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }
		public static void MapFont(TimePickerHandler handler, ITimePicker view) { }
		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker) { }
	}
}