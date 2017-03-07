using System;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	public static class NumericExtensions
	{

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static double Clamp(this double self, double min, double max)
		{
			return Math.Min(max, Math.Max(self, min));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static int Clamp(this int self, int min, int max)
		{
			return Math.Min(max, Math.Max(self, min));
		}
	}
}