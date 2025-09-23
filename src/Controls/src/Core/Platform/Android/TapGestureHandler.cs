using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{
	internal class TapGestureHandler
	{
		public TapGestureHandler(Func<View?> getView, Func<IList<GestureElement>> getChildElements)
		{
			GetView = getView;
			GetChildElements = getChildElements;
		}

		Func<IList<GestureElement>> GetChildElements { get; }
		Func<View?> GetView { get; }

		public void OnSingleClick()
		{
			// only handle click if we don't have double tap registered
			if (TapGestureRecognizers(2).Any())
				return;

			OnTap(1, null);
		}

		public bool OnTap(int count, MotionEvent? e)
		{
			Point point;

			if (e == null)
				point = new Point(-1, -1);
			else
				point = new Point(e.GetX(), e.GetY());

			var view = GetView();

			if (view == null)
				return false;

			var captured = false;

			var children = view.GetChildElements(point);

			if (children != null)
			{
				foreach (var recognizer in children.GetChildGesturesFor<TapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == count))
				{
					if (!CheckButtonMask(recognizer, e))
						continue;

					recognizer.SendTapped(view, (view) => e.CalculatePosition(GetView(), view));
					captured = true;
				}
			}

			if (captured)
				return captured;

			IEnumerable<TapGestureRecognizer> gestureRecognizers = TapGestureRecognizers(count);
			foreach (var gestureRecognizer in gestureRecognizers)
			{
				if (!CheckButtonMask(gestureRecognizer, e))
					continue;

				gestureRecognizer.SendTapped(view, (view) => e.CalculatePosition(GetView(), view));
				captured = true;
			}

			return captured;

			bool CheckButtonMask(TapGestureRecognizer tapGestureRecognizer, MotionEvent? motionEvent)
			{
				if (tapGestureRecognizer.Buttons == ButtonsMask.Secondary)
				{
					var buttonState = motionEvent?.ButtonState ?? MotionEventButtonState.Primary;

					return
						buttonState == MotionEventButtonState.Secondary ||
						buttonState == MotionEventButtonState.StylusSecondary;
				}

				return (tapGestureRecognizer.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
			}
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<TapGestureRecognizer>().Any()
								|| GetChildElements().GetChildGesturesFor<TapGestureRecognizer>().Any();
		}

		public IEnumerable<TapGestureRecognizer> TapGestureRecognizers(int count)
		{
			var view = GetView();
			if (view == null)
				return Enumerable.Empty<TapGestureRecognizer>();

			return view.GestureRecognizers.GetGesturesFor<TapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == count);
		}

		internal bool HasMultiTapGestureRecognizers()
		{
			var view = GetView();
			if (view == null)
				return false;
				
			// Check direct gesture recognizers on the view
			var hasMultiTap = view.GestureRecognizers?.OfType<TapGestureRecognizer>()
				.Any(r => r.NumberOfTapsRequired > 2) == true;
			
			if (hasMultiTap)
				return true;
				
			// Check child elements' gesture recognizers
			var children = GetChildElements();
			return children?.GetChildGesturesFor<TapGestureRecognizer>()
				.Any(r => r.NumberOfTapsRequired > 2) == true;
		}

	}
}