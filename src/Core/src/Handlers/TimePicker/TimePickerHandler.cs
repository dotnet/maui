#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTimePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiTimePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TimePicker;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ITimePickerHandler
	{
		public static IPropertyMapper<ITimePicker, ITimePickerHandler> TimePickerMapper = new PropertyMapper<ITimePicker, ITimePickerHandler>(ViewHandler.ViewMapper)
		{
#if __ANDROID__
			[nameof(ITimePicker.Background)] = MapBackground,
#elif __IOS__
			[nameof(ITimePicker.FlowDirection)] = MapFlowDirection,
#endif
			[nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(ITimePicker.Font)] = MapFont,
			[nameof(ITimePicker.Format)] = MapFormat,
			[nameof(ITimePicker.TextColor)] = MapTextColor,
			[nameof(ITimePicker.Time)] = MapTime,
		};

		public static CommandMapper<ITimePicker, ITimePickerHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public TimePickerHandler() : base(TimePickerMapper)
		{
		}

		public TimePickerHandler(IPropertyMapper mapper) : base(mapper ?? TimePickerMapper)
		{
		}

		ITimePicker ITimePickerHandler.VirtualView => VirtualView;

		PlatformView ITimePickerHandler.PlatformView => PlatformView;
	}
}