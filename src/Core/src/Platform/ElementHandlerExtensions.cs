#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif ANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
# endif

namespace Microsoft.Maui.Platform
{
	public static class ElementHandlerExtensions
	{
		public static bool CanUpdateProperty(this IElementHandler viewHandler)
		{
			var platformView = viewHandler?.PlatformView;

			if (platformView == null)
				return false;

#if ANDROID
			if(platformView is PlatformView androidView && androidView.IsDisposed())
				return false;
#endif
			return true;
		}
	}
}