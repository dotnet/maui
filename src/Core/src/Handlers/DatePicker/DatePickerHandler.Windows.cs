using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, DatePicker>
	{
		protected override DatePicker CreateNativeView() => new DatePicker();

		[MissingMapper]
		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker) { }
	}
}