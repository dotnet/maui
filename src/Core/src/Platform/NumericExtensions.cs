using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui
{
	internal static class NumericExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static double Clamp(this double self, double min, double max)
		{
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
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Clamp(this int self, int min, int max)
		{
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
		}

		internal static int ToEm(this double pt)
		{
			return Convert.ToInt32(pt * 0.0624f * 1000); //Coefficient for converting Pt to Em. The value is uniform spacing between characters, in units of 1/1000 of an em.
		}
	}
}