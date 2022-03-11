using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	internal static class EnumerableExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (T item in enumeration)
			{
				action(item);
			}
		}

		public static IDictionary<TKey, List<TSource>> GroupToDictionary<TSource, TKey>(this IEnumerable<TSource> enumeration, Func<TSource, TKey> func)
			where TKey : notnull
		{
			var result = new Dictionary<TKey, List<TSource>>();
			foreach (TSource item in enumeration)
			{
				var group = func(item);
				if (!result.ContainsKey(group))
					result.Add(group, new List<TSource> { item });
				else
					result[group].Add(item);
			}
			return result;
		}

		public static int IndexOf<T>(this IEnumerable<T> enumerable, T item)
		{
			if (enumerable == null)
				throw new ArgumentNullException("enumerable");

			var i = 0;
			foreach (T element in enumerable)
			{
				if (Equals(element, item))
					return i;

				i++;
			}

			return -1;
		}

		public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate)
		{
			var i = 0;
			foreach (T element in enumerable)
			{
				if (predicate(element))
					return i;

				i++;
			}

			return -1;
		}

		public static T Last<T>(this IList<T> self) => self[self.Count - 1];
	}
}
