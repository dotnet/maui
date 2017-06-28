using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	internal static class ViewCellExtensions
	{
		public static bool IsInViewCell(this VisualElement element)
		{
			var parent = element.Parent;
			while (parent != null)
			{
				if (parent is ViewCell)
				{
					return true;
				}
				parent = parent.Parent;
			}

			return false;
		}

		public static void EnsureLongClickCancellation(this AView view, MotionEvent motionEvent, bool handled, VisualElement element)
		{
			if (view.IsDisposed())
			{
				return;
			}

			if (motionEvent.Action == MotionEventActions.Up && handled && view.LongClickable && element.IsInViewCell())
			{
				// In order for long presses/clicks (for opening context menus) to work in a ViewCell 
				// which contains any Clickable Views (e.g., any with TapGestures associated, or Buttons) 
				// the top-level container in the ViewCell has to be LongClickable; unfortunately, Android 
				// cancels a pending long press/click during MotionEventActions.Up, which the View won't
				// get if the gesture listener has already processed it. So when all these conditions are 
				// true, we need to go ahead and send the Up event to the View; if we don't, the context menu will open
				view.OnTouchEvent(motionEvent);
			}
		}
	}
}