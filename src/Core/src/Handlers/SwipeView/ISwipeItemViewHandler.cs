#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.ContentView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.ContentViewGroup;
#endif

namespace Microsoft.Maui.Handlers
{
#if __IOS__ || MACCATALYST || MONOANDROID
	public interface ISwipeItemViewHandler : IViewHandler
	{
		new ISwipeItemView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
#endif
}