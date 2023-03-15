#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public static class GridExtensions
	{
		/// <summary>
		/// Adds an <see cref="IView" /> to the <see cref="Grid"/> at the specified column and row with a row span of 1 and a column span of 1.
		/// </summary>
		/// <param name="grid">The <see cref="Grid"/> to which the <see cref="IView" /> will be added.</param>
		/// <param name="view">The <see cref="IView" /> to add.</param>
		/// <param name="column">The column in which to place the <see cref="IView" />.</param>
		/// <param name="row">The row in which to place the <see cref="IView" />.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="row"/> or <paramref name="column"/> are less than 0.
		/// </exception>
		/// <remarks>If the <see cref="Grid"/> does not have enough rows/columns to encompass the specified location, they will be added.</remarks>
		public static void Add(this Grid grid, IView view, int column = 0, int row = 0)
		{
			grid.AddWithSpan(view, row, column, 1, 1);
		}

		/// <summary>
		/// Adds an <see cref="IView" /> to the <see cref="Grid"/> at the specified row and column spans. 
		/// </summary>
		/// <param name="grid">The <see cref="Grid"/> to which the <see cref="IView" /> will be added.</param>
		/// <param name="view">The <see cref="IView" /> to add.</param>
		/// <param name="left">The left edge of the column span. Must be greater than or equal to 0.</param>
		/// <param name="right">The right edge of the column span. Must be greater than left. The <see cref="IView" /> won't occupy this column, but will stop just before it.</param>
		/// <param name="top">The top edge of the row span. Must be greater than or equal to 0.</param>
		/// <param name="bottom">The bottom edge of the row span. Must be greater than top. The <see cref="IView" /> won't occupy this row, but will stop just before it.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="left"/> or <paramref name="top"/> are less than 0, <paramref name="bottom"/> is less than or equal to <paramref name="top"/>,
		/// or <paramref name="right"/> is less than or equal to <paramref name="left"/>.
		/// </exception>
		/// <remarks>If the <see cref="Grid"/> does not have enough rows/columns to encompass the specified spans, they will be added.</remarks>
		[Obsolete("This method is obsolete. Please use AddWithSpan(this Grid, IView, int, int, int, int) instead.")]
		public static void Add(this Grid grid, IView view, int left, int right, int top, int bottom)
		{
			grid.AddWithSpan(view, top, left, bottom - top, right - left);
		}

		/// <summary>
		/// Adds an <see cref="IView" /> to the the <see cref="Grid"/> at the specified row and column with the specified row and column spans.
		/// </summary>
		/// <param name="grid">The <see cref="Grid"/> to which the <see cref="IView" /> will be added.</param>
		/// <param name="view">The <see cref="IView" /> to add.</param>
		/// <param name="row">The top row in which to place the <see cref="IView" />. Defaults to 0.</param>
		/// <param name="column">The left column in which to place the <see cref="IView" />. Defaults to 0.</param>
		/// <param name="rowSpan">The number of rows the <see cref="IView" /> should span. Defaults to 1.</param>
		/// <param name="columnSpan">The number of columns the <see cref="IView" /> should span. Defaults to 1.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="view"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when <paramref name="row"/> or <paramref name="column"/> are less than 0, or  <paramref name="rowSpan"/> or <paramref name="columnSpan"/> are less than 1.
		/// </exception>
		/// <remarks>If the <see cref="Grid"/> does not have enough rows/columns to encompass the specified spans, they will be added.</remarks>
		public static void AddWithSpan(this Grid grid, IView view, int row = 0, int column = 0, int rowSpan = 1, int columnSpan = 1)
		{
			if (view is null)
				throw new ArgumentNullException(nameof(view));
			if (row < 0)
				throw new ArgumentOutOfRangeException(nameof(row));
			if (column < 0)
				throw new ArgumentOutOfRangeException(nameof(column));
			if (rowSpan < 1)
				throw new ArgumentOutOfRangeException(nameof(rowSpan));
			if (columnSpan < 1)
				throw new ArgumentOutOfRangeException(nameof(columnSpan));

			grid.Add(view);

			EnsureRows(grid, row + rowSpan);
			grid.SetRow(view, row);
			grid.SetRowSpan(view, rowSpan);

			EnsureColumns(grid, column + columnSpan);
			grid.SetColumn(view, column);
			grid.SetColumnSpan(view, columnSpan);
		}

		static void EnsureRows(Grid grid, int rows)
		{
			if (rows == 1)
			{
				return;
			}

			var count = grid.RowDefinitions.Count;

			for (int n = count; n < rows; n++)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}

		static void EnsureColumns(Grid grid, int columns)
		{
			if (columns == 1)
			{
				return;
			}

			var count = grid.ColumnDefinitions.Count;

			for (int n = count; n < columns; n++)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}
		}
	}
}
