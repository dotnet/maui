using System;
#if __IOS__ || MACCATALYST
using UIKit;
using PlatformView = UIKit.UIView;
#elif __ANDROID__
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui
{
	// TODO MARK THIS OBSOLETE FOR NET10 
	// There's already one of these inside the Microsoft.Maui.Platform namespace
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IView element)
		{
			if (element?.Handler?.PlatformView is not PlatformView platformView)
				throw new NullReferenceException("Can't access view from a null handler");

#if __ANDROID__
			platformView.SendAccessibilityEvent(EventTypes.ViewHoverEnter);
#elif __IOS__ || MACCATALYST
			UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, platformView);
#endif
		}
	}
}
