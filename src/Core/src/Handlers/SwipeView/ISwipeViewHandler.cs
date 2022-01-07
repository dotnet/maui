#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Microsoft.Maui.Platform.MauiSwipeView;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface ISwipeViewHandler : IViewHandler
	{
		ISwipeView TypedVirtualView { get; }
		NativeView TypedNativeView { get; }
	}
}