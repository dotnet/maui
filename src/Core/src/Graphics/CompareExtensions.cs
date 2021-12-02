using System;

namespace Microsoft.Maui.Graphics
{
	internal static class CompareExtensions
	{
		public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
		{
			if (max.CompareTo(min) < 0)
				return max;

			if (value.CompareTo(min) < 0)
				return min;

			if (value.CompareTo(max) > 0)
				return max;

			return value;
		}
	}
}