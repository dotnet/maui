#if __IOS__ && !MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiTimePicker;
#elif MACCATALYST
using PlatformView = UIKit.UIDatePicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiTimePicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TimePicker;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface ITimePickerHandler : IViewHandler
	{
		new ITimePicker VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}