#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = AndroidX.SwipeRefreshLayout.Widget.SwipeRefreshLayout;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.RefreshContainer;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IRefreshViewHandler : IViewHandler
	{
		new IRefreshView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}