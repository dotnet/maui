using System;

namespace Microsoft.Maui
{
	internal static class ArrayExtensions
	{
		/// <summary>
		/// Inserts an object at a certain index of an array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="self">The array to insert the item in.</param>
		/// <param name="index">The index where <paramref name="item"/> needs to be inserted.</param>
		/// <param name="item">The item of type <typeparamref name="T"/> to be inserted.</param>
		/// <returns>A new array of type <typeparamref name="T"/> with <paramref name="item"/> inserted at the specified index.</returns>
		public static T[] Insert<T>(this T[] self, int index, T item)
		{
			var result = new T[self.Length + 1];
			if (index > 0)
				Array.Copy(self, result, index);

			result[index] = item;

			if (index < self.Length)
				Array.Copy(self, index, result, index + 1, result.Length - index - 1);

			return result;
		}

		/// <summary>
		/// Removes an item from an array by specifying the object.
		/// </summary>
		/// <typeparam name="T">The type of object contained in this array.</typeparam>
		/// <param name="self">The array to remove the item from.</param>
		/// <param name="item">The item of type <typeparamref name="T"/> to be removed.</param>
		/// <returns>A new array of type <typeparamref name="T"/> where <paramref name="item"/> has been removed.</returns>
		public static T[] Remove<T>(this T[] self, T item)
		{
			return self.RemoveAt(self.IndexOf(item));
		}

		/// <summary>
		/// Removes an item from an array by specifying the index.
		/// </summary>
		/// <typeparam name="T">The type of object contained in this array.</typeparam>
		/// <param name="self">The array to remove the item from.</param>
		/// <param name="index">The index of the item that will be removed.</param>
		/// <returns>A new array of type <typeparamref name="T"/> where the item at <paramref name="index"/> has been removed.</returns>
		public static T[] RemoveAt<T>(this T[] self, int index)
		{
			var result = new T[self.Length - 1];
			if (index > 0)
				Array.Copy(self, result, index);

			if (index < self.Length - 1)
				Array.Copy(self, index + 1, result, index, self.Length - index - 1);

			return result;
		}

		/// <summary>
		/// Retrieves the last item from an array.
		/// </summary>
		/// <typeparam name="T">The type of object to be retrieved from <paramref name="self"/>.</typeparam>
		/// <param name="self">The array to retrieve the last item from.</param>
		/// <returns>An object of type <typeparamref name="T"/> that is the last item in the collection.</returns>
		public static T Last<T>(this T[] self) => self[self.Length - 1];

		internal static T? FirstOrDefaultNoLinq<T>(this T[]? items) => items is null || items.Length == 0 ? default : items[0];
	}
}