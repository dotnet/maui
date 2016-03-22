using System;

namespace Xamarin.Forms
{
	internal static class NumericExtensions
	{
		public static double Clamp(this double self, double min, double max)
		{
			return Math.Min(max, Math.Max(self, min));
		}

		public static int Clamp(this int self, int min, int max)
		{
			return Math.Min(max, Math.Max(self, min));
		}
	}
}