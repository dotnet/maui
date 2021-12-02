using System;

namespace Microsoft.Maui.Controls
{
	public static class GridLayoutExtensions
	{
		public static void Add(this GridLayout gridLayout, IView view, int column = 0, int row = 0)
		{
			if (view == null)
				throw new ArgumentNullException(nameof(view));
			if (column < 0)
				throw new ArgumentOutOfRangeException(nameof(column));
			if (row < 0)
				throw new ArgumentOutOfRangeException(nameof(row));

			gridLayout.Add(view);
			gridLayout.SetColumn(view, column);
			gridLayout.SetRow(view, row);
		}
	}
}
