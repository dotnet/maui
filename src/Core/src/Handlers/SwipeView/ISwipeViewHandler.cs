#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.SwipeControl;
#elif TIZEN
using PlatformView = ElmSharp.EvasObject;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface ISwipeViewHandler : IViewHandler
	{
		new ISwipeView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
