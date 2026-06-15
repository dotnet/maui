#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal class LongPressGestureHandler
	{
		readonly Func<IViewHandler> _handlerGetter;
		readonly Func<View> _viewGetter;

		public LongPressGestureHandler(Func<IViewHandler> handlerGetter, Func<View> viewGetter)
		{
			_handlerGetter = handlerGetter ?? throw new ArgumentNullException(nameof(handlerGetter));
			_viewGetter = viewGetter ?? throw new ArgumentNullException(nameof(viewGetter));
		}

		public bool HasAnyGestures()
		{
			var view = _viewGetter();
			return view?.GestureRecognizers.OfType<LongPressGestureRecognizer>().Any() ?? false;
		}

		public IEnumerable<LongPressGestureRecognizer> LongPressGestureRecognizers()
		{
			var view = _viewGetter();
			return view?.GestureRecognizers.OfType<LongPressGestureRecognizer>() ?? Enumerable.Empty<LongPressGestureRecognizer>();
		}

		public void OnLongPress(MotionEvent e)
		{
			var view = _viewGetter();
			if (view == null)
				return;

			var handler = _handlerGetter();
			var originPoint = new Point(e.GetX(), e.GetY());
			Func<IElement, Point?> getPosition = (relativeTo) => CalculatePosition(relativeTo, originPoint, handler);

			foreach (var recognizer in LongPressGestureRecognizers())
			{
				// Android's native GestureDetector uses system default long press timeout
				// (typically 400ms from ViewConfiguration.getLongPressTimeout()).
				// The MinimumPressDuration property is ignored on Android due to platform limitations.
				recognizer.SendLongPressed(view, getPosition);
				recognizer.SendLongPressing(view, GestureStatus.Completed, getPosition);
			}
		}

		static Point? CalculatePosition(IElement relativeTo, Point originPoint, IViewHandler handler)
		{
			var virtualView = handler?.VirtualView as View;
			if (virtualView == null)
				return null;

			// If relativeTo is null or same as the view, return position relative to the view
			if (relativeTo == null || relativeTo == virtualView)
				return originPoint;

			// Calculate position relative to another element
			var platformView = relativeTo.ToPlatform();
			if (platformView == null)
				return null;

			var targetViewScreenLocation = virtualView.GetLocationOnScreen();
			if (!targetViewScreenLocation.HasValue)
				return null;

			var windowX = targetViewScreenLocation.Value.X + originPoint.X;
			var windowY = targetViewScreenLocation.Value.Y + originPoint.Y;

			var relativeViewLocation = ((View)relativeTo).GetLocationOnScreen();
			if (!relativeViewLocation.HasValue)
				return null;

			return new Point(windowX - relativeViewLocation.Value.X, windowY - relativeViewLocation.Value.Y);
		}
	}
}
