#if __IOS__ || MACCATALYST || MONOANDROID || WINDOWS
using PlatformView = Microsoft.Maui.Platform.PlatformTouchGraphicsView;
#elif TIZEN
using PlatformView = Microsoft.Maui.Graphics.Skia.Views.SkiaGraphicsView;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IGraphicsViewHandler : IViewHandler
	{
		new IGraphicsView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}