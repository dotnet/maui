using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Layouts
{
	public class GridLayoutManager : LayoutManager
	{
		public GridLayoutManager(IGridLayout layout) : base(layout)
		{
			Grid = layout;
		}

		public IGridLayout Grid { get; }

		public override Size Measure(double widthConstraint, double heightConstraint)
		{
			var structure = new GridStructure(Grid, widthConstraint, heightConstraint);

			return new Size(structure.GridWidth(), structure.GridHeight());
		}

		public override void ArrangeChildren(Rectangle childBounds)
		{
			var structure = new GridStructure(Grid, childBounds.Width, childBounds.Height);

			foreach (var view in Grid.Children)
			{
				var cell = structure.ComputeFrameFor(view);
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
			Cell[] _cells { get; }

			readonly Dictionary<SpanKey, Span> _spans = new Dictionary<SpanKey, Span>();

			public GridStructure(IGridLayout grid, double widthConstraint, double heightConstraint)
			{
				_grid = grid;
				_gridWidthConstraint = widthConstraint;
				_gridHeightConstraint = heightConstraint;
				_rows = new Row[_grid.RowDefinitions.Count];

				for (int n = 0; n < _grid.RowDefinitions.Count; n++)
				{
					_rows[n] = new Row(_grid.RowDefinitions[n]);
				}

				_columns = new Column[_grid.ColumnDefinitions.Count];

				for (int n = 0; n < _grid.ColumnDefinitions.Count; n++)
				{
					_columns[n] = new Column(_grid.ColumnDefinitions[n]);
				}

				_cells = new Cell[_grid.Children.Count];

				InitializeCells();

				MeasureCells();
			}

			void InitializeCells()
			{
				for (int n = 0; n < _grid.Children.Count; n++)
				{
					var view = _grid.Children[n];
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

			public Rectangle ComputeFrameFor(IView view)
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

					var measure = _grid.Children[cell.ViewIndex].Measure(availableWidth, availableHeight);

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
		}

		class Span
		{
			public int Start { get; }
			public int Length { get; }
			public bool IsColumn { get; }
			public double Requested { get; }

			public SpanKey Key { get; }

			public Span(int start, int length, bool isColumn, double value)
			{
				Start = start;
				Length = length;
				IsColumn = isColumn;
				Requested = value;

				Key = new SpanKey(Start, Length, IsColumn);
			}
		}

		class SpanKey
		{
			public SpanKey(int start, int length, bool isColumn)
			{
				Start = start;
				Length = length;
				IsColumn = isColumn;
			}

			public int Start { get; }
			public int Length { get; }
			public bool IsColumn { get; }

			public override bool Equals(object? obj)
			{
				return obj is SpanKey key &&
					   Start == key.Start &&
					   Length == key.Length &&
					   IsColumn == key.IsColumn;
			}

			public override int GetHashCode()
			{
				return Start.GetHashCode() ^ Length.GetHashCode() ^ IsColumn.GetHashCode();
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
		}

		class Column : Definition
		{
			public IGridColumnDefinition ColumnDefinition { get; set; }

			public override bool IsAuto => ColumnDefinition.Width.IsAuto;

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

			public Row(IGridRowDefinition rowDefinition)
			{
				RowDefinition = rowDefinition;
				if (rowDefinition.Height.IsAbsolute)
				{
					Size = rowDefinition.Height.Value;
				}
			}
		}
	}
}
