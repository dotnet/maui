using System;
#if __IOS__ || MACCATALYST
using UIKit;
using NativeView = UIKit.UIView;
#elif __ANDROID__
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IView element)
		{
			if (element?.Handler?.NativeView is not NativeView nativeView)
				throw new NullReferenceException("Can't access view from a null handler");

#if __ANDROID__
			nativeView.SendAccessibilityEvent(EventTypes.ViewFocused);
#elif __IOS__ || MACCATALYST
			UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, nativeView);
#endif
		}
	}
}