#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIPageControl;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
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