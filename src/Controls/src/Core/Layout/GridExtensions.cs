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
	}
}
