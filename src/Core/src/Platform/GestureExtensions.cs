using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	public static class GestureExtensions
	{
		public static IEnumerable<T> GetChildGesturesFor<T>(this IEnumerable<IGestureView> views, Func<T, bool>? predicate = null) where T : IGestureRecognizer
		{
			if (views == null)
				yield break;

			if (predicate == null)
				predicate = x => true;

			foreach (var view in views)
				foreach (var item in view.GestureRecognizers)
				{
					if (item is T gesture && predicate(gesture))
						yield return gesture;
				}
		}

		public static IEnumerable<T> GetGesturesFor<T>(this IEnumerable<IGestureRecognizer> gestures, Func<T, bool>? predicate = null) where T : IGestureRecognizer
		{
			if (gestures == null)
				yield break;

			if (predicate == null)
				predicate = x => true;

			foreach (IGestureRecognizer item in new List<IGestureRecognizer>(gestures))
			{
				if (item is T gesture && predicate(gesture))
				{
					yield return gesture;
				}
			}
		}
	}
}