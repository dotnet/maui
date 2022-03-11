#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;

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
			var structure = _gridStructure ?? new GridStructure(Grid, bounds.Width, bounds.Height);

			foreach (var view in Grid)
			{
				if (view.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var cell = structure.GetCellBoundsFor(view, bounds.Left, bounds.Top);

				view.Arrange(cell);
			}

			var actual = new Size(structure.MeasuredGridWidth(), structure.MeasuredGridHeight());

			return actual.AdjustForFill(bounds, Grid);
		}

		class GridStructure
		{
			readonly IGridLayout _grid;
			readonly double _gridWidthConstraint;
			readonly double _gridHeightConstraint;
			readonly double _explicitGridHeight;
			readonly double _explicitGridWidth;
			readonly double _gridMaxHeight;
			readonly double _gridMinHeight;
			readonly double _gridMaxWidth;
			readonly double _gridMinWidth;

			Row[] _rows { get; }
			Column[] _columns { get; }
			IView[] _childrenToLayOut;
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

				_gridWidthConstraint = widthConstraint;
				_gridHeightConstraint = heightConstraint;

				_explicitGridHeight = _grid.Height;
				_explicitGridWidth = _grid.Width;
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

				if (_rowDefinitions.Count == 0)
				{
					// Since no rows are specified, we'll create an implied row 0 
					_rows = new Row[1];
					_rows[0] = new Row(new ImpliedRow());
				}
				else
				{
					_rows = new Row[_rowDefinitions.Count];

					for (int n = 0; n < _rowDefinitions.Count; n++)
					{
						_rows[n] = new Row(_rowDefinitions[n]);
					}
				}

				if (_columnDefinitions.Count == 0)
				{
					// Since no columns are specified, we'll create an implied column 0 
					_columns = new Column[1];
					_columns[0] = new Column(new ImpliedColumn());
				}
				else
				{
					_columns = new Column[_columnDefinitions.Count];

					for (int n = 0; n < _columnDefinitions.Count; n++)
					{
						_columns[n] = new Column(_columnDefinitions[n]);
					}
				}

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

			void InitializeCells()
			{
				// If the width/height constraints are infinity, then Star rows/columns won't really make any sense.
				// When that happens, we need to tag the cells so they can be measured as Auto cells instead.
				bool isGridWidthConstraintInfinite = double.IsInfinity(_gridWidthConstraint);
				bool isGridHeightConstraintInfinite = double.IsInfinity(_gridHeightConstraint);

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
						columnGridLengthType |= ToGridLengthType(_columns[columnIndex].ColumnDefinition.Width.GridUnitType);
					}

					var row = _grid.GetRow(view).Clamp(0, _rows.Length - 1);
					var rowSpan = _grid.GetRowSpan(view).Clamp(1, _rows.Length - row);

					var rowGridLengthType = GridLengthType.None;

					for (int rowIndex = row; rowIndex < row + rowSpan; rowIndex++)
					{
						rowGridLengthType |= ToGridLengthType(_rows[rowIndex].RowDefinition.Height.GridUnitType);
					}

					// Check for infinite constraints and Stars, so we can mark them for measurement as if they were Auto
					var measureStarAsAuto = (isGridHeightConstraintInfinite && IsStar(rowGridLengthType))
						|| (isGridWidthConstraintInfinite && IsStar(columnGridLengthType));

					_cells[n] = new Cell(n, row, column, rowSpan, columnSpan, columnGridLengthType, rowGridLengthType, measureStarAsAuto);
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
				var height = _explicitGridHeight > -1 ? _explicitGridHeight : GridHeight();

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
				var width = _explicitGridWidth > -1 ? _explicitGridWidth : GridWidth();

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

			double SumDefinitions(Definition[] definitions, double spacing)
			{
				double sum = 0;

				for (int n = 0; n < definitions.Length; n++)
				{
					var current = definitions[n].Size;

					if (current <= 0 && !definitions[n].IsStar)
					{
						continue;
					}

					sum += current;

					if (n > 0)
					{
						sum += spacing;
					}
				}

				return sum;
			}

			void MeasureCells()
			{
				for (int n = 0; n < _cells.Length; n++)
				{
					var cell = _cells[n];

					if (cell.ColumnGridLengthType == GridLengthType.Absolute
						&& cell.RowGridLengthType == GridLengthType.Absolute)
					{
						continue;
					}

					var availableWidth = AvailableWidth(cell);
					var availableHeight = AvailableHeight(cell);

					if (cell.IsColumnSpanAuto || cell.IsRowSpanAuto || cell.MeasureStarAsAuto)
					{
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

				ResolveSpans();

				ResolveStarColumns();
				ResolveStarRows();

				EnsureFinalMeasure();
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

			void ResolveSpan(Definition[] definitions, int start, int length, double spacing, double requestedSize)
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
						if (cellCheck(cell)) // Check whether this cell should count toward the type of star value were measuring
						{
							// Update the star width if the view in this cell is bigger
							starSize = Math.Max(starSize, dimension(_grid[cell.ViewIndex].DesiredSize));
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

			void ResolveStarColumns()
			{
				var availableSpace = _gridWidthConstraint - GridWidth();
				static bool cellCheck(Cell cell) => cell.IsColumnSpanStar;
				static double getDimension(Size size) => size.Width;

				ResolveStars(_columns, availableSpace, cellCheck, getDimension);
			}

			void ResolveStarRows()
			{
				var availableSpace = _gridHeightConstraint - GridHeight();
				static bool cellCheck(Cell cell) => cell.IsRowSpanStar;
				static double getDimension(Size size) => size.Height;

				ResolveStars(_rows, availableSpace, cellCheck, getDimension);
			}

			void EnsureFinalMeasure()
			{
				foreach (var cell in _cells)
				{
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

					_childrenToLayOut[cell.ViewIndex].Measure(width, height);
				}
			}

			double AvailableWidth(Cell cell)
			{
				var alreadyUsed = GridWidth();
				var available = _gridWidthConstraint - alreadyUsed;

				// Because our cell may overlap columns that are already measured (and counted in GridWidth()),
				// we'll need to add the size of those columns back into our available space
				double cellColumnsWidth = 0;

				for (int c = cell.Column; c < cell.Column + cell.ColumnSpan; c++)
				{
					cellColumnsWidth += _columns[c].Size;
				}

				cellColumnsWidth += (cell.ColumnSpan - 1) * _columnSpacing;

				return available + cellColumnsWidth;
			}

			double AvailableHeight(Cell cell)
			{
				var alreadyUsed = GridHeight();
				var available = _gridHeightConstraint - alreadyUsed;

				// Because our cell may overlap rows that are already measured (and counted in GridHeight()),
				// we'll need to add the size of those rows back into our available space
				double cellRowsHeight = 0;

				for (int c = cell.Row; c < cell.Row + cell.RowSpan; c++)
				{
					cellRowsHeight += _rows[c].Size;
				}

				cellRowsHeight += (cell.RowSpan - 1) * _rowSpacing;

				return available + cellRowsHeight;
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

			/// <summary>
			/// Marks the cell as requiring initial measurement even though the measurement type is Star
			/// Star measurements don't make sense when the axis constraint is infinity; when that happens, we treat them 
			/// Auto instead. We need to tag that situation in the Cell so the Auto measurement can happen; otherwise, we 
			/// can end up with un-measured controls when resolving the Star cells.
			/// </summary>
			public bool MeasureStarAsAuto { get; }

			public Cell(int viewIndex, int row, int column, int rowSpan, int columnSpan,
				GridLengthType columnGridLengthType, GridLengthType rowGridLengthType, bool measureStarAsAuto)
			{
				ViewIndex = viewIndex;
				Row = row;
				Column = column;
				RowSpan = rowSpan;
				ColumnSpan = columnSpan;
				ColumnGridLengthType = columnGridLengthType;
				RowGridLengthType = rowGridLengthType;
				MeasureStarAsAuto = measureStarAsAuto;
			}

			public bool IsColumnSpanAuto => HasFlag(ColumnGridLengthType, GridLengthType.Auto);
			public bool IsRowSpanAuto => HasFlag(RowGridLengthType, GridLengthType.Auto);
			public bool IsColumnSpanStar => HasFlag(ColumnGridLengthType, GridLengthType.Star);
			public bool IsRowSpanStar => HasFlag(RowGridLengthType, GridLengthType.Star);

			bool HasFlag(GridLengthType a, GridLengthType b)
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

		static bool IsStar(GridLengthType gridLengthType)
		{
			return (gridLengthType & GridLengthType.Star) == GridLengthType.Star;
		}

		abstract class Definition
		{
			public double Size { get; set; }

			public void Update(double size)
			{
				if (size > Size)
				{
					Size = size;
				}
			}

			public abstract bool IsAuto { get; }
			public abstract bool IsStar { get; }

			public abstract GridLength GridLength { get; }
		}

		class Column : Definition
		{
			public IGridColumnDefinition ColumnDefinition { get; set; }

			public override bool IsAuto => ColumnDefinition.Width.IsAuto;
			public override bool IsStar => ColumnDefinition.Width.IsStar;
			public override GridLength GridLength => ColumnDefinition.Width;

			public Column(IGridColumnDefinition columnDefinition)
			{
				ColumnDefinition = columnDefinition;
				if (columnDefinition.Width.IsAbsolute)
				{
					Size = columnDefinition.Width.Value;
				}
			}
		}

		class Row : Definition
		{
			public IGridRowDefinition RowDefinition { get; set; }

			public override bool IsAuto => RowDefinition.Height.IsAuto;
			public override bool IsStar => RowDefinition.Height.IsStar;
			public override GridLength GridLength => RowDefinition.Height;

			public Row(IGridRowDefinition rowDefinition)
			{
				RowDefinition = rowDefinition;
				if (rowDefinition.Height.IsAbsolute)
				{
					Size = rowDefinition.Height.Value;
				}
			}
		}

		// If the IGridLayout doesn't have any rows/columns defined, the manager will use an implied single row or column
		// in their place. 

		class ImpliedRow : IGridRowDefinition
		{
			public GridLength Height => GridLength.Star;
		}

		class ImpliedColumn : IGridColumnDefinition
		{
			public GridLength Width => GridLength.Star;
		}
	}
}
