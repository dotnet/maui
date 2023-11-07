using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Handlers
{

	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{

		protected override MauiDatePicker CreatePlatformView()
		{
			return new MauiDatePicker();
		}

		[MissingMapper]
		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		[MissingMapper]
		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		[MissingMapper]
		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker) { }

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		[MissingMapper]
		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker) { }

	}

}