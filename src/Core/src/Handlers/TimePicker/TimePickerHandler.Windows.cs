#nullable enable
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, TimePicker>
	{
		WBrush? _defaultForeground;

		protected override TimePicker CreatePlatformView() => new TimePicker();

		protected override void ConnectHandler(TimePicker nativeView)
		{
			nativeView.TimeChanged += OnControlTimeChanged;
		}

		protected override void DisconnectHandler(TimePicker nativeView)
		{
			nativeView.TimeChanged -= OnControlTimeChanged;
		}

		void SetupDefaults(TimePicker nativeView)
		{
			_defaultForeground = nativeView.Foreground;
		}

		public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(TimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker, handler._defaultForeground);
		}

		void OnControlTimeChanged(object? sender, TimePickerValueChangedEventArgs e)
		{
			if (VirtualView != null)
			{
				VirtualView.Time = e.NewTime;
				VirtualView.InvalidateMeasure();
			}
		}
	}
}