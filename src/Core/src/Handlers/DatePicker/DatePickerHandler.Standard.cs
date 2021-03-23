using System;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : AbstractViewHandler<IDatePicker, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker) { }
		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker) { }
	}
}