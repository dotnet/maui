using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	static class NumericExtensions
	{
		const double Tolerance = 0.001;

		public static bool IsCloseTo(this double sizeA, double sizeB)
		{
			if (Math.Abs(sizeA - sizeB) > Tolerance)
			{
				return false;
			}

			return true;
		}
	}
}