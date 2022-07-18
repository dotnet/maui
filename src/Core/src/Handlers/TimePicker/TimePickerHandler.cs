#if __IOS__ && !MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTimePicker;
#elif MACCATALYST
using PlatformView = UIKit.UIDatePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiTimePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TimePicker;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Entry;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ITimePickerHandler
	{
		public static IPropertyMapper<ITimePicker, ITimePickerHandler> Mapper = new PropertyMapper<ITimePicker, ITimePickerHandler>(ViewHandler.ViewMapper)
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

		public TimePickerHandler() : base(Mapper)
		{
		}

		public TimePickerHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{
		}

		ITimePicker ITimePickerHandler.VirtualView => VirtualView;

		PlatformView ITimePickerHandler.PlatformView => PlatformView;
	}
}