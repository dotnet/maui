using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public class TapGestureHandler
	{
		public TapGestureHandler(Func<IView> getView, Func<IList<IGestureView>> getChildElements)
		{
			GetView = getView;
			GetChildElements = getChildElements;
		}

		Func<IList<IGestureView>> GetChildElements { get; }
		Func<IView> GetView { get; }

		public void OnSingleClick()
		{
			// Only handle click if we don't have double tap registered
			if (TapGestureRecognizers(2).Any())
				return;

			OnTap(1, new Point(-1, -1));
		}

		public bool OnTap(int count, Point point)
		{
			IView view = GetView();

			if (view == null)
				return false;

			var captured = false;

			var children = view.GetChildElements(point);

			if (children != null)
				foreach (var recognizer in children.GetChildGesturesFor<ITapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == count))
				{
					recognizer.Tapped(view);
					captured = true;
				}

			if (captured)
				return captured;

			IEnumerable<ITapGestureRecognizer> gestureRecognizers = TapGestureRecognizers(count);
			foreach (var gestureRecognizer in gestureRecognizers)
			{
				gestureRecognizer.Tapped(view);
				captured = true;
			}

			return captured;
		}

		public bool HasAnyGestures()
		{
			IView view = GetView();

			return view != null && view.GestureRecognizers.OfType<ITapGestureRecognizer>().Any()				
				|| GetChildElements().GetChildGesturesFor<ITapGestureRecognizer>().Any();
		}

		public IEnumerable<ITapGestureRecognizer> TapGestureRecognizers(int count)
		{
			IView view = GetView();

			if (view == null)
				return Enumerable.Empty<ITapGestureRecognizer>();

			return view.GestureRecognizers.GetGesturesFor<ITapGestureRecognizer>(recognizer => recognizer.NumberOfTapsRequired == count);
		}
	}
}