using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class Grid
	{
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			if (!InternalChildren.Any())
				return;

			// grab a snapshot of this grid, fully-measured, so we can do the layout
			var structure = new GridStructure(this, width, height);

			for (var index = 0; index < InternalChildren.Count; index++)
			{
				var child = (View)InternalChildren[index];
				if (!child.IsVisible)
					continue;
				int r = GetRow(child);
				int c = GetColumn(child);
				int rs = GetRowSpan(child);
				int cs = GetColumnSpan(child);

				double posx = x + c * ColumnSpacing;
				for (var i = 0; i < c; i++)
					posx += structure.Columns[i].ActualWidth;
				double posy = y + r * RowSpacing;
				for (var i = 0; i < r; i++)
					posy += structure.Rows[i].ActualHeight;

				double w = structure.Columns[c].ActualWidth;
				for (var i = 1; i < cs; i++)
					w += ColumnSpacing + structure.Columns[c + i].ActualWidth;
				double h = structure.Rows[r].ActualHeight;
				for (var i = 1; i < rs; i++)
					h += RowSpacing + structure.Rows[r + i].ActualHeight;

				// in the future we can might maybe optimize by passing the already calculated size request
				LayoutChildIntoBoundingRegion(child, new Rectangle(posx, posy, w, h));
			}
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			if (!InternalChildren.Any())
				return new SizeRequest(new Size(0, 0));

			// grab a snapshot of this grid, fully-measured, so we can do the layout
			var structure = new GridStructure(this, widthConstraint, heightConstraint, true);

			double columnWidthSum = 0;
			double nonStarColumnWidthSum = 0;
			for (var index = 0; index < structure.Columns.Count; index++)
			{
				ColumnDefinition c = structure.Columns[index];
				columnWidthSum += c.ActualWidth;
				if (!c.Width.IsStar)
					nonStarColumnWidthSum += c.ActualWidth;
			}
			double rowHeightSum = 0;
			double nonStarRowHeightSum = 0;
			for (var index = 0; index < structure.Rows.Count; index++)
			{
				RowDefinition r = structure.Rows[index];
				rowHeightSum += r.ActualHeight;
				if (!r.Height.IsStar)
					nonStarRowHeightSum += r.ActualHeight;
			}

			var request = new Size(columnWidthSum + (structure.Columns.Count - 1) * ColumnSpacing, rowHeightSum + (structure.Rows.Count - 1) * RowSpacing);
			var minimum = new Size(nonStarColumnWidthSum + (structure.Columns.Count - 1) * ColumnSpacing, nonStarRowHeightSum + (structure.Rows.Count - 1) * RowSpacing);

			var result = new SizeRequest(request, minimum);
			return result;
		}

		/// <summary>
		/// Creates a snapshot of the Grid row/column structure, optionally with measurement
		/// </summary>
		class GridStructure
		{
			// These aren't strictly necessary; we could just use the auto-properties. But when we drop .NET Standard 1.0
			// support, the public accessors should be made into ReadOnlyList and we'll need these private lists
			List<ColumnDefinition> _columns = new List<ColumnDefinition>();
			List<RowDefinition> _rows = new List<RowDefinition>();

			// Technically speaking, these should really be ReadOnlyList, but AsReadOnly isn't available in .NET Standard 1.0
			// So for now we're just going to trust our internal code not to rely on modifying these
			public List<ColumnDefinition> Columns => _columns;
			public List<RowDefinition> Rows => _rows;

			public GridStructure(Grid grid)
			{
				EnsureRowsColumnsInitialized(grid);
			}

			public GridStructure(Grid grid, double width, double height, bool requestSize = false) : this(grid)
			{
				AssignAbsoluteCells();

				CalculateAutoCells(grid, width, height);

				var columnSpacing = grid.ColumnSpacing;
				var rowSpacing = grid.RowSpacing;

				if (!requestSize)
				{
					ContractAutoColumnsIfNeeded(width, columnSpacing, rowSpacing);
					ContractAutoRowsIfNeeded(height, columnSpacing, rowSpacing);
				}

				double totalStarsHeight = 0;
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (row.Height.IsStar)
						totalStarsHeight += row.Height.Value;
				}

				double totalStarsWidth = 0;
				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					if (col.Width.IsStar)
						totalStarsWidth += col.Width.Value;
				}

				if (requestSize)
				{
					MeasureAndContractStarredColumns(grid, width, height, totalStarsWidth);
					MeasureAndContractStarredRows(grid, width, height, totalStarsHeight);
				}
				else
				{
					CalculateStarCells(width, height, totalStarsWidth, totalStarsHeight, columnSpacing, rowSpacing);
				}

				ZeroUnassignedCells();

				ExpandLastAutoRowIfNeeded(grid, height, requestSize);
				ExpandLastAutoColumnIfNeeded(grid, width, requestSize);
			}

			void AssignAbsoluteCells()
			{
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (row.Height.IsAbsolute)
						row.ActualHeight = row.Height.Value;
				}

				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					if (col.Width.IsAbsolute)
						col.ActualWidth = col.Width.Value;
				}
			}

			void CalculateAutoCells(Grid grid, double width, double height)
			{
				// this require multiple passes. First process the 1-span, then 2, 3, ...
				// And this needs to be run twice, just in case a lower-span column can be determined by a larger span
				for (var iteration = 0; iteration < 2; iteration++)
				{
					for (var rowspan = 1; rowspan <= _rows.Count; rowspan++)
					{
						for (var i = 0; i < _rows.Count; i++)
						{
							RowDefinition row = _rows[i];
							if (!row.Height.IsAuto)
								continue;
							if (row.ActualHeight >= 0) // if Actual is already set (by a smaller span), skip till pass 3
								continue;

							double actualHeight = row.ActualHeight;
							double minimumHeight = row.MinimumHeight;
							for (var index = 0; index < grid.InternalChildren.Count; index++)
							{
								var child = (View)(grid.InternalChildren[index]);
								if (!child.IsVisible || GetRowSpan(child) != rowspan || !IsInRow(child, i) || NumberOfUnsetRowHeight(child) > 1)
									continue;
								double assignedWidth = GetAssignedColumnWidth(child);
								double assignedHeight = GetAssignedRowHeight(child);
								double widthRequest = assignedWidth + GetUnassignedWidth(width, grid.ColumnSpacing);
								double heightRequest = double.IsPositiveInfinity(height) ? double.PositiveInfinity : assignedHeight + GetUnassignedHeight(height, grid.RowSpacing);

								SizeRequest sizeRequest = child.Measure(widthRequest, heightRequest, MeasureFlags.IncludeMargins);
								actualHeight = Math.Max(actualHeight, sizeRequest.Request.Height - assignedHeight - grid.RowSpacing * (GetRowSpan(child) - 1));
								minimumHeight = Math.Max(minimumHeight, sizeRequest.Minimum.Height - assignedHeight - grid.RowSpacing * (GetRowSpan(child) - 1));
							}
							if (actualHeight >= 0)
								row.ActualHeight = actualHeight;
							if (minimumHeight >= 0)
								row.MinimumHeight = minimumHeight;
						}
					}

					for (var colspan = 1; colspan <= _columns.Count; colspan++)
					{
						for (var i = 0; i < _columns.Count; i++)
						{
							ColumnDefinition col = _columns[i];
							if (!col.Width.IsAuto)
								continue;
							if (col.ActualWidth >= 0) // if Actual is already set (by a smaller span), skip
								continue;

							double actualWidth = col.ActualWidth;
							double minimumWidth = col.MinimumWidth;
							for (var index = 0; index < grid.InternalChildren.Count; index++)
							{
								var child = (View)(grid.InternalChildren)[index];
								if (!child.IsVisible || GetColumnSpan(child) != colspan || !IsInColumn(child, i) || NumberOfUnsetColumnWidth(child) > 1)
									continue;
								double assignedWidth = GetAssignedColumnWidth(child);
								double assignedHeight = GetAssignedRowHeight(child);
								double widthRequest = double.IsPositiveInfinity(width) ? double.PositiveInfinity : assignedWidth + GetUnassignedWidth(width, grid.ColumnSpacing);
								double heightRequest = assignedHeight + GetUnassignedHeight(height, grid.RowSpacing);

								SizeRequest sizeRequest = child.Measure(widthRequest, heightRequest, MeasureFlags.IncludeMargins);
								actualWidth = Math.Max(actualWidth, sizeRequest.Request.Width - assignedWidth - (GetColumnSpan(child) - 1) * grid.ColumnSpacing);
								minimumWidth = Math.Max(minimumWidth, sizeRequest.Minimum.Width - assignedWidth - (GetColumnSpan(child) - 1) * grid.ColumnSpacing);
							}
							if (actualWidth >= 0)
								col.ActualWidth = actualWidth;
							if (minimumWidth >= 0)
								col.MinimumWidth = minimumWidth;
						}
					}
				}
			}

			void CalculateStarCells(double width, double height, double totalStarsWidth, double totalStarsHeight, double columnSpacing, double rowSpacing)
			{
				double starColWidth = GetUnassignedWidth(width, columnSpacing) / totalStarsWidth;
				double starRowHeight = GetUnassignedHeight(height, rowSpacing) / totalStarsHeight;

				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					if (col.Width.IsStar)
						col.ActualWidth = col.Width.Value * starColWidth;
				}

				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (row.Height.IsStar)
						row.ActualHeight = row.Height.Value * starRowHeight;
				}
			}

			double ComputeColumnWidthSum()
			{
				double columnwWidthSum = 0;
				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition c = _columns[index];
					columnwWidthSum += Math.Max(0, c.ActualWidth);
				}

				return columnwWidthSum;
			}

			double ComputeRowHeightSum()
			{
				double rowHeightSum = 0;
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition r = _rows[index];
					rowHeightSum += Math.Max(0, r.ActualHeight);
				}

				return rowHeightSum;
			}

			Size ComputeCurrentSize(double columnSpacing, double rowSpacing)
			{
				var columnWidthSum = ComputeColumnWidthSum();
				var rowHeightSum = ComputeRowHeightSum();

				return new Size(columnWidthSum + (_columns.Count - 1) * columnSpacing, rowHeightSum + (_rows.Count - 1) * rowSpacing);
			}

			void ContractAutoColumnsIfNeeded(double targetWidth, double columnSpacing, double rowSpacing)
			{
				var currentSize = ComputeCurrentSize(columnSpacing, rowSpacing);

				if (currentSize.Width <= targetWidth)
				{
					return;
				}

				double contractionSpace = 0;
				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition c = _columns[index];
					if (c.Width.IsAuto)
						contractionSpace += c.ActualWidth - c.MinimumWidth;
				}
				if (contractionSpace > 0)
				{
					// contract as much as we can but no more
					double contractionNeeded = Math.Min(contractionSpace, Math.Max(currentSize.Width - targetWidth, 0));
					double contractFactor = contractionNeeded / contractionSpace;

					for (var index = 0; index < _columns.Count; index++)
					{
						ColumnDefinition col = _columns[index];
						if (!col.Width.IsAuto)
							continue;
						double availableSpace = col.ActualWidth - Math.Max(col.MinimumWidth, 0);
						double contraction = availableSpace * contractFactor;
						col.ActualWidth -= contraction;
						contractionNeeded -= contraction;
					}
				}
			}

			void ContractAutoRowsIfNeeded(double targetHeight, double columnSpacing, double rowSpacing)
			{
				var currentSize = ComputeCurrentSize(columnSpacing, rowSpacing);

				if (currentSize.Height <= targetHeight)
				{
					return;
				}

				double contractionSpace = 0;
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition r = _rows[index];
					if (r.Height.IsAuto)
						contractionSpace += r.ActualHeight - r.MinimumHeight;
				}
				if (!(contractionSpace > 0))
					return;
				// contract as much as we can but no more
				double contractionNeeded = Math.Min(contractionSpace, Math.Max(currentSize.Height - targetHeight, 0));
				double contractFactor = contractionNeeded / contractionSpace;
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (!row.Height.IsAuto)
						continue;
					double availableSpace = row.ActualHeight - row.MinimumHeight;
					double contraction = availableSpace * contractFactor;
					row.ActualHeight -= contraction;
					contractionNeeded -= contraction;
				}
			}

			void ContractStarColumnsIfNeeded(double targetWidth, double columnSpacing, double rowSpacing)
			{
				var request = ComputeCurrentSize(columnSpacing, rowSpacing);

				if (request.Width <= targetWidth)
				{
					return;
				}

				double starColumnWidth = 0;
				double starColumnMinWidth = 0;
				double contractionSpace = 0;

				for (int n = 0; n < _columns.Count; n++)
				{
					var column = _columns[n];

					if (!column.Width.IsStar)
					{
						continue;
					}

					if (starColumnWidth == 0)
					{
						starColumnWidth = column.ActualWidth;
					}
					else
					{
						starColumnWidth = Math.Min(column.ActualWidth, starColumnWidth);
					}

					if (column.MinimumWidth > starColumnMinWidth)
					{
						starColumnMinWidth = column.MinimumWidth;
					}

					contractionSpace += column.ActualWidth - column.MinimumWidth;
				}

				if (contractionSpace <= 0)
				{
					return;
				}

				// contract as much as we can but no more
				double contractionNeeded = Math.Min(contractionSpace, Math.Max(request.Width - targetWidth, 0));
				double contractionFactor = contractionNeeded / contractionSpace;
				var delta = contractionFactor >= 1
					? starColumnWidth - starColumnMinWidth
					: contractionFactor * (starColumnWidth - starColumnMinWidth);

				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition column = _columns[index];
					if (!column.Width.IsStar)
					{
						continue;
					}

					column.ActualWidth -= delta * column.Width.Value;
				}
			}

			void ContractStarRowsIfNeeded(double targetHeight, double columnSpacing, double rowSpacing)
			{
				var request = ComputeCurrentSize(columnSpacing, rowSpacing);

				if (request.Height <= targetHeight)
				{
					return;
				}

				double starRowHeight = 0;
				double starRowMinHeight = 0;
				double contractionSpace = 0;

				for (int n = 0; n < _rows.Count; n++)
				{
					var row = _rows[n];

					if (!row.Height.IsStar)
					{
						continue;
					}

					if (starRowHeight == 0)
					{
						starRowHeight = row.ActualHeight;
					}
					else
					{
						starRowHeight = Math.Min(row.ActualHeight, starRowHeight);
					}

					if (row.MinimumHeight > starRowMinHeight)
					{
						starRowMinHeight = row.MinimumHeight;
					}

					contractionSpace += row.ActualHeight - row.MinimumHeight;
				}

				if (contractionSpace <= 0)
				{
					return;
				}

				double contractionNeeded = Math.Min(contractionSpace, Math.Max(request.Height - targetHeight, 0));
				double contractionFactor = contractionNeeded / contractionSpace;
				var delta = contractionFactor >= 1
					? starRowHeight - starRowMinHeight
					: contractionFactor * (starRowHeight - starRowMinHeight);

				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (!row.Height.IsStar)
					{
						continue;
					}

					row.ActualHeight -= delta * row.Height.Value;
				}
			}

			void EnsureRowsColumnsInitialized(Grid grid)
			{
				_columns = grid.ColumnDefinitions == null ? new List<ColumnDefinition>() : grid.ColumnDefinitions.ToList();
				_rows = grid.RowDefinitions == null ? new List<RowDefinition>() : grid.RowDefinitions.ToList();

				int lastRow = -1;
				for (var index = 0; index < grid.InternalChildren.Count; index++)
				{
					Element w = grid.InternalChildren[index];
					lastRow = Math.Max(lastRow, GetRow(w) + GetRowSpan(w) - 1);
				}
				lastRow = Math.Max(lastRow, grid.RowDefinitions.Count - 1);

				int lastCol = -1;
				for (var index = 0; index < grid.InternalChildren.Count; index++)
				{
					Element w = grid.InternalChildren[index];
					lastCol = Math.Max(lastCol, GetColumn(w) + GetColumnSpan(w) - 1);
				}
				lastCol = Math.Max(lastCol, grid.ColumnDefinitions.Count - 1);

				while (_columns.Count <= lastCol)
					_columns.Add(new ColumnDefinition());
				while (_rows.Count <= lastRow)
					_rows.Add(new RowDefinition());

				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					col.ActualWidth = -1;
				}
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					row.ActualHeight = -1;
				}
			}

			void ExpandLastAutoColumnIfNeeded(Grid grid, double width, bool expandToRequest)
			{
				for (var index = 0; index < grid.InternalChildren.Count; index++)
				{
					Element element = grid.InternalChildren[index];
					var child = (View)element;
					if (!child.IsVisible)
						continue;

					ColumnDefinition col = GetLastAutoColumn(child);
					if (col == null)
						continue;

					double assignedWidth = GetAssignedColumnWidth(child);
					double w = double.IsPositiveInfinity(width) ? double.PositiveInfinity : assignedWidth + GetUnassignedWidth(width, grid.ColumnSpacing);
					SizeRequest sizeRequest = child.Measure(w, GetAssignedRowHeight(child), MeasureFlags.IncludeMargins);
					double requiredWidth = expandToRequest ? sizeRequest.Request.Width : sizeRequest.Minimum.Width;
					double deltaWidth = requiredWidth - assignedWidth - (GetColumnSpan(child) - 1) * grid.ColumnSpacing;
					if (deltaWidth > 0)
					{
						col.ActualWidth += deltaWidth;
					}
				}
			}

			void ExpandLastAutoRowIfNeeded(Grid grid, double height, bool expandToRequest)
			{
				for (var index = 0; index < grid.InternalChildren.Count; index++)
				{
					Element element = grid.InternalChildren[index];
					var child = (View)element;
					if (!child.IsVisible)
						continue;

					RowDefinition row = GetLastAutoRow(child);
					if (row == null)
						continue;

					double assignedHeight = GetAssignedRowHeight(child);
					var unassignedHeight = GetUnassignedHeight(height, grid.RowSpacing);
					double h = double.IsPositiveInfinity(height) ? double.PositiveInfinity : assignedHeight + unassignedHeight;

					var acw = GetAssignedColumnWidth(child);

					SizeRequest sizeRequest = child.Measure(acw, h, MeasureFlags.IncludeMargins);

					double requiredHeight = expandToRequest
						? sizeRequest.Request.Height
						: sizeRequest.Request.Height <= h ? sizeRequest.Request.Height : sizeRequest.Minimum.Height;

					double deltaHeight = requiredHeight - assignedHeight - (GetRowSpan(child) - 1) * grid.RowSpacing;
					if (deltaHeight > 0)
					{
						row.ActualHeight += deltaHeight;
					}
				}
			}

			void MeasureAndContractStarredColumns(Grid grid, double width, double height, double totalStarsWidth)
			{
				double columnSpacing = grid.ColumnSpacing;
				double rowSpacing = grid.RowSpacing;

				double starColWidth;
				starColWidth = MeasuredStarredColumns(grid, GetUnassignedWidth(width, columnSpacing), height, totalStarsWidth);

				if (!double.IsPositiveInfinity(width) && double.IsPositiveInfinity(height))
				{
					// re-zero columns so GetUnassignedWidth returns correctly
					for (var index = 0; index < _columns.Count; index++)
					{
						ColumnDefinition col = _columns[index];
						if (col.Width.IsStar)
							col.ActualWidth = 0;
					}

					starColWidth = Math.Max(starColWidth, GetUnassignedWidth(width, columnSpacing) / totalStarsWidth);
				}

				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					if (col.Width.IsStar)
						col.ActualWidth = col.Width.Value * starColWidth;
				}

				ContractStarColumnsIfNeeded(width, columnSpacing, rowSpacing);
			}

			void MeasureAndContractStarredRows(Grid grid, double width, double height, double totalStarsHeight)
			{
				double columnSpacing = grid.ColumnSpacing;
				double rowSpacing = grid.RowSpacing;

				double starRowHeight;
				starRowHeight = MeasureStarredRows(grid, width, GetUnassignedHeight(height, rowSpacing), totalStarsHeight);

				if (!double.IsPositiveInfinity(height) && double.IsPositiveInfinity(width))
				{
					for (var index = 0; index < _rows.Count; index++)
					{
						RowDefinition row = _rows[index];
						if (row.Height.IsStar)
							row.ActualHeight = 0;
					}

					starRowHeight = Math.Max(starRowHeight, GetUnassignedHeight(height, rowSpacing) / totalStarsHeight);
				}

				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (row.Height.IsStar)
						row.ActualHeight = row.Height.Value * starRowHeight;
				}

				ContractStarRowsIfNeeded(height, columnSpacing, rowSpacing);
			}

			double MeasuredStarredColumns(Grid grid, double widthConstraint, double heightConstraint, double totalStarsWidth)
			{
				for (var iteration = 0; iteration < 2; iteration++)
				{
					for (var colspan = 1; colspan <= _columns.Count; colspan++)
					{
						for (var i = 0; i < _columns.Count; i++)
						{
							ColumnDefinition col = _columns[i];
							if (!col.Width.IsStar)
								continue;
							if (col.ActualWidth >= 0) // if Actual is already set (by a smaller span), skip
								continue;

							double actualWidth = col.ActualWidth;
							double minimumWidth = col.MinimumWidth;
							for (var index = 0; index < grid.InternalChildren.Count; index++)
							{
								var child = (View)(grid.InternalChildren)[index];
								if (!child.IsVisible || GetColumnSpan(child) != colspan || !IsInColumn(child, i) || NumberOfUnsetColumnWidth(child) > 1)
									continue;
								double assignedWidth = GetAssignedColumnWidth(child);

								// If we already have row height info, use it when measuring
								double assignedHeight = GetAssignedRowHeight(child);
								var hConstraint = assignedHeight > 0 ? assignedHeight : heightConstraint;

								SizeRequest sizeRequest = child.Measure(widthConstraint, hConstraint, MeasureFlags.IncludeMargins);
								var columnSpacing = (GetColumnSpan(child) - 1) * grid.ColumnSpacing;
								actualWidth = Math.Max(actualWidth, sizeRequest.Request.Width - assignedWidth - columnSpacing);
								minimumWidth = Math.Max(minimumWidth, sizeRequest.Minimum.Width - assignedWidth - columnSpacing);
							}
							if (actualWidth >= 0)
								col.ActualWidth = actualWidth;

							if (minimumWidth >= 0)
								col.MinimumWidth = minimumWidth;
						}
					}
				}

				double starColRequestWidth = 0;
				double starColMinWidth = 0;
				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					if (!col.Width.IsStar || col.Width.Value == 0 || col.ActualWidth <= 0)
						continue;

					starColRequestWidth = Math.Max(starColRequestWidth, col.ActualWidth / col.Width.Value);
					starColMinWidth = Math.Max(starColMinWidth, col.MinimumWidth / col.Width.Value);
				}

				if (starColRequestWidth * totalStarsWidth <= widthConstraint)
				{
					return starColRequestWidth;
				}

				return Math.Max(widthConstraint / totalStarsWidth, starColMinWidth);
			}

			double MeasureStarredRows(Grid grid, double widthConstraint, double heightConstraint, double totalStarsHeight)
			{
				for (var iteration = 0; iteration < 2; iteration++)
				{
					for (var rowspan = 1; rowspan <= _rows.Count; rowspan++)
					{
						for (var i = 0; i < _rows.Count; i++)
						{
							RowDefinition row = _rows[i];
							if (!row.Height.IsStar)
								continue;
							if (row.ActualHeight >= 0) // if Actual is already set (by a smaller span), skip till pass 3
								continue;

							double actualHeight = row.ActualHeight;
							double minimumHeight = row.MinimumHeight;
							for (var index = 0; index < grid.InternalChildren.Count; index++)
							{
								var child = (View)(grid.InternalChildren)[index];
								if (!child.IsVisible || GetRowSpan(child) != rowspan || !IsInRow(child, i) || NumberOfUnsetRowHeight(child) > 1)
									continue;
								double assignedHeight = GetAssignedRowHeight(child);

								// If we already have column width info, use it when measuring
								double assignedWidth = GetAssignedColumnWidth(child);
								var wConstraint = assignedWidth > 0 ? assignedWidth : widthConstraint;

								SizeRequest sizeRequest = child.Measure(wConstraint, heightConstraint, MeasureFlags.IncludeMargins);
								var rowSpacing = (GetRowSpan(child) - 1) * grid.RowSpacing;
								actualHeight = Math.Max(actualHeight, sizeRequest.Request.Height - assignedHeight - rowSpacing);
								minimumHeight = Math.Max(minimumHeight, sizeRequest.Minimum.Height - assignedHeight - rowSpacing);
							}
							if (actualHeight >= 0)
								row.ActualHeight = actualHeight;

							if (minimumHeight >= 0)
								row.MinimumHeight = minimumHeight;
						}
					}
				}

				double starRowRequestHeight = 0;
				double starRowMinHeight = 0;
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (!row.Height.IsStar || row.Height.Value == 0 || row.ActualHeight <= 0)
						continue;

					starRowRequestHeight = Math.Max(starRowRequestHeight, row.ActualHeight / row.Height.Value);
					starRowMinHeight = Math.Max(starRowMinHeight, row.MinimumHeight / row.Height.Value);
				}

				if (starRowRequestHeight * totalStarsHeight <= heightConstraint)
				{
					return starRowRequestHeight;
				}

				return Math.Max(heightConstraint / totalStarsHeight, starRowMinHeight);
			}

			void ZeroUnassignedCells()
			{
				for (var index = 0; index < _columns.Count; index++)
				{
					ColumnDefinition col = _columns[index];
					if (col.ActualWidth < 0)
						col.ActualWidth = 0;
				}
				for (var index = 0; index < _rows.Count; index++)
				{
					RowDefinition row = _rows[index];
					if (row.ActualHeight < 0)
						row.ActualHeight = 0;
				}
			}

			#region Helpers

			static bool IsInColumn(BindableObject child, int column)
			{
				int childColumn = GetColumn(child);
				int span = GetColumnSpan(child);
				return childColumn <= column && column < childColumn + span;
			}

			static bool IsInRow(BindableObject child, int row)
			{
				int childRow = GetRow(child);
				int span = GetRowSpan(child);
				return childRow <= row && row < childRow + span;
			}

			int NumberOfUnsetColumnWidth(BindableObject child)
			{
				var n = 0;
				int index = GetColumn(child);
				int span = GetColumnSpan(child);
				for (int i = index; i < index + span; i++)
					if (_columns[i].ActualWidth <= 0)
						n++;
				return n;
			}

			int NumberOfUnsetRowHeight(BindableObject child)
			{
				var n = 0;
				int index = GetRow(child);
				int span = GetRowSpan(child);
				for (int i = index; i < index + span; i++)
					if (_rows[i].ActualHeight <= 0)
						n++;
				return n;
			}

			double GetAssignedColumnWidth(BindableObject child)
			{
				var actual = 0d;
				int index = GetColumn(child);
				int span = GetColumnSpan(child);
				for (int i = index; i < index + span; i++)
					if (_columns[i].ActualWidth >= 0)
						actual += _columns[i].ActualWidth;
				return actual;
			}

			double GetAssignedRowHeight(BindableObject child)
			{
				var actual = 0d;
				int index = GetRow(child);
				int span = GetRowSpan(child);
				for (int i = index; i < index + span; i++)
					if (_rows[i].ActualHeight >= 0)
						actual += _rows[i].ActualHeight;
				return actual;
			}

			ColumnDefinition GetLastAutoColumn(BindableObject child)
			{
				int index = GetColumn(child);
				int span = GetColumnSpan(child);
				for (int i = index + span - 1; i >= index; i--)
					if (_columns[i].Width.IsAuto)
						return _columns[i];
				return null;
			}

			RowDefinition GetLastAutoRow(BindableObject child)
			{
				int index = GetRow(child);
				int span = GetRowSpan(child);
				for (int i = index + span - 1; i >= index; i--)
					if (_rows[i].Height.IsAuto)
						return _rows[i];
				return null;
			}

			double GetUnassignedHeight(double heightRequest, double rowSpacing)
			{
				double assigned = (_rows.Count - 1) * rowSpacing;
				for (var i = 0; i < _rows.Count; i++)
				{
					double actual = _rows[i].ActualHeight;
					if (actual >= 0)
						assigned += actual;
				}
				return heightRequest - assigned;
			}

			double GetUnassignedWidth(double widthRequest, double columnSpacing)
			{
				double assigned = (_columns.Count - 1) * columnSpacing;
				for (var i = 0; i < _columns.Count; i++)
				{
					double actual = _columns[i].ActualWidth;
					if (actual >= 0)
						assigned += actual;
				}
				return widthRequest - assigned;
			}

			#endregion
		}
	}
}