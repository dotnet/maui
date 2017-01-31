using System;

namespace Xamarin.Forms.Platform.Android
{
	public static class VisualElementExtensions
	{
		public static IVisualElementRenderer GetRenderer(this VisualElement self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			IVisualElementRenderer renderer = Platform.GetRenderer(self);

			return renderer;
		}

		public static bool ShouldBeMadeClickable(this View view)
		{
			var shouldBeClickable = false;
			for (var i = 0; i < view.GestureRecognizers.Count; i++)
			{
				IGestureRecognizer gesture = view.GestureRecognizers[i];
				if (gesture is TapGestureRecognizer || gesture is PinchGestureRecognizer || gesture is PanGestureRecognizer)
				{
					shouldBeClickable = true;
					break;
				}
			}

			// do some evil
			// This is required so that a layout only absorbs click events if it is not fully transparent
			// However this is not desirable behavior in a ViewCell because it prevents the ViewCell from activating
			if (view is Layout && view.BackgroundColor != Color.Transparent && view.BackgroundColor != Color.Default)
			{
				Element parent = view.RealParent;
				var skip = false;
				while (parent != null)
				{
					if (parent is ViewCell)
					{
						skip = true;
						break;
					}
					parent = parent.RealParent;
				}

				if (!skip)
					shouldBeClickable = true;
			}

			return shouldBeClickable;
		}
	}
}