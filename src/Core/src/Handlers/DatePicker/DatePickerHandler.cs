#if IOS && !MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif MACCATALYST
using PlatformView = UIKit.UIDatePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.CalendarDatePicker;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Entry;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : IDatePickerHandler
	{
		public static IPropertyMapper<IDatePicker, IDatePickerHandler> Mapper = new PropertyMapper<IDatePicker, IDatePickerHandler>(ViewHandler.ViewMapper)
		{
#if ANDROID || WINDOWS
			[nameof(IDatePicker.Background)] = MapBackground,
#elif IOS
			[nameof(IDatePicker.FlowDirection)] = MapFlowDirection,
#endif
			[nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
			[nameof(IDatePicker.Date)] = MapDate,
			[nameof(IDatePicker.Font)] = MapFont,
			[nameof(IDatePicker.Format)] = MapFormat,
			[nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
			[nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
			[nameof(IDatePicker.TextColor)] = MapTextColor,
		};

		public static CommandMapper<IPicker, IDatePickerHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public DatePickerHandler() : base(Mapper, CommandMapper)
		{
		}

		public DatePickerHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public DatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IDatePicker IDatePickerHandler.VirtualView => VirtualView;

		PlatformView IDatePickerHandler.PlatformView => PlatformView;
	}
}