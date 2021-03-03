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
	}
}