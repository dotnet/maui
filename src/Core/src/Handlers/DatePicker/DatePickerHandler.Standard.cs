using System;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker) { }
		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker) { }
		internal static partial void MapIsOpen(IDatePickerHandler handler, IDatePicker datePicker) { }
	}
}