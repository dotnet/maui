#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.CalendarDatePicker;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IDatePickerHandler : IViewHandler
	{
		new IDatePicker VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}