#nullable disable
using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PanGestureHandler
	{
		Func<double, double> PixelTranslation
		{
			get
			{
				return (input) =>
				{
					var context = GetView()?.Handler?.MauiContext?.Context;
					if (context == null)
						return 0;

					return context.FromPixels(input);
				};
			}
		}

		public PanGestureHandler(Func<View> getView)
		{
			GetView = getView;
		}

		Func<View> GetView { get; }

		public bool OnPan(float x, float y, int pointerCount)
		{
			View view = GetView();

			if (view == null)
				return false;

			var result = false;
			foreach (PanGestureRecognizer panGesture in
				view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>(g => g.TouchPoints == pointerCount))
			{
				((IPanGestureController)panGesture).SendPan(view, PixelTranslation(x), PixelTranslation(y), PanGestureRecognizer.CurrentId.Value);
				result = true;
			}

			return result;
		}

		public bool OnPanComplete()
		{
			View view = GetView();

			if (view == null)
				return false;

			var result = false;
			foreach (PanGestureRecognizer panGesture in view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>())
			{
				((IPanGestureController)panGesture).SendPanCompleted(view, PanGestureRecognizer.CurrentId.Value);
				result = true;
			}
			PanGestureRecognizer.CurrentId.Increment();
			return result;
		}

		public bool OnPanStarted(int pointerCount)
		{
			View view = GetView();

			if (view == null)
				return false;

			var result = false;
			foreach (PanGestureRecognizer panGesture in
				view.GestureRecognizers.GetGesturesFor<PanGestureRecognizer>(g => g.TouchPoints == pointerCount))
			{
				((IPanGestureController)panGesture).SendPanStarted(view, PanGestureRecognizer.CurrentId.Value);
				result = true;
			}
			return result;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<PanGestureRecognizer>().Any();
		}
	}
}