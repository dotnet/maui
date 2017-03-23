using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class MotionEventHelper
	{
		VisualElement _element;
		bool _isInViewCell;

		public bool HandleMotionEvent(IViewParent parent)
		{
			if (_isInViewCell || _element.InputTransparent)
			{
				return false;
			}

			var renderer = parent as Platform.DefaultRenderer;
			if (renderer == null)
			{
				return false;
			}

			// Let the container know that we're "fake" handling this event
			renderer.NotifyFakeHandling();

			return true;
		}

		public void UpdateElement(VisualElement element)
		{
			_isInViewCell = false;
			_element = element;

			if (_element == null)
			{
				return;
			}

			// Determine whether this control is inside a ViewCell;
			// we don't fake handle the events because ListView needs them for row selection
			var parent = _element.Parent;
			while (parent != null)
			{
				if (parent is ViewCell)
				{
					_isInViewCell = true;
					break;
				}
				parent = parent.Parent;
			}
		}
	}
}