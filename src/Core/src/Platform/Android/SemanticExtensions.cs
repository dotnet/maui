using System;
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(this IFrameworkElement element)
		{
			if (element?.Handler?.NativeView is not View view)
				throw new NullReferenceException("Can't access view from a null handler");

			view.SendAccessibilityEvent(EventTypes.ViewFocused);
		}
	}
}
