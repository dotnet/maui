#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeControl;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface ISwipeViewHandler : IViewHandler
	{
		ISwipeView TypedVirtualView { get; }
		PlatformView TypedPlatformView { get; }
	}
}