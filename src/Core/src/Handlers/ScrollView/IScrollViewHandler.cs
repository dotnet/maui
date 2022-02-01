#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIScrollView;
#elif MONOANDROID
using NativeView = Microsoft.Maui.Platform.MauiScrollView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Controls.ScrollViewer;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface IScrollViewHandler : IViewHandler
	{
		new IScrollView VirtualView { get; }
		new NativeView NativeView { get; }
	}
}