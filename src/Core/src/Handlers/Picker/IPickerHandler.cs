#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiPicker;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiPicker;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ComboBox;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IPickerHandler : IViewHandler
	{
		new IPicker VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}