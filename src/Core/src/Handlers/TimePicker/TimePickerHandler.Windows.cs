#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, TimePicker>
	{
		protected override TimePicker CreatePlatformView() => new TimePicker();

		protected override void ConnectHandler(TimePicker platformView)
		{
			platformView.Loaded += OnControlLoaded;
			platformView.TimeChanged += OnControlTimeChanged;
		}

		protected override void DisconnectHandler(TimePicker platformView)
		{
			platformView.Loaded -= OnControlLoaded;
			platformView.TimeChanged -= OnControlTimeChanged;
		}

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateTime(timePicker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView.UpdateFont(timePicker, fontManager);
		}

		public static void MapFlowDirection(ITimePickerHandler handler, ITimePicker timePicker)
		{
			if (handler.PlatformView is not null)
			{
				handler.PlatformView.UpdateFlowDirection(timePicker);
				handler.PlatformView.UpdateTextAlignment(timePicker);
			}
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView.UpdateTextColor(timePicker);
		}

		// TODO NET8 make public
		internal static void MapBackground(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateBackground(timePicker);
		}

		void OnControlLoaded(object sender, RoutedEventArgs e)
		{
			PlatformView.UpdateTextAlignment(VirtualView);
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