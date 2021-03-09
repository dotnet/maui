using Android.Views;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	// TODO ezhart We're still going to have to override the relevant methods in the various
	// Android controls to accomplish the touch handling consistency hacks 
	// But we can probably make this class static, make the methods static, and track the 
	// two variables in the control subclasses. That way we don't have to create an instance of this
	// class for every control.

	// Also, we can probably do all that in Forms, too. 

	internal class MotionEventHelper
	{
		VisualElement _element;
		bool _isInViewCell;

		public bool HandleMotionEvent(IViewParent parent, MotionEvent motionEvent)
		{
			if (_isInViewCell || _element == null || motionEvent == null || motionEvent.Action == MotionEventActions.Cancel)
			{
				return false;
			}

			var renderer = parent as AppCompat.Platform.DefaultRenderer;
			if (renderer == null || ShouldPassThroughElement())
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
			_isInViewCell = element.IsInViewCell();
		}

		bool ShouldPassThroughElement()
		{
			if (_element is Layout layout)
			{
				if (!layout.InputTransparent)
				{
					// If the layout is not input transparent, then the event should not pass through it
					return false;
				}

				if (layout.CascadeInputTransparent)
				{
					// This is a layout, and it's transparent, and all its children are transparent, then the event
					// can just pass through 
					return true;
				}

				if (AppCompat.Platform.GetRenderer(_element) is AppCompat.Platform.DefaultRenderer renderer)
				{
					// If the event is being bubbled up from a child which is not inputtransparent, we do not want
					// it to be passed through (just up the tree)
					if (renderer.NotReallyHandled)
					{
						return false;
					}
				}

				// This event isn't being bubbled up by a non-InputTransparent child layout
				return true;
			}

			if (_element.InputTransparent)
			{
				// This is not a layout and it's transparent; the event can just pass through 
				return true;
			}

			return false;
		}
	}
}