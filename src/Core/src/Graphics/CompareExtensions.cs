using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Graphics
{
	internal static class CompareExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DateTime Clamp(this DateTime value, DateTime min, DateTime max)
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