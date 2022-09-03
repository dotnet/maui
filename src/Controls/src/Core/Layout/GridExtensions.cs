using System;

namespace Microsoft.Maui.Controls
{
	public static class GridExtensions
	{
		public static void Add(this Grid grid, IView view, int column = 0, int row = 0)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));
			if (column < 0)
				throw new ArgumentOutOfRangeException(nameof(column));
			if (row < 0)
				throw new ArgumentOutOfRangeException(nameof(row));

			grid.Add(view);
			grid.SetColumn(view, column);
			grid.SetRow(view, row);
		}
		
		public static void Add(this Grid grid, IView view, int left, int right, int top, int bottom)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));
            if (left < 0)
                throw new ArgumentOutOfRangeException(nameof(left));
            if (right < 1 || right < left + 1)
                throw new ArgumentOutOfRangeException(nameof(right));
            if (top < 0)
                throw new ArgumentOutOfRangeException(nameof(top));
            if (bottom < 1 || bottom < top + 1)
                throw new ArgumentOutOfRangeException(nameof(bottom));

            grid.Add(view);
            grid.SetColumn(view, left);
            grid.SetRow(view, top);
            grid.SetColumnSpan(view, right - left);
            grid.SetRowSpan(view, bottom - top);
        }
	}
}
