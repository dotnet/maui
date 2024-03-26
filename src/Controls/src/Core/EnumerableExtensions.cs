using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Internals
{
	static class EnumerableExtensions
	{
		public static bool HasChildGesturesFor<T>(this IEnumerable<GestureElement>? elements, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (elements is null)
				return false;

			if (predicate is null)
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

		public static IEnumerable<T> GetChildGesturesFor<T>(this IEnumerable<GestureElement>? elements, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (elements is null)
				yield break;

			if (predicate is null)
				predicate = x => true;

			foreach (var element in elements)
				foreach (var item in element.GestureRecognizers)
				{
					var gesture = item as T;
					if (gesture != null && predicate(gesture))
						yield return gesture;
				}
		}

		/// <remarks>The method makes a defensive copy of the gestures.</remarks>
		public static IEnumerable<T> GetGesturesFor<T>(this IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (gestures is null)
				yield break;

			if (predicate is null)
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

		public static bool HasAnyGesturesFor<T>(this IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (gestures is null)
			{
				return false;
			}

			predicate ??= x => true;

			foreach (IGestureRecognizer item in gestures)
			{
				if (item is T gesture && predicate(gesture))
				{
					return true;
				}
			}

			return false;
		}
	}
}
