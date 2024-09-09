using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Internals
{
	static class EnumerableExtensions
	{
		public static bool HasChildGesturesFor<T>(this IEnumerable<GestureElement>? elements, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (elements is null)
			{
				return false;
			}

			foreach (var element in elements)
			{
				foreach (var item in element.GestureRecognizers)
				{
					if (item is T gesture && (predicate is null || predicate(gesture)))
					{
						return true;
					}
				}
			}

			return false;
		}

		public static IEnumerable<T> GetChildGesturesFor<T>(this IEnumerable<GestureElement>? elements, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (elements is null)
			{
				yield break;
			}

			foreach (var element in elements)
			{
				foreach (var item in element.GestureRecognizers)
				{
					if (item is T gesture && (predicate is null || predicate(gesture)))
					{
						yield return gesture;
					}
				}
			}
		}

		/// <remarks>The method makes a defensive copy of the gestures.</remarks>
		public static IEnumerable<T> GetGesturesFor<T>(this IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (gestures is null)
			{
				yield break;
			}

			foreach (IGestureRecognizer item in new List<IGestureRecognizer>(gestures))
			{
				if (item is T gesture && (predicate is null || predicate(gesture)))
				{
					yield return gesture;
				}
			}
		}

		internal static bool HasAnyGesturesFor<T>(this IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null) where T : GestureRecognizer
			=> FirstGestureOrDefault(gestures, predicate) is not null;

		internal static T? FirstGestureOrDefault<T>(this IEnumerable<IGestureRecognizer>? gestures, Func<T, bool>? predicate = null) where T : GestureRecognizer
		{
			if (gestures is null)
			{
				return null;
			}

			foreach (IGestureRecognizer item in gestures)
			{
				if (item is T gesture && (predicate is null || predicate(gesture)))
				{
					return gesture;
				}
			}

			return null;
		}
	}
}
