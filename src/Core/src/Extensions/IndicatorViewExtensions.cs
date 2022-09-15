using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	internal static class IndicatorViewExtensions
	{
		/// <summary>
		/// Gets the maximum number of visible indicator items that can be shown.
		/// </summary>
		/// <param name="indicatorView">The <see cref="IIndicatorView"/> instance to get the maximum visible items for.</param>
		/// <returns>Maximum number of visible indicator items that can be shown.</returns>
		public static int GetMaximumVisible(this IIndicatorView indicatorView)
		{
			var minValue = Math.Min(indicatorView.MaximumVisible, indicatorView.Count);
			var maximumVisible = minValue <= 0 ? 0 : minValue;
			bool hideSingle = indicatorView.HideSingle;

			if (maximumVisible == 1 && hideSingle)
				maximumVisible = 0;

			return maximumVisible;
		}

		/// <summary>
		/// Determines whether the current indicator items are circle shaped.
		/// </summary>
		/// <param name="indicatorView">The <see cref="IIndicatorView"/> instance to determine the indicator item shape for.</param>
		/// <returns><see langword="true"/> if the indicator items are circle shaped, otherwise <see langword="false"/>.</returns>
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
