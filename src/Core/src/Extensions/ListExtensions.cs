using System.Collections.Generic;

namespace Microsoft.Maui
{
	internal static class ListExtensions
	{
		/// <summary>
		/// Attempts to remove an item from a <see cref="IList{T}"/> instance.
		/// </summary>
		/// <typeparam name="T">The type of object to be removed from <paramref name="list"/>.</typeparam>
		/// <param name="list">The <see cref="IList{T}"/> instance to remove the item from.</param>
		/// <param name="item">The item of type <typeparamref name="T"/> to be removed.</param>
		/// <returns><see langword="true"/> if the item was successfully removed, otherwise <see langword="false"/>.</returns>
		public static bool TryRemove<T>(this IList<T> list, T item)
		{
			try
			{
				list.Remove(item);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
