using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(IFrameworkElement element)
		{
			if (element?.Handler?.NativeView is not NSObject nativeView)
				return;

			UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, nativeView);
		}
	}
}
