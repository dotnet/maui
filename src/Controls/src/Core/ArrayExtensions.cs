using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class ArrayExtensions
	{
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

		public static T[] Remove<T>(this T[] self, T item)
		{
			return self.RemoveAt(self.IndexOf(item));
		}

		public static T[] RemoveAt<T>(this T[] self, int index)
		{
			var result = new T[self.Length - 1];
			if (index > 0)
				Array.Copy(self, result, index);

			if (index < self.Length - 1)
				Array.Copy(self, index + 1, result, index, self.Length - index - 1);

			return result;
		}
	}
}