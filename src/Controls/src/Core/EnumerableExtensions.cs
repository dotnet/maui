using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	static class EnumerableExtensions
	{
		public static bool HasChildGesturesFor<T>(this IEnumerable<GestureElement> elements, Func<T, bool> predicate = null) where T : GestureRecognizer
		{
			if (elements == null)
				return false;

			if (predicate == null)
				predicate = x => true;

			foreach (var element in elements)
				foreach (var item in element.GestureRecognizers)
				{
					var gesture = item as T;
					if (gesture != null && predicate(gesture))
						return true;
				}

			return false;
		}

		public static IEnumerable<T> GetChildGesturesFor<T>(this IEnumerable<GestureElement> elements, Func<T, bool> predicate = null) where T : GestureRecognizer
		{
			if (elements == null)
				yield break;

			if (predicate == null)
				predicate = x => true;

			foreach (var element in elements)
				foreach (var item in element.GestureRecognizers)
				{
					var gesture = item as T;
					if (gesture != null && predicate(gesture))
						yield return gesture;
				}
		}

		public static IEnumerable<T> GetGesturesFor<T>(this IEnumerable<IGestureRecognizer> gestures, Func<T, bool> predicate = null) where T : GestureRecognizer
		{
			if (gestures == null)
				yield break;

			if (predicate == null)
				predicate = x => true;

			foreach (IGestureRecognizer item in new List<IGestureRecognizer>(gestures))
			{
				var gesture = item as T;
				if (gesture != null && predicate(gesture))
				{
					yield return gesture;
				}
			}
		}
	}
}
