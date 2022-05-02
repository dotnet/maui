using System;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker) { }
	}
}