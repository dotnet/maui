#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiShapeView;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiShapeView;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Graphics.Win2D.W2DGraphicsView;
#elif TIZEN
using PlatformView = Microsoft.Maui.Platform.MauiShapeView;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IShapeViewHandler : IViewHandler
	{
		new IShapeView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}