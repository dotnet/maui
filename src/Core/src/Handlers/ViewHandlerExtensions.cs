#if IOS || MACCATALYST
using NativeView = UIKit.UIView;
#elif ANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		public static NativeView? GetWrappedNativeView(this IViewHandler viewHandler) =>
			viewHandler.VirtualView?.GetWrappedNativeView();
	}
}
