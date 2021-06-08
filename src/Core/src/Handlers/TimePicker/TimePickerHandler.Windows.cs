using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, TimePicker>
	{
		WBrush? _defaultForeground;

		protected override TimePicker CreateNativeView() => new TimePicker();

		protected override void SetupDefaults(TimePicker nativeView)
		{
			_defaultForeground = nativeView.Foreground;

			base.SetupDefaults(nativeView);
		}

		[MissingMapper]
		public static void MapFormat(TimePickerHandler handler, ITimePicker view) { }

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.NativeView?.UpdateTime(timePicker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker view) { }

		[MissingMapper]
		public static void MapFont(TimePickerHandler handler, ITimePicker view) { }

		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.NativeView?.UpdateTextColor(timePicker, handler._defaultForeground);
		}
	}
}