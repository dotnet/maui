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
		/// Retrieves the last item from a <see cref="IList{T}"/> instance.
		/// </summary>
		/// <typeparam name="T">The type of object to be retrieved from <paramref name="self"/>.</typeparam>
		/// <param name="self">The collection to retrieve the last item from.</param>
		/// <returns>An object of type <typeparamref name="T"/> that is the last item in the collection.</returns>
		public static T Last<T>(this IList<T> self) => self[self.Count - 1];
	}
}
