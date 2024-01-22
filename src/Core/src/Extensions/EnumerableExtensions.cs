using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	internal static class EnumerableExtensions
	{
		/// <summary>
		/// Loops trough each item in <paramref name="enumeration"/> and invokes <paramref name="action"/> on it.
		/// </summary>
		/// <typeparam name="T">The type of object contained in this collection.</typeparam>
		/// <param name="enumeration">The collection to loop through.</param>
		/// <param name="action">The action that is invoked on each item in this collection separately.
		/// The current item of  type <typeparamref name="T"/> will be provided as an input parameter.</param>
		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (T item in enumeration)
			{
				action(item);
			}
		}

		/// <summary>
		/// Groups elements from an IEnumerable based on a specified key selector function and returns a dictionary
		/// where keys are the computed keys and values are lists of elements with the same key.
		/// Used by XAML Hot Reload.
		/// </summary>
		/// <typeparam name="TSource">The type of elements in the input enumeration.</typeparam>
		/// <typeparam name="TKey">The type of keys produced by the key selector function.</typeparam>
		/// <param name="enumeration">The input IEnumerable of elements to be grouped.</param>
		/// <param name="func">A function that extracts a key from an element of the input enumeration.</param>
		/// <returns>A dictionary with keys as computed by the key selector function and values as lists of elements
		/// that share the same key.</returns>
		[Obsolete("Legacy API used in previous versions of XAML Hot Reload. Do not use.")]
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

		/// <summary>
		/// Find the index of a specific item within the collection.
		/// </summary>
		/// <typeparam name="T">The type of object contained in this collection.</typeparam>
		/// <param name="enumerable">The collection in which to look for <paramref name="item"/>.</param>
		/// <param name="item">The object to be located in this collection.</param>
		/// <returns>The index of <paramref name="item"/> in the collection or -1 when the item is not found.</returns>
		/// <exception cref="ArgumentNullException">Throws when <paramref name="enumerable"/> is <see langword="null"/>.</exception>
		public static int IndexOf<T>(this IEnumerable<T> enumerable, T item)
		{
			if (enumerable == null)
				throw new ArgumentNullException(nameof(enumerable));

            if (enumerable is IList<T> list)
                return list.IndexOf(item);

            if (enumerable is T[] array)
                return Array.IndexOf(array, item);

			var i = 0;
			foreach (T element in enumerable)
			{
				if (Equals(element, item))
					return i;

				i++;
			}

			return -1;
		}

		/// <summary>
		/// Find the index for the first occurrence of an item within the collection as matched through the specified predicate.
		/// Used by XAML Hot Reload.
		/// </summary>
		/// <typeparam name="T">The type of object contained in this collection.</typeparam>
		/// <param name="enumerable">The collection in which to look for the item.</param>
		/// <param name="predicate">The predicate used to evaluate each item and see if it matches.
		/// The item currently evaluated of type <typeparamref name="T"/> is provided as an input parameter.
		/// The resulting value should be a <see cref="bool"/> which is <see langword="true"/> if this is the item to match, otherwise <see langword="false"/>.</param>
		/// <returns>The index of the first item to match through <paramref name="predicate"/> in the collection or -1 when no match is not found.</returns>
		[Obsolete("Use IndexOf<T>(IEnumerable<T>, T item) instead.")]
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

		/// <summary>
		/// Retrieves the last item from a <see cref="IList{T}"/> instance.
		/// </summary>
		/// <typeparam name="T">The type of object to be retrieved from <paramref name="self"/>.</typeparam>
		/// <param name="self">The collection to retrieve the last item from.</param>
		/// <returns>An object of type <typeparamref name="T"/> that is the last item in the collection.</returns>
		public static T Last<T>(this IList<T> self) => self[self.Count - 1];
	}
}
