#nullable enable
#if __IOS__ || MACCATALYST
using NativeView = Microsoft.Maui.Platform.ContentView;
#elif __ANDROID__
using NativeView = Microsoft.Maui.Platform.ContentViewGroup;
#elif WINDOWS
using NativeView = Microsoft.Maui.Platform.ContentPanel;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IBorderHandler : IViewHandler
	{
		new IBorderView VirtualView { get; }
		new NativeView PlatformView { get; }
	}
}