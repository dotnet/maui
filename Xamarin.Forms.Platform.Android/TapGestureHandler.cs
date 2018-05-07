using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class TapGestureHandler
	{
		public TapGestureHandler(Func<View> getView, Func<IList<GestureElement>> getChildElements)
		{
			GetView = getView;
			GetChildElements = getChildElements;
		}

		Func<IList<GestureElement>> GetChildElements { get; }
		Func<View> GetView { get; }

		public void OnSingleClick()
		{
			// only handle click if we don't have double tap registered
			if (TapGestureRecognizers(2).Any())
				return;

			OnTap(1, new Point(-1, -1));
		}

		public bool OnTap(int count, Point point)
		{
			View view = GetView();

			if (view == null)
				return false;

			var captured = false;

			var children = view.GetChildElements(point);

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<TapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == count))
				{
					recognizer.SendTapped(view);
					captured = true;
				}

			if (captured)
				return captured;

			IEnumerable<TapGestureRecognizer> gestureRecognizers = TapGestureRecognizers(count);
			foreach (var gestureRecognizer in gestureRecognizers)
			{
				gestureRecognizer.SendTapped(view);
				captured = true;
			}

			return captured;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<TapGestureRecognizer>().Any()
								|| GetChildElements().GetChildGesturesFor<TapGestureRecognizer>().Any();
		}

		public IEnumerable<TapGestureRecognizer> TapGestureRecognizers(int count)
		{
			View view = GetView();
			if (view == null)
				return Enumerable.Empty<TapGestureRecognizer>();

			return view.GestureRecognizers.GetGesturesFor<TapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == count);
		}

	}
}