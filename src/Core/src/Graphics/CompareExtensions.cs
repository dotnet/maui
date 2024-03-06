using System;

namespace Microsoft.Maui.Graphics
{
	internal static class CompareExtensions
	{
		public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
		{
			if (max.CompareTo(min) < 0)

/* Unmerged change from project 'Core(net8.0)'
Before:
				return max;

			if (value.CompareTo(min) < 0)
				return min;
After:
			{
				return max;
			}
*/

/* Unmerged change from project 'Core(net8.0-maccatalyst)'
Before:
				return max;

			if (value.CompareTo(min) < 0)
				return min;
After:
			{
				return max;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.19041.0)'
Before:
				return max;

			if (value.CompareTo(min) < 0)
				return min;
After:
			{
				return max;
			}
*/

/* Unmerged change from project 'Core(net8.0-windows10.0.20348.0)'
Before:
				return max;

			if (value.CompareTo(min) < 0)
				return min;
After:
			{
				return max;
			}
*/
			{
				return max;
			}

			if (value.CompareTo(min) < 0)
			{
				return min;
			}

			if (value.CompareTo(max) > 0)
			{
			{
				return max;
			}
			}

			return value;
		}
	}
}