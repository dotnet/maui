using System;
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;

namespace Microsoft.Maui
{
	public static partial class SemanticExtensions
	{
		public static void SetSemanticFocus(IFrameworkElement element)
		{
			var view = element.GetViewForAccessibility();

			view?.SendAccessibilityEvent(EventTypes.ViewFocused);
		}

		internal static View? GetViewForAccessibility(this IFrameworkElement element)
		{
			var handler = element?.Handler;

			if (handler == null)
				throw new NullReferenceException("Can't access view from a null handler");

			if (element is Layout)
				return handler.NativeView as View;
			else if (handler is ViewGroup vg && vg.ChildCount > 0)
				return vg.GetChildAt(0);
			else
				return handler.NativeView as View;
		}
	}
}
