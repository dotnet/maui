#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal static class SizeExtensions
	{
		const double Tolerance = 0.001;

		public static bool IsCloseTo(this CGSize sizeA, CGSize sizeB)
		{
			if (Math.Abs(sizeA.Height - sizeB.Height) > Tolerance)
			{
				return false;
			}

			if (Math.Abs(sizeA.Width - sizeB.Width) > Tolerance)
			{
				return false;
			}

			return true;
		}

		public static bool IsCloseTo(this CGSize sizeA, Size sizeB)
		{
			if (Math.Abs(sizeA.Height - sizeB.Height) > Tolerance)
			{
				return false;
			}

			if (Math.Abs(sizeA.Width - sizeB.Width) > Tolerance)
			{
				return false;
			}

			return true;
		}
	}
}
