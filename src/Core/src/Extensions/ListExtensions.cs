using System;
using System.Collections.Generic;

namespace Microsoft.Maui
{
	internal static class ListExtensions
	{
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
