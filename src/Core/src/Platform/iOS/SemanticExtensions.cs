using Foundation;
using UIKit;
using System;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IFrameworkElement element)
		{
			if (element?.Handler?.NativeView == null)
				throw new NullReferenceException("Can't access view from a null handler");

			if (element.Handler.NativeView is not NSObject nativeView)
				return;

			UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, nativeView);
		}
	}
}
