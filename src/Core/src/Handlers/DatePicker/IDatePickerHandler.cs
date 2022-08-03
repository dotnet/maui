#if IOS && !MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif MACCATALYST
using PlatformView = UIKit.UIDatePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiDatePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.CalendarDatePicker;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.Entry;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
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