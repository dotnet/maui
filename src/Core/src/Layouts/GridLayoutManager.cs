#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
			return new Size(_gridStructure.GridWidth(), _gridStructure.GridHeight());
		}

		public override void ArrangeChildren(Rectangle childBounds)
		{
			var structure = _gridStructure ?? new GridStructure(Grid, childBounds.Width, childBounds.Height);

			foreach (var view in Grid.Children)
			{
				if (view.Visibility == Visibility.Collapsed)
				{
					continue;
				}

				var cell = structure.GetCellBoundsFor(view);
				view.Arrange(cell);
			}
		}

		class GridStructure
		{
			readonly IGridLayout _grid;
			readonly double _gridWidthConstraint;
			readonly double _gridHeightConstraint;

			Row[] _rows { get; }
			Column[] _columns { get; }
			IView[] _children;
			Cell[] _cells { get; }

			readonly Dictionary<SpanKey, Span> _spans = new();

			public GridStructure(IGridLayout grid, double widthConstraint, double heightConstraint)
			{
				_grid = grid;
				_gridWidthConstraint = widthConstraint;
				_gridHeightConstraint = heightConstraint;

				if (_grid.RowDefinitions.Count == 0)
				{
					// Since no rows are specified, we'll create an implied row 0 
					_rows = new Row[1];
					_rows[0] = new Row(new ImpliedRow());
				}
				else
				{
					_rows = new Row[_grid.RowDefinitions.Count];

					for (int n = 0; n < _grid.RowDefinitions.Count; n++)
					{
						_rows[n] = new Row(_grid.RowDefinitions[n]);
					}
				}

				if (_grid.ColumnDefinitions.Count == 0)
				{
					// Since no columns are specified, we'll create an implied column 0 
					_columns = new Column[1];
					_columns[0] = new Column(new ImpliedColumn());
				}
				else
				{
					_columns = new Column[_grid.ColumnDefinitions.Count];

					for (int n = 0; n < _grid.ColumnDefinitions.Count; n++)
					{
						_columns[n] = new Column(_grid.ColumnDefinitions[n]);
					}
				}

				_children = _grid.Children.Where(child => child.Visibility != Visibility.Collapsed).ToArray();

				// We'll ignore any collapsed child views during layout
				_cells = new Cell[_children.Length];

				InitializeCells();

				MeasureCells();
			}

			void InitializeCells()
			{
				for (int n = 0; n < _children.Length; n++)
				{
					var view = _children[n];

					if (view.Visibility == Visibility.Collapsed)
					{
						continue;
					}

					var column = _grid.GetColumn(view);
					var columnSpan = _grid.GetColumnSpan(view);

					var columnGridLengthType = GridLengthType.None;

					for (int columnIndex = column; columnIndex < column + columnSpan; columnIndex++)
					{
						columnGridLengthType |= ToGridLengthType(_columns[columnIndex].ColumnDefinition.Width.GridUnitType);
					}

					var row = _grid.GetRow(view);
					var rowSpan = _grid.GetRowSpan(view);

					var rowGridLengthType = GridLengthType.None;

					for (int rowIndex = row; rowIndex < row + rowSpan; rowIndex++)
					{
						rowGridLengthType |= ToGridLengthType(_rows[rowIndex].RowDefinition.Height.GridUnitType);
					}

					_cells[n] = new Cell(n, row, column, rowSpan, columnSpan, columnGridLengthType, rowGridLengthType);
				}
			}

			public Rectangle GetCellBoundsFor(IView view)
			{
				var firstColumn = _grid.GetColumn(view);
				var lastColumn = firstColumn + _grid.GetColumnSpan(view);

				var firstRow = _grid.GetRow(view);
				var lastRow = firstRow + _grid.GetRowSpan(view);

				double top = TopEdgeOfRow(firstRow);
				double left = LeftEdgeOfColumn(firstColumn);

				double width = 0;

				for (int n = firstColumn; n < lastColumn; n++)
				{
					width += _columns[n].Size;
				}

				double height = 0;

				for (int n = firstRow; n < lastRow; n++)
				{
					height += _rows[n].Size;
				}

				// TODO ezhart this isn't correctly accounting for row spacing when spanning multiple rows
				// (and column spacing is probably wrong, too)

				return new Rectangle(left, top, width, height);
			}

			public double GridHeight()
			{
				return SumDefinitions(_rows, _grid.RowSpacing);
			}

			public double GridWidth()
			{
				return SumDefinitions(_columns, _grid.ColumnSpacing);
			}

			double SumDefinitions(Definition[] definitions, double spacing)
			{
				double sum = 0;

				for (int n = 0; n < definitions.Length; n++)
				{
					var current = definitions[n].Size;

					if (current <= 0)
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

					var availableWidth = _gridWidthConstraint - GridWidth();
					var availableHeight = _gridHeightConstraint - GridHeight();

					var measure = _children[cell.ViewIndex].Measure(availableWidth, availableHeight);

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

				ResolveSpans();

				ResolveStarColumns();
				ResolveStarRows();
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
						ResolveSpan(_columns, span.Start, span.Length, _grid.ColumnSpacing, span.Requested);
					}
					else
					{
						ResolveSpan(_rows, span.Start, span.Length, _grid.RowSpacing, span.Requested);
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
				double left = 0;

				for (int n = 0; n < column; n++)
				{
					left += _columns[n].Size;
					left += _grid.ColumnSpacing;
				}

				return left;
			}

			double TopEdgeOfRow(int row)
			{
				double top = 0;

				for (int n = 0; n < row; n++)
				{
					top += _rows[n].Size;
					top += _grid.RowSpacing;
				}

				return top;
			}

			void ResolveStars(Definition[] defs, double availableSpace, Func<Cell, bool> cellCheck, Func<Size, double> dimension)
			{
				// Count up the total weight of star columns (e.g., "*, 3*, *" == 5)

				var starCount = 0;

				foreach (var definition in defs)
				{
					if (definition.IsStar)
					{
						starCount += (int)definition.GridLength.Value;
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
							starSize = Math.Max(starSize, dimension(_grid.Children[cell.ViewIndex].DesiredSize));
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
						definition.Size = starSize * (int)definition.GridLength.Value;
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

			private void ResolveStarRows()
			{
				var availableSpace = _gridHeightConstraint - GridHeight();
				static bool cellCheck(Cell cell) => cell.IsRowSpanStar;
				static double getDimension(Size size) => size.Height;

				ResolveStars(_rows, availableSpace, cellCheck, getDimension);
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

			public GridLengthType ColumnGridLengthType { get; }
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
