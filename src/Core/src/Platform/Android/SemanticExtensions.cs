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

			view?.SendAccessibilityEvent(EventTypes.ViewAccessibilityFocused);
		}

		internal static global::Android.Views.View? GetViewForAccessibility(this IFrameworkElement element)
		{
			var handler = element?.Handler;

			if (element is Layout)
				return (View?)handler?.NativeView;
			else if (handler is ViewGroup vg && vg.ChildCount > 0)
				return vg.GetChildAt(0);
			else if (handler != null)
				return (View?)handler?.NativeView;

			return null;
		}

		internal static global::Android.Views.View? GetViewForAccessibility(this IFrameworkElement element, global::Android.Views.View renderer)
		{
			if (renderer == null)
				return element?.GetViewForAccessibility();

			if (element is Layout)
				return renderer;
			else if (renderer is ViewGroup vg && vg.ChildCount > 0)
				return vg.GetChildAt(0);

			return renderer;
		}
	}
}
