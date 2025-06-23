#nullable enable
using Microsoft.UI.Xaml;
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
			platformView.Loaded += OnLoaded;
		}

		protected override void DisconnectHandler(CalendarDatePicker platformView)
		{
			platformView.DateChanged -= DateChanged;
			platformView.Loaded -= OnLoaded;
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateDate(datePicker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateMinimumDate(datePicker);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateMaximumDate(datePicker);
		}

		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView.UpdateCharacterSpacing(datePicker);
		}

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView.UpdateFont(datePicker, fontManager);
		}

		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
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

		void OnLoaded(object sender, RoutedEventArgs e)
		{
			if (VirtualView is not null)
			{
				PlatformView?.UpdateCharacterSpacing(VirtualView);
			}
		}

		public static partial void MapBackground(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateBackground(datePicker);
		}
	}
}