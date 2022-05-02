#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif MONOANDROID
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif WINDOWS
using PlatformView = Microsoft.Maui.Platform.MauiPageControl;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.ElmSharp.IndicatorView;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IIndicatorViewHandler : IViewHandler
	{
		new IIndicatorView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}