namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		protected override MauiDatePicker CreateNativeView()
		{
			return new MauiDatePicker();
		}

		[MissingMapper]
		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateFormat(datePicker);
		}

		[MissingMapper]
		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateDate(datePicker);
		}

		[MissingMapper]
		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker) { }

		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.MapFont(datePicker);
		}

		[MissingMapper]
		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker) { }
	}
}
