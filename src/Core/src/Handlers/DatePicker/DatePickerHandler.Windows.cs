#nullable enable
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, CalendarDatePicker>
	{
		protected override CalendarDatePicker CreatePlatformView() => new CalendarDatePicker();

		protected override void ConnectHandler(CalendarDatePicker platformView)
		{
			platformView.DateChanged += DateChanged;
		}

		protected override void DisconnectHandler(CalendarDatePicker platformView)
		{
			platformView.DateChanged -= DateChanged;
		}

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateMinimumDate(datePicker);
		}

		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateMaximumDate(datePicker);
		}

		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateCharacterSpacing(datePicker);
		}

		public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateTextColor(datePicker);
		}

		private void DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
		{
			if (VirtualView == null)
				return;

			if (!args.NewDate.HasValue)
			{
				return;
			}

			// TODO ezhart 2021-06-21 For the moment, IDatePicker requires a date to be selected; once that's fixed, we can uncomment these next lines

			//if (!args.NewDate.HasValue)
			//{
			//	VirtualView.Date = null;
			//	return;
			//}

			//if (VirtualView.Date == null)
			//{
			//	VirtualView.Date = args.NewDate.Value.Date;
			//}

			VirtualView.Date = args.NewDate.Value.Date;
		}

		// TODO NET8 add to public API
		internal static void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateBackground(datePicker);
		}
	}
}