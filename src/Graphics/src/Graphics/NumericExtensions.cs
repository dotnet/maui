#if NETSTANDARD2_1_OR_GREATER
using System;
#endif
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Graphics
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class NumericExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(this float self, float min, float max)
		{
#if NETCOREAPP3_0_OR_GREATER
			return float.Clamp(self, min, max);
#else
			if (max < min)
			{
				return max;
			}
			else if (self < min)
			{
				return min;
			}
			else if (self > max)
			{
				return max;
			}

			return self;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Clamp(this double self, double min, double max)
		{
#if NETCOREAPP3_0_OR_GREATER
			return double.Clamp(self, min, max);
#else
			if (max < min)
			{
				return max;
			}
			else if (self < min)
			{
				return min;
			}
			else if (self > max)
			{
				return max;
			}

			return self;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(this int self, int min, int max)
		{
#if NETSTANDARD2_1_OR_GREATER
			return Math.Clamp(self, min, max);
#else
			if (max < min)
			{
				return max;
			}
			else if (self < min)
			{
				return min;
			}
			else if (self > max)
			{
				return max;
			}

			return self;
#endif
		}
	}
}
