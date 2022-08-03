using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	internal static class IndicatorViewExtensions
	{
		public static int GetMaximumVisible(this IIndicatorView indicatorView)
		{
			var minValue = Math.Min(indicatorView.MaximumVisible, indicatorView.Count);
			var maximumVisible = minValue <= 0 ? 0 : minValue;
			bool hideSingle = indicatorView.HideSingle;

			if (maximumVisible == 1 && hideSingle)
				maximumVisible = 0;

			return maximumVisible;
		}

		public static bool IsCircleShape(this IIndicatorView indicatorView)
		{
			var sH = indicatorView.IndicatorsShape;
			var pointsCount = 13;
			if (sH != null)
			{
				var path = sH.PathForBounds(new Rect(0, 0, 6, 6));
				pointsCount = path.Count;
			}

			return pointsCount == 13;

		}
	}
}
