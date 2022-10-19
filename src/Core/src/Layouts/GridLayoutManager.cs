#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Layouts
{
	public class GridLayoutManager : LayoutManager
	{
		GridStructure? _gridStructure;

		public GridLayoutManager(IGridLayout layout) : base(layout)
		{
			Grid = layout;
		}

		public IGridLayout Grid { get; }

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			_gridStructure = new GridStructure(Grid, widthConstraint, heightConstraint);

			var measuredWidth = _gridStructure.MeasuredGridWidth();
			var measuredHeight = _gridStructure.MeasuredGridHeight();

			return new Size(measuredWidth, measuredHeight);
		}

		public override Size ArrangeChildren(Rect bounds)
		{
			_gridStructure ??= new GridStructure(Grid, bounds.Width, bounds.Height);

			_gridStructure.DecompressStars(bounds.Size);

			foreach (var view in Grid)
			{
				if (view.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var cell = _gridStructure.GetCellBoundsFor(view, bounds.Left, bounds.Top);
				view.Arrange(cell);
			}

			var actual = new Size(_gridStructure.MeasuredGridWidth(), _gridStructure.MeasuredGridHeight());

			return actual.AdjustForFill(bounds, Grid);
		}

		class GridStructure
		{
			public double HeightConstraint => _gridHeightConstraint;
			public double WidthConstraint => _gridWidthConstraint;

			readonly IGridLayout _grid;
			readonly double _gridWidthConstraint;
			readonly double _gridHeightConstraint;

			readonly double _explicitGridHeight;
			readonly double _explicitGridWidth;
			readonly double _gridMaxHeight;
			readonly double _gridMinHeight;
			readonly double _gridMaxWidth;
			readonly double _gridMinWidth;

			Definition[] _rows { get; }
			Definition[] _columns { get; }

			readonly IView[] _childrenToLayOut;
			Cell[] _cells { get; }

			readonly Thickness _padding;
			readonly double _rowSpacing;
			readonly double _columnSpacing;
			readonly IReadOnlyList<IGridRowDefinition> _rowDefinitions;
			readonly IReadOnlyList<IGridColumnDefinition> _columnDefinitions;

			readonly Dictionary<SpanKey, Span> _spans = new();

			public GridStructure(IGridLayout grid, double widthConstraint, double heightConstraint)
			{
				_grid = grid;

				_explicitGridHeight = _grid.Height;
				_explicitGridWidth = _grid.Width;

				_gridWidthConstraint = Dimension.IsExplicitSet(_explicitGridWidth) ? _explicitGridWidth : widthConstraint;
				_gridHeightConstraint = Dimension.IsExplicitSet(_explicitGridHeight) ? _explicitGridHeight : heightConstraint;

				_gridMaxHeight = _grid.MaximumHeight;
				_gridMinHeight = _grid.MinimumHeight;
				_gridMaxWidth = _grid.MaximumWidth;
				_gridMinWidth = _grid.MinimumWidth;

				// Cache these GridLayout properties so we don't have to keep looking them up via _grid
				// (Property access via _grid may have performance implications for some SDKs.)
				_padding = grid.Padding;
				_columnSpacing = grid.ColumnSpacing;
				_rowSpacing = grid.RowSpacing;
				_rowDefinitions = grid.RowDefinitions;
				_columnDefinitions = grid.ColumnDefinitions;

				_rows = InitializeRows();
				_columns = InitializeColumns();

				// We could work out the _childrenToLayOut array (with the Collapsed items filtered out) with a Linq 1-liner
				// but doing it the hard way means we don't allocate extra enumerators, especially if we're in the 
				// happy path where _none_ of the children are Collapsed.
				var gridChildCount = _grid.Count;

				_childrenToLayOut = new IView[gridChildCount];
				int currentChild = 0;
				for (int n = 0; n < gridChildCount; n++)
				{
					if (_grid[n].Visibility != Visibility.Collapsed)
					{
						_childrenToLayOut[currentChild] = _grid[n];
						currentChild += 1;
					}
				}

				if (currentChild < gridChildCount)
				{
					Array.Resize(ref _childrenToLayOut, currentChild);
				}

				// We'll ignore any collapsed child views during layout
				_cells = new Cell[_childrenToLayOut.Length];

				InitializeCells();

				MeasureCells();
			}

			static Definition[] Implied()
			{
				return new Definition[]
				{
					new Definition(GridLength.Star)
				};
			}

			Definition[] InitializeRows()
			{
				int count = _rowDefinitions.Count;

				if (count == 0)
				{
					// Since no rows are specified, we'll create an implied row 0 
					return Implied();
				}

				var rows = new Definition[count];

				for (int n = 0; n < count; n++)
				{
					var definition = _rowDefinitions[n];
					rows[n] = new Definition(definition.Height);
				}

				return rows;
			}

			Definition[] InitializeColumns()
			{
				int count = _columnDefinitions.Count;

				if (count == 0)
				{
					// Since no columns are specified, we'll create an implied column 0 
					return Implied();
				}

				var definitions = new Definition[count];

				for (int n = 0; n < count; n++)
				{
					var definition = _columnDefinitions[n];
					definitions[n] = new Definition(definition.Width);
				}

				return definitions;
			}

			void InitializeCells()
			{
				for (int n = 0; n < _childrenToLayOut.Length; n++)
				{
					var view = _childrenToLayOut[n];

					if (view.Visibility == Visibility.Collapsed)
					{
						continue;
					}

					var column = _grid.GetColumn(view).Clamp(0, _columns.Length - 1);
					var columnSpan = _grid.GetColumnSpan(view).Clamp(1, _columns.Length - column);

					var columnGridLengthType = GridLengthType.None;

					for (int columnIndex = column; columnIndex < column + columnSpan; columnIndex++)
					{
						columnGridLengthType |= ToGridLengthType(_columns[columnIndex].GridLength.GridUnitType);
					}

					var row = _grid.GetRow(view).Clamp(0, _rows.Length - 1);
					var rowSpan = _grid.GetRowSpan(view).Clamp(1, _rows.Length - row);

					var rowGridLengthType = GridLengthType.None;

					for (int rowIndex = row; rowIndex < row + rowSpan; rowIndex++)
					{
						rowGridLengthType |= ToGridLengthType(_rows[rowIndex].GridLength.GridUnitType);
					}

					_cells[n] = new Cell(n, row, column, rowSpan, columnSpan, columnGridLengthType, rowGridLengthType);
				}
			}

			public Rect GetCellBoundsFor(IView view, double xOffset, double yOffset)
			{
				var firstColumn = _grid.GetColumn(view).Clamp(0, _columns.Length - 1);
				var columnSpan = _grid.GetColumnSpan(view).Clamp(1, _columns.Length - firstColumn);
				var lastColumn = firstColumn + columnSpan;

				var firstRow = _grid.GetRow(view).Clamp(0, _rows.Length - 1);
				var rowSpan = _grid.GetRowSpan(view).Clamp(1, _rows.Length - firstRow);
				var lastRow = firstRow + rowSpan;

				double top = TopEdgeOfRow(firstRow);
				double left = LeftEdgeOfColumn(firstColumn);

				double width = 0;
				double height = 0;

				for (int n = firstColumn; n < lastColumn; n++)
				{
					width += _columns[n].Size;
				}

				for (int n = firstRow; n < lastRow; n++)
				{
					height += _rows[n].Size;
				}

				// Account for any space between spanned rows/columns
				width += (columnSpan - 1) * _columnSpacing;
				height += (rowSpan - 1) * _rowSpacing;

				return new Rect(left + xOffset, top + yOffset, width, height);
			}

			public double GridHeight()
			{
				return SumDefinitions(_rows, _rowSpacing) + _padding.VerticalThickness;
			}

			public double GridWidth()
			{
				return SumDefinitions(_columns, _columnSpacing) + _padding.HorizontalThickness;
			}

			public double MeasuredGridHeight()
			{
				var height = Dimension.IsExplicitSet(_explicitGridHeight) ? _explicitGridHeight : GridHeight();

				if (_gridMaxHeight >= 0 && height > _gridMaxHeight)
				{
					height = _gridMaxHeight;
				}

				if (_gridMinHeight >= 0 && height < _gridMinHeight)
				{
					height = _gridMinHeight;
				}

				return height;
			}

			public double MeasuredGridWidth()
			{
				var width = Dimension.IsExplicitSet(_explicitGridWidth) ? _explicitGridWidth : GridWidth();

				if (_gridMaxWidth >= 0 && width > _gridMaxWidth)
				{
					width = _gridMaxWidth;
				}

				if (_gridMinWidth >= 0 && width < _gridMinWidth)
				{
					width = _gridMinWidth;
				}

				return width;
			}

			static double SumDefinitions(Definition[] definitions, double spacing)
			{
				double sum = 0;

				for (int n = 0; n < definitions.Length; n++)
				{
					sum += definitions[n].Size;

					if (n > 0)
					{
						sum += spacing;
					}
				}

				return sum;
			}

			void MeasureCells()
			{
				// Do the initial pass for all the auto/star stuff
				MeasureCellsWithUnknowns();

				ResolveStarColumns(_gridWidthConstraint);
				ResolveStarRows(_gridHeightConstraint);

				// Measure the content for cells where we know the dimensions
				MeasureKnownCells();

				ResolveSpans();

				// Compress the star values to their minimums for measurement 
				CompressStarMeasurements();
			}

			void MeasureCellsWithUnknowns()
			{
				for (int n = 0; n < _cells.Length; n++)
				{
					var cell = _cells[n];

					if (cell.IsAbsolute)
					{
						// This cell is entirely within rows/columns with absolute sizes; we don't need to measure
						// it to figure out those sizes
						continue;
					}

					var availableWidth = cell.IsColumnSpanAuto ? double.PositiveInfinity : AvailableWidth(cell);
					var availableHeight = cell.IsRowSpanAuto ? double.PositiveInfinity : AvailableHeight(cell);

					var measure = _childrenToLayOut[cell.ViewIndex].Measure(availableWidth, availableHeight);

					if (cell.IsColumnSpanAuto)
					{
						if (cell.ColumnSpan == 1)
						{
							_columns[cell.Column].Update(measure.Width);
						}
						else
						{
							var span = new Span(cell.Column, cell.ColumnSpan, true, measure.Width);
							TrackSpan(span);
						}
					}

					if (cell.IsRowSpanAuto)
					{
						if (cell.RowSpan == 1)
						{
							_rows[cell.Row].Update(measure.Height);
						}
						else
						{
							var span = new Span(cell.Row, cell.RowSpan, false, measure.Height);
							TrackSpan(span);
						}
					}
				}
			}

			void TrackSpan(Span span)
			{
				if (_spans.TryGetValue(span.Key, out Span? otherSpan))
				{
					// This span may replace an equivalent but smaller span
					if (span.Requested > otherSpan.Requested)
					{
						_spans[span.Key] = span;
					}
				}
				else
				{
					_spans[span.Key] = span;
				}
			}

			void ResolveSpans()
			{
				foreach (var span in _spans.Values)
				{
					if (span.IsColumn)
					{
						ResolveSpan(_columns, span.Start, span.Length, _columnSpacing, span.Requested);
					}
					else
					{
						ResolveSpan(_rows, span.Start, span.Length, _rowSpacing, span.Requested);
					}
				}
			}

			static void ResolveSpan(Definition[] definitions, int start, int length, double spacing, double requestedSize)
			{
				double currentSize = 0;
				var end = start + length;

				// Determine how large the spanned area currently is
				for (int n = start; n < end; n++)
				{
					currentSize += definitions[n].Size;

					if (n > start)
					{
						currentSize += spacing;
					}
				}

				if (requestedSize <= currentSize)
				{
					// If our request fits in the current size, we're good
					return;
				}

				// Figure out how much more space we need in this span
				double required = requestedSize - currentSize;

				// And how many parts of the span to distribute that space over
				int autoCount = 0;
				for (int n = start; n < end; n++)
				{
					if (definitions[n].IsAuto)
					{
						autoCount += 1;
					}
					else if (definitions[n].IsStar)
					{
						// Ah, part of this span is a Star; that means it doesn't count
						// for sizing the Auto parts of the span at all. We can just cut out now.
						return;
					}
				}

				double distribution = required / autoCount;

				// And distribute that over the rows/columns in the span
				for (int n = start; n < end; n++)
				{
					if (definitions[n].IsAuto)
					{
						definitions[n].Size += distribution;
					}
				}
			}

			double LeftEdgeOfColumn(int column)
			{
				double left = _padding.Left;

				for (int n = 0; n < column; n++)
				{
					left += _columns[n].Size;
					left += _columnSpacing;
				}

				return left;
			}

			double TopEdgeOfRow(int row)
			{
				double top = _padding.Top;

				for (int n = 0; n < row; n++)
				{
					top += _rows[n].Size;
					top += _rowSpacing;
				}

				return top;
			}

			void ResolveStars(Definition[] defs, double availableSpace, Func<Cell, bool> cellCheck, Func<Size, double> dimension)
			{
				// Count up the total weight of star columns (e.g., "*, 3*, *" == 5)

				var starCount = 0.0;

				foreach (var definition in defs)
				{
					if (definition.IsStar)
					{
						starCount += definition.GridLength.Value;
					}
				}

				if (starCount == 0)
				{
					return;
				}

				double starSize = 0;

				if (double.IsInfinity(availableSpace))
				{
					// If the available space we're measuring is infinite, then the 'star' doesn't really mean anything
					// (each one would be infinite). So instead we'll use the size of the actual view in the star row/column.
					// This means that an empty star row/column goes to zero if the available space is infinite. 

					foreach (var cell in _cells)
					{
						if (cellCheck(cell)) // Check whether this cell should count toward the type of star value we're measuring
						{
							// Update the star width if the view in this cell is bigger
							starSize = Math.Max(starSize, dimension(_childrenToLayOut[cell.ViewIndex].DesiredSize));
						}
					}
				}
				else
				{
					// If we have a finite space, we can divvy it up among the full star weight
					starSize = availableSpace / starCount;
				}

				foreach (var definition in defs)
				{
					if (definition.IsStar)
					{
						// Give the star row/column the appropriate portion of the space based on its weight
						definition.Size = starSize * definition.GridLength.Value;
					}
				}
			}

			void ResolveStarColumns(double widthConstraint)
			{
				var availableSpace = widthConstraint - GridWidth();
				static bool cellCheck(Cell cell) => cell.IsColumnSpanStar;
				static double getDimension(Size size) => size.Width;

				ResolveStars(_columns, availableSpace, cellCheck, getDimension);
			}

			void ResolveStarRows(double heightConstraint)
			{
				var availableSpace = heightConstraint - GridHeight();
				static bool cellCheck(Cell cell) => cell.IsRowSpanStar;
				static double getDimension(Size size) => size.Height;

				ResolveStars(_rows, availableSpace, cellCheck, getDimension);
			}

			void MeasureKnownCells()
			{
				foreach (var cell in _cells)
				{
					if (!cell.NeedsKnownMeasurePass)
					{
						continue;
					}

					double width = 0;
					double height = 0;

					for (int n = cell.Row; n < cell.Row + cell.RowSpan; n++)
					{
						height += _rows[n].Size;
					}

					for (int n = cell.Column; n < cell.Column + cell.ColumnSpan; n++)
					{
						width += _columns[n].Size;
					}

					if (width == 0 || height == 0)
					{
						continue;
					}

					var measure = _childrenToLayOut[cell.ViewIndex].Measure(width, height);

					if (cell.IsColumnSpanStar && cell.ColumnSpan > 1)
					{
						var span = new Span(cell.Column, cell.ColumnSpan, true, measure.Width);
						TrackSpan(span);
					}

					if (cell.IsRowSpanStar && cell.RowSpan > 1)
					{
						var span = new Span(cell.Row, cell.RowSpan, false, measure.Height);
						TrackSpan(span);
					}
				}
			}

			double AvailableWidth(Cell cell)
			{
				// Because our cell may overlap columns that are already measured (and counted in GridWidth()),
				// we'll need to add the size of those columns back into our available space
				double cellColumnsWidth = 0;

				// So we'll have to tally up the known widths of those rows. While we do that, we'll
				// keep track of whether all the columns spanned by this cell are absolute widths
				bool absolute = true;

				for (int c = cell.Column; c < cell.Column + cell.ColumnSpan; c++)
				{
					cellColumnsWidth += _columns[c].Size;

					if (!_columns[c].IsAbsolute)
					{
						absolute = false;
					}
				}

				cellColumnsWidth += (cell.ColumnSpan - 1) * _columnSpacing;

				if (absolute)
				{
					// If all the spanned columns were absolute, then we know the exact available width for 
					// the view that's in this cell
					return cellColumnsWidth;
				}

				// Since some of the columns weren't already specified, we'll need to work out what's left
				// of the Grid's width for this cell

				var alreadyUsed = GridWidth();
				var available = _gridWidthConstraint - alreadyUsed;

				return available + cellColumnsWidth;
			}

			double AvailableHeight(Cell cell)
			{
				// Because our cell may overlap rows that are already measured (and counted in GridHeight()),
				// we'll need to add the size of those rows back into our available space
				double cellRowsHeight = 0;

				// So we'll have to tally up the known heights of those rows. While we do that, we'll
				// keep track of whether all the rows spanned by this cell are absolute heights
				bool absolute = true;

				for (int c = cell.Row; c < cell.Row + cell.RowSpan; c++)
				{
					cellRowsHeight += _rows[c].Size;

					if (!_rows[c].IsAbsolute)
					{
						absolute = false;
					}
				}

				cellRowsHeight += (cell.RowSpan - 1) * _rowSpacing;

				if (absolute)
				{
					// If all the spanned rows were absolute, then we know the exact available height for 
					// the view that's in this cell
					return cellRowsHeight;
				}

				// Since some of the rows weren't already specified, we'll need to work out what's left
				// of the Grid's height for this cell

				var alreadyUsed = GridHeight();
				var available = _gridHeightConstraint - alreadyUsed;

				return available + cellRowsHeight;
			}

			public void DecompressStars(Size targetSize)
			{
				if (_grid.VerticalLayoutAlignment == LayoutAlignment.Fill || Dimension.IsExplicitSet(_explicitGridHeight))
				{
					// Reset the size on all star rows
					ZeroOutStarSizes(_rows);

					// And compute them for the actual arrangement height
					ResolveStarRows(targetSize.Height);
				}

				if (_grid.HorizontalLayoutAlignment == LayoutAlignment.Fill || Dimension.IsExplicitSet(_explicitGridWidth))
				{
					// Reset the size on all star columns
					ZeroOutStarSizes(_columns);

					// And compute them for the actual arrangement width
					ResolveStarColumns(targetSize.Width);
				}
			}

			void CompressStarMeasurements()
			{
				CompressStarRows();
				CompressStarColumns();
			}

			void CompressStarRows()
			{
				var copy = ScratchCopy(_rows);

				// Iterate over the cells and inflate the star row sizes in the copy
				// to the minimum required in order to contain the cells

				for (int n = 0; n < _cells.Length; n++)
				{
					var cell = _cells[n];

					if (!cell.IsRowSpanStar)
					{
						// This cell doesn't span any star rows, nothing to do
						continue;
					}

					var start = cell.Row;
					var end = start + cell.RowSpan;

					var desiredHeight = Math.Min(_gridHeightConstraint, _childrenToLayOut[cell.ViewIndex].DesiredSize.Height);

					ExpandStarsInSpan(desiredHeight, _rows, copy, start, end);
				}

				UpdateStarSizes(_rows, copy);
			}

			void CompressStarColumns()
			{
				var copy = ScratchCopy(_columns);

				// Iterate over the cells and inflate the star column sizes in the copy
				// to the minimum required in order to contain the cells

				for (int n = 0; n < _cells.Length; n++)
				{
					var cell = _cells[n];

					if (!cell.IsColumnSpanStar)
					{
						// This cell doesn't span any star columns, nothing to do
						continue;
					}

					var start = cell.Column;
					var end = start + cell.ColumnSpan;

					var cellRequiredWidth = Math.Min(_gridWidthConstraint, _childrenToLayOut[cell.ViewIndex].DesiredSize.Width);

					ExpandStarsInSpan(cellRequiredWidth, _columns, copy, start, end);
				}

				UpdateStarSizes(_columns, copy);
			}

			static Definition[] ScratchCopy(Definition[] original)
			{
				var copy = new Definition[original.Length];

				for (int n = 0; n < original.Length; n++)
				{
					copy[n] = new Definition(original[n].GridLength)
					{
						Size = original[n].Size
					};
				}

				// zero out the star sizes in the copy
				ZeroOutStarSizes(copy);

				return copy;
			}

			static void ExpandStarsInSpan(double spaceNeeded, Definition[] original, Definition[] updated, int start, int end)
			{
				// Remove the parts of spaceNeeded which are already covered by explicit and auto columns in the span
				for (int n = start; n < end; n++)
				{
					if (original[n].IsAbsolute || original[n].IsAuto)
					{
						spaceNeeded -= original[n].Size;
					}
				}

				// Determine how much space the star sizes in the span are already requesting
				// (because of other overlapping cells)

				double spaceAvailable = 0;
				int starCount = 0;
				for (int n = start; n < end; n++)
				{
					if (updated[n].IsStar)
					{
						starCount += 1;
						spaceAvailable += updated[n].Size;
					}
				}

				// If previous inflations from other cells haven't given us enough room,
				// distribute the amount of space we still need evenly across the stars in the span
				if (spaceAvailable < spaceNeeded)
				{
					var toAdd = (spaceNeeded - spaceAvailable) / starCount;
					for (int n = start; n < end; n++)
					{
						if (updated[n].IsStar)
						{
							updated[n].Update(toAdd);
						}
					}
				}
			}

			static void ZeroOutStarSizes(Definition[] definitions)
			{
				for (int n = 0; n < definitions.Length; n++)
				{
					var definition = definitions[n];
					if (definition.IsStar)
					{
						definition.Size = 0;
					}
				}
			}

			static void UpdateStarSizes(Definition[] original, Definition[] updated)
			{
				for (int n = 0; n < updated.Length; n++)
				{
					if (!updated[n].IsStar)
					{
						continue;
					}

					original[n].Size = updated[n].Size;
				}
			}
		}

		// Dictionary key for tracking a Span
		record SpanKey(int Start, int Length, bool IsColumn);

		class Span
		{
			public int Start { get; }
			public int Length { get; }
			public bool IsColumn { get; }
			public double Requested { get; }

			public SpanKey Key { get; }

			public Span(int start, int length, bool isColumn, double requestedLength)
			{
				Start = start;
				Length = length;
				IsColumn = isColumn;
				Requested = requestedLength;

				Key = new SpanKey(Start, Length, IsColumn);
			}
		}

		class Cell
		{
			public int ViewIndex { get; }
			public int Row { get; }
			public int Column { get; }
			public int RowSpan { get; }
			public int ColumnSpan { get; }

			/// <summary>
			/// A combination of all the measurement types in the columns this cell spans
			/// </summary>
			public GridLengthType ColumnGridLengthType { get; }

			/// <summary>
			/// A combination of all the measurement types in the rows this cell spans
			/// </summary>
			public GridLengthType RowGridLengthType { get; }

			public Cell(int viewIndex, int row, int column, int rowSpan, int columnSpan,
				GridLengthType columnGridLengthType, GridLengthType rowGridLengthType)
			{
				ViewIndex = viewIndex;
				Row = row;
				Column = column;
				RowSpan = rowSpan;
				ColumnSpan = columnSpan;
				ColumnGridLengthType = columnGridLengthType;
				RowGridLengthType = rowGridLengthType;
			}

			public bool IsColumnSpanAuto => HasFlag(ColumnGridLengthType, GridLengthType.Auto);
			public bool IsRowSpanAuto => HasFlag(RowGridLengthType, GridLengthType.Auto);
			public bool IsColumnSpanStar => HasFlag(ColumnGridLengthType, GridLengthType.Star);
			public bool IsRowSpanStar => HasFlag(RowGridLengthType, GridLengthType.Star);
			public bool IsAbsolute => ColumnGridLengthType == GridLengthType.Absolute
						&& RowGridLengthType == GridLengthType.Absolute;

			// If any part of the Cell's spans are Absolute or Star, then the Cell will need a measure pass at the known
			// size once that's been determined (i.e., when the Auto stuff has been worked out)
			public bool NeedsKnownMeasurePass => ((ColumnGridLengthType | RowGridLengthType) ^ GridLengthType.Auto) > 0;

			static bool HasFlag(GridLengthType a, GridLengthType b)
			{
				// Avoiding Enum.HasFlag here for performance reasons; we don't need the type check
				return (a & b) == b;
			}
		}

		[Flags]
		enum GridLengthType
		{
			None = 0,
			Absolute = 1,
			Auto = 2,
			Star = 4
		}

		static GridLengthType ToGridLengthType(GridUnitType gridUnitType)
		{
			return gridUnitType switch
			{
				GridUnitType.Absolute => GridLengthType.Absolute,
				GridUnitType.Star => GridLengthType.Star,
				GridUnitType.Auto => GridLengthType.Auto,
				_ => GridLengthType.None,
			};
		}

		class Definition
		{
			readonly GridLength _gridLength;
			public double Size { get; set; }

			public void Update(double size)
			{
				if (size > Size)
				{
					Size = size;
				}
			}

			public bool IsAuto => _gridLength.IsAuto;
			public bool IsStar => _gridLength.IsStar;
			public bool IsAbsolute => _gridLength.IsAbsolute;

			public GridLength GridLength => _gridLength;

			public Definition(GridLength gridLength)
			{
				if (gridLength.IsAbsolute)
				{
					Size = gridLength.Value;
				}

				_gridLength = gridLength;
			}
		}
	}
}
