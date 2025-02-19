using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public static class ShimExtensions
	{
		public static void LowerChild(this Layout layout, View view)
		{
			if (!layout.Contains(view) || layout.First() == view)
				return;

			layout.Remove(view);
			layout.Insert(view, 0);
		}

		public static void RaiseChild(this Layout layout, View view)
		{
			if (!layout.Contains(view) || layout.Last() == view)
				return;

			layout.Remove(view);
			layout.Add(view);
		}

		public static void Add(this IList<IView> list, View view, int left, int top)
		{
			if (list is Grid grid)
				grid.Add(view, left, top);
			else
				throw new NotImplementedException($"{list}");
		}

		public static void Add(this Grid grid, View view, int left, int top)
		{
			if (left < 0)
				throw new ArgumentOutOfRangeException("left");
			if (top < 0)
				throw new ArgumentOutOfRangeException("top");
			grid.Add(view, left, left + 1, top, top + 1);
		}

		public static double AutoSize => -1;
		public static void Add(this AbsoluteLayout layout, View view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None)
		{
			layout.SetLayoutBounds(view, bounds);
			layout.SetLayoutFlags(view, flags);
			layout.Add(view);

		}

		public static void Add(this AbsoluteLayout layout, View view, Point position)
		{
			layout.SetLayoutBounds(view, new Rect(position.X, position.Y, AutoSize, AutoSize));
			layout.Add(view);
		}

		public static void Add(this IList<IView> list, View view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None)
		{
			if (list is AbsoluteLayout abs)
				abs.Add(view, bounds, flags);
			else
				throw new NotImplementedException($"{list}");

		}

		public static void Add(this IList<IView> list, View view, Point position)
		{
			if (list is AbsoluteLayout abs)
				abs.Add(view, position);
			else
				throw new NotImplementedException($"{list}");
		}

		public static void Add(this IList<IView> list, View view, int left, int right, int top, int bottom)
		{
			if (list is Grid grid)
				grid.Add(view, left, right, top, bottom);
			else
				throw new NotImplementedException($"{list}");
		}

		public static void AddHorizontal(this IList<IView> list, IEnumerable<View> views)
		{
			if (list is Grid grid)
				grid.AddHorizontal(views);
			else
				throw new NotImplementedException($"{list}");
		}

		public static void AddHorizontal(this IList<IView> list, View view)
		{

			if (list is Grid grid)
				grid.AddHorizontal(view);
			else
				throw new NotImplementedException($"{list}");
		}

		public static void AddVertical(this IList<IView> list, IEnumerable<View> views)
		{

			if (list is Grid grid)
				grid.AddVertical(views);
			else
				throw new NotImplementedException($"{list}");
		}

		public static void AddVertical(this IList<IView> list, View view)
		{
			if (list is Grid grid)
				grid.AddVertical(view);
			else
				throw new NotImplementedException($"{list}");
		}


		private static int RowCount(this Grid grid) => Math.Max(
			grid.Max<IView, int?>(w => grid.GetRow(w) + grid.GetRowSpan(w)) ?? 0,
			grid.RowDefinitions.Count
		);

		private static int ColumnCount(this Grid grid) => Math.Max(
			grid.Max<IView, int?>(w => grid.GetColumn(w) + grid.GetColumnSpan(w)) ?? 0,
			grid.ColumnDefinitions.Count
		);

		public static void AddHorizontal(this Grid grid, IEnumerable<View> views)
		{
			if (views == null)
				throw new ArgumentNullException("views");

			foreach (View v in views)
				AddHorizontal(grid, v);
		}

		public static void AddHorizontal(this Grid grid, View view)
		{
			if (view == null)
				throw new ArgumentNullException("view");

			var rows = grid.RowCount();
			var columns = grid.ColumnCount();

			// if no rows, create a row
			if (rows == 0)
				rows++;

			grid.Add(view, columns, columns + 1, 0, rows);
		}

		public static void AddVertical(this Grid grid, IEnumerable<View> views)
		{
			if (views == null)
				throw new ArgumentNullException("views");

			foreach (var v in views)
				AddVertical(grid, (View)v);
		}

		public static void AddVertical(this Grid grid, View view)
		{
			if (view == null)
				throw new ArgumentNullException("view");

			var rows = grid.RowCount();
			var columns = grid.ColumnCount();

			// if no columns, create a column
			if (columns == 0)
				columns++;

			grid.Add(view, 0, columns, rows, rows + 1);
		}

		public static void Add(this Grid grid, View view, int left, int right, int top, int bottom)
		{
			if (left < 0)
				throw new ArgumentOutOfRangeException("left");
			if (top < 0)
				throw new ArgumentOutOfRangeException("top");
			if (left >= right)
				throw new ArgumentOutOfRangeException("right");
			if (top >= bottom)
				throw new ArgumentOutOfRangeException("bottom");
			if (view == null)
				throw new ArgumentNullException("view");

			grid.SetRow(view, top);
			grid.SetRowSpan(view, bottom - top);
			grid.SetColumn(view, left);
			grid.SetColumnSpan(view, right - left);

			grid.Add(view);
		}
	}
}
