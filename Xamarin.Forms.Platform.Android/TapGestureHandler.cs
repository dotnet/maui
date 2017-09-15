using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	internal class TapGestureHandler
	{
		public TapGestureHandler(Func<View> getView)
		{
			GetView = getView;
		}

		Func<View> GetView { get; }

		public void OnSingleClick()
		{
			// only handle click if we don't have double tap registered
			if (TapGestureRecognizers(2).Any())
				return;

			OnTap(1);
		}

		public bool OnTap(int count)
		{
			View view = GetView();

			if (view == null)
				return false;

			IEnumerable<TapGestureRecognizer> gestureRecognizers = TapGestureRecognizers(count);
			var result = false;
			foreach (TapGestureRecognizer gestureRecognizer in gestureRecognizers)
			{
				gestureRecognizer.SendTapped(view);
				result = true;
			}

			return result;
		}

		public bool HasAnyGestures()
		{
			var view = GetView();
			return view != null && view.GestureRecognizers.OfType<TapGestureRecognizer>().Any();
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