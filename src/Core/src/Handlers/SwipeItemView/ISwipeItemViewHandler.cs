#if IOS || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.ContentCanvas;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public interface ISwipeItemViewHandler : IViewHandler
	{
		new ISwipeItemView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}