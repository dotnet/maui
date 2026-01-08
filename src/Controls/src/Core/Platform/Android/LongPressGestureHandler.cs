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

			var position = new Point(e.GetX(), e.GetY());

			foreach (var recognizer in LongPressGestureRecognizers())
			{
				// Android's native GestureDetector uses system default long press timeout
				// (typically 400ms from ViewConfiguration.getLongPressTimeout()).
				// The MinimumPressDuration property is ignored on Android due to platform limitations.
				recognizer.SendLongPressed(view, position);
				recognizer.SendLongPressing(view, GestureStatus.Completed, position);
			}
		}
	}
}
