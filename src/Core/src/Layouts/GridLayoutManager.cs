#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

			_gridStructure.PrepareForArrange(bounds.Size);

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
			readonly IGridLayout _grid;
			readonly double _gridWidthConstraint;
			readonly double _gridHeightConstraint;

			readonly double _explicitGridHeight;
			readonly double _explicitGridWidth;
			readonly double _gridMaxHeight;
			readonly double _gridMinHeight;
			readonly double _gridMaxWidth;
			readonly double _gridMinWidth;

			readonly bool _isStarWidthPrecomputable;
			readonly bool _isStarHeightPrecomputable;

			readonly double _rowStarCount;
			readonly double _columnStarCount;

			Definition[] _rows { get; }
			Definition[] _columns { get; }

			readonly IView[] _childrenToLayOut;
			Cell[] _cells { get; }

			readonly Thickness _padding;
			readonly double _rowSpacing;
			readonly double _columnSpacing;

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

				_rows = GridStructure.InitializeRows(grid.RowDefinitions);
				_columns = GridStructure.InitializeColumns(grid.ColumnDefinitions);

				_rowStarCount = CountStars(_rows);
				_columnStarCount = CountStars(_columns);

				// Determine whether we can figure out the * values before doing any measurements
				// i.e., are there any Auto values in the relevant dimensions, and are we working 
				// with fixed constraints on the Grid itself
				_isStarHeightPrecomputable = IsStarHeightPrecomputable();
				_isStarWidthPrecomputable = IsStarWidthPrecomputable();

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
				return
				[
					new Definition(GridLength.Star)
				];
			}

			static Definition[] InitializeRows(IReadOnlyList<IGridRowDefinition> rowDefinitions)
			{
				int count = rowDefinitions.Count;

				if (count == 0)
				{
					// Since no rows are specified, we'll create an implied row 0 
					return Implied();
				}

				var rows = new Definition[count];

				for (int n = 0; n < count; n++)
				{
					var definition = rowDefinitions[n];
					rows[n] = new Definition(definition.Height);
				}

				return rows;
			}

			static Definition[] InitializeColumns(IReadOnlyList<IGridColumnDefinition> columnDefinitions)
			{
				int count = columnDefinitions.Count;

				if (count == 0)
				{
					// Since no columns are specified, we'll create an implied column 0 
					return Implied();
				}

				var definitions = new Definition[count];

				for (int n = 0; n < count; n++)
				{
					var definition = columnDefinitions[n];
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

					var cell = new Cell(n, row, column, rowSpan, columnSpan, columnGridLengthType, rowGridLengthType);

					// We may have enough info at this point to determine some of the measurement constraints for cells
					DetermineCellMeasureWidth(cell);
					DetermineCellMeasureHeight(cell);

					_cells[n] = cell;
				}

				if (_isStarWidthPrecomputable)
				{
					// We have enough information to go ahead and work out the sizes of the * columns
					ResolveStarColumns(_gridWidthConstraint);
				}

				if (_isStarHeightPrecomputable)
				{
					// We have enough information to go ahead and work out the sizes of the * rows
					ResolveStarRows(_gridHeightConstraint);
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
					width += _columns[n].Size.Dp;
				}

				for (int n = firstRow; n < lastRow; n++)
				{
					height += _rows[n].Size.Dp;
				}

				// Account for any space between spanned rows/columns
				width += (columnSpan - 1) * _columnSpacing;
				height += (rowSpan - 1) * _rowSpacing;

				return new Rect(left + xOffset, top + yOffset, width, height);
			}

			double GridHeight()
			{
				return SumDefinitions(_rows, _rowSpacing) + _padding.VerticalThickness;
			}

			double GridWidth()
			{
				return SumDefinitions(_columns, _columnSpacing) + _padding.HorizontalThickness;
			}

			double GridMinimumHeight()
			{
				return SumDefinitions(_rows, _rowSpacing, minimize: true) + _padding.VerticalThickness;
			}

			double GridMinimumWidth()
			{
				return SumDefinitions(_columns, _columnSpacing, minimize: true) + _padding.HorizontalThickness;
			}

			public double MeasuredGridHeight()
			{
				var height = Dimension.IsExplicitSet(_explicitGridHeight) ? _explicitGridHeight : GridMinimumHeight();

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
				var width = Dimension.IsExplicitSet(_explicitGridWidth) ? _explicitGridWidth : GridMinimumWidth();

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

			static double SumDefinitions(Definition[] definitions, double spacing, bool minimize = false)
			{
				double sum = 0;

				for (int n = 0; n < definitions.Length; n++)
				{
					sum += minimize ? definitions[n].MinimumSize.Dp : definitions[n].Size.Dp;

					if (n > 0)
					{
						sum += spacing;
					}
				}

				return sum;
			}

			void MeasureCells()
			{
				FirstMeasurePass();

				if (!_isStarWidthPrecomputable)
				{
					// We didn't have enough info to work out the * column sizes earlier, but now that
					// we've measured the Auto dimensions, we can finalize those values
					ResolveStarColumns(_gridWidthConstraint);
				}

				if (!_isStarHeightPrecomputable)
				{
					// We didn't have enough info to work out the * row sizes earlier, but now that
					// we've measured the Auto dimensions, we can finalize those values
					ResolveStarRows(_gridHeightConstraint);
				}

				SecondMeasurePass();

				ResolveSpans();

				// Compress the star values to their minimums for measurement 
				MinimizeStarsForMeasurement();
			}

			Size MeasureCell(Cell cell, double width, double height)
			{
				var child = _childrenToLayOut[cell.ViewIndex];
				var result = child.Measure(width, height);
				return result;
			}

			void FirstMeasurePass()
			{
				for (int n = 0; n < _cells.Length; n++)
				{
					var cell = _cells[n];

					bool treatCellHeightAsAuto = TreatCellHeightAsAuto(cell);
					bool treatCellWidthAsAuto = TreatCellWidthAsAuto(cell);

					if (double.IsNaN(cell.MeasureHeight.Dp) || double.IsNaN(cell.MeasureWidth.Dp))
					{
						// We still have some unknown measure constraints (* rows/columns that need to have
						// the Auto measurements settled before we can measure them). So mark this cell for the 
						// second pass, to be done once we know the constraints.
						cell.NeedsSecondPass = true;

						continue;
					}

					var measure = MeasureCell(cell, cell.MeasureWidth.Dp, cell.MeasureHeight.Dp);

					if (treatCellWidthAsAuto)
					{
						if (cell.ColumnSpan == 1)
						{
							_columns[cell.Column].Update(measure.Width);
						}
						else
						{
							TrackSpan(new Span(cell.Column, cell.ColumnSpan, true, measure.Width));
						}
					}

					if (treatCellHeightAsAuto)
					{
						if (cell.RowSpan == 1)
						{
							_rows[cell.Row].Update(measure.Height);
						}
						else
						{
							TrackSpan(new Span(cell.Row, cell.RowSpan, false, measure.Height));
						}
					}
				}
			}

			void SecondMeasurePass()
			{
				foreach (var cell in _cells)
				{
					if (!cell.NeedsSecondPass)
					{
						continue;
					}

					double width = 0;
					double height = 0;

					if (double.IsInfinity(cell.MeasureHeight.Dp))
					{
						height = double.PositiveInfinity;
					}
					else
					{
						for (int n = cell.Row; n < cell.Row + cell.RowSpan; n++)
						{
							height += _rows[n].Size.Dp;
						}
					}

					if (double.IsInfinity(cell.MeasureWidth.Dp))
					{
						width = double.PositiveInfinity;
					}
					else
					{
						for (int n = cell.Column; n < cell.Column + cell.ColumnSpan; n++)
						{
							width += _columns[n].Size.Dp;
						}
					}

					if (width == 0 || height == 0)
					{
						continue;
					}

					var measure = MeasureCell(cell, width, height);

					if (cell.IsColumnSpanStar && cell.ColumnSpan > 1)
					{
						TrackSpan(new Span(cell.Column, cell.ColumnSpan, true, measure.Width));
					}
					else if (cell.ColumnSpan == 1)
					{
						if (TreatCellWidthAsAuto(cell))
						{
							_columns[cell.Column].Update(measure.Width);
						}
					}

					if (cell.IsRowSpanStar && cell.RowSpan > 1)
					{
						TrackSpan(new Span(cell.Row, cell.RowSpan, false, measure.Height));
					}
					else if (cell.RowSpan == 1)
					{
						if (TreatCellHeightAsAuto(cell))
						{
							_rows[cell.Row].Update(measure.Height);
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
					currentSize += definitions[n].Size.Dp;

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
					left += _columns[n].Size.Dp;
					left += _columnSpacing;
				}

				return left;
			}

			double TopEdgeOfRow(int row)
			{
				double top = _padding.Top;

				for (int n = 0; n < row; n++)
				{
					top += _rows[n].Size.Dp;
					top += _rowSpacing;
				}

				return top;
			}

			void ResolveStars(Definition[] defs, double availableSpace, Func<Cell, bool> cellCheck, Func<Size, double> dimension, double starCount)
			{
				Debug.Assert(starCount > 0, "The caller of ResolveStars has already checked that there are star values to resolve.");

				if (availableSpace <= 0)
				{
					// This can happen if an Auto-measured part of a span is larger than the Grid's constraint;
					// There's a negative amount of space left for the Star values. Just bail, the
					// Star values are already zero and they can't get any smaller.
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
							// Update the star size if the view in this cell is bigger
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
				if (_columnStarCount == 0)
				{
					return;
				}

				var availableSpace = widthConstraint - GridWidth();
				static bool cellCheck(Cell cell) => cell.IsColumnSpanStar;
				static double getDimension(Size size) => size.Width;

				ResolveStars(_columns, availableSpace, cellCheck, getDimension, _columnStarCount);

				foreach (var cell in _cells)
				{
					if (double.IsNaN(cell.MeasureWidth))
					{
						UpdateKnownMeasureWidth(cell);
					}
				}
			}

			void ResolveStarRows(double heightConstraint)
			{
				if (_rowStarCount == 0)
				{
					return;
				}

				var availableSpace = heightConstraint - GridHeight();
				static bool cellCheck(Cell cell) => cell.IsRowSpanStar;
				static double getDimension(Size size) => size.Height;

				ResolveStars(_rows, availableSpace, cellCheck, getDimension, _rowStarCount);

				foreach (var cell in _cells)
				{
					if (double.IsNaN(cell.MeasureHeight.Dp))
					{
						UpdateKnownMeasureHeight(cell);
					}
				}
			}

			void MinimizeStarsForMeasurement()
			{
				MinimizeStarRows();
				MinimizeStarColumns();
			}

			void MinimizeStarRows()
			{
				// Iterate over the cells; determine if they span any star rows
				// and find the mininum sizes for those star rows to contain the cell contents

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

					var cellRequiredHeight = Math.Min(_gridHeightConstraint, _childrenToLayOut[cell.ViewIndex].DesiredSize.Height);

					DetermineMinimumStarSizesInSpan(cellRequiredHeight, _rows, start, end);
				}
			}

			void MinimizeStarColumns()
			{
				// Iterate over the cells; determine if they span any star columns
				// and find the mininum sizes for those star columns to contain the cell contents

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

					DetermineMinimumStarSizesInSpan(cellRequiredWidth, _columns, start, end);
				}
			}

			static void DetermineMinimumStarSizesInSpan(double spaceNeeded, Definition[] definitions, int start, int end)
			{
				// Remove the parts of spaceNeeded which are already covered by explicit and auto columns in the span
				for (int n = start; n < end; n++)
				{
					if (definitions[n].IsAbsolute || definitions[n].IsAuto)
					{
						spaceNeeded -= definitions[n].Size.Dp;
					}
				}

				// Determine how much space the star sizes in the span are already requesting
				// (because of other overlapping cells, parts of this span may have already been processed)

				double spaceAvailable = 0;
				int starsInSpan = 0;
				for (int n = start; n < end; n++)
				{
					if (definitions[n].IsStar)
					{
						starsInSpan += 1;
						spaceAvailable += definitions[n].MinimumSize;
					}
				}

				// If previous inflations from other cells haven't given us enough room,
				// distribute the amount of space we still need evenly across the stars in the span
				if (spaceAvailable < spaceNeeded)
				{
					var toAdd = (spaceNeeded - spaceAvailable) / starsInSpan;
					for (int n = start; n < end; n++)
					{
						if (definitions[n].IsStar)
						{
							definitions[n].MinimumSize += toAdd;
							if (definitions[n].MinimumSize > definitions[n].Size)
							{
								definitions[n].MinimumSize = definitions[n].Size;
							}
						}
					}
				}
			}

			public void PrepareForArrange(Size targetSize)
			{
				// Minimize all the star values (if any); then we can expand them to the target size if necessary
				MinimizeStars(_rows);
				MinimizeStars(_columns);

				bool expandStarRows = _rowStarCount > 0
					&& (Dimension.IsExplicitSet(_explicitGridHeight)
					|| _grid.VerticalLayoutAlignment == LayoutAlignment.Fill);

				bool expandStarColumns = _columnStarCount > 0
					&& (Dimension.IsExplicitSet(_explicitGridWidth)
					|| _grid.HorizontalLayoutAlignment == LayoutAlignment.Fill);

				if (expandStarRows)
				{
					// If the grid is constrained vertically, we will need to limit the upper size of * rows;
					// if not, they can be whatever size makes sense for the content
					var limitStarRowHeights = !double.IsInfinity(_gridHeightConstraint);
					ExpandStarDefinitions(_rows, targetSize.Height - _padding.VerticalThickness, GridMinimumHeight() - _padding.VerticalThickness,
						_rowSpacing, _rowStarCount, limitStarRowHeights);
				}

				if (expandStarColumns)
				{
					// If the grid is constrained horizontally, we will need to limit the upper size of * columns;
					// if not, they can be whatever size makes sense for the content
					var limitStarRowWidths = !double.IsInfinity(_gridWidthConstraint);
					ExpandStarDefinitions(_columns, targetSize.Width - _padding.HorizontalThickness, GridMinimumWidth() - _padding.HorizontalThickness,
						_columnSpacing, _columnStarCount, limitStarRowWidths);
				}
			}

			static void MinimizeStars(Definition[] defs)
			{
				foreach (var def in defs)
				{
					if (def.IsStar)
					{
						def.Size = def.MinimumSize;
					}
				}
			}

			void ExpandStarDefinitions(Definition[] definitions, double targetSize, double currentSize, double spacing, double starCount, bool limitStarSizes)
			{
				// Figure out what the star value should be at this size
				var starSize = ComputeStarSizeForTarget(targetSize, definitions, spacing, starCount);

				if (limitStarSizes)
				{
					// Before we expand the star values, we need to ensure that the size and minimum size of
					// each star row/column do not exceed the value for starSize at the arranged size 
					// (which may be smaller than the measured size)
					EnsureSizeLimit(definitions, starSize);
				}

				// Get density for pixel-perfect distribution
				var density = GetDensity();

				// Inflate the stars so that we fill up the space at this size
				ExpandStars(targetSize, currentSize, definitions, starSize, starCount, density);
			}

			static void EnsureSizeLimit(Definition[] definitions, double starSize)
			{
				for (int n = 0; n < definitions.Length; n++)
				{
					var def = definitions[n];
					if (!def.IsStar)
					{
						continue;
					}

					var maxSize = starSize * def.GridLength.Value;
					def.Size = Math.Min(maxSize, def.Size);
					def.MinimumSize = Math.Min(maxSize, def.MinimumSize);
				}
			}

			static double ComputeStarSizeForTarget(double targetSize, Definition[] defs, double spacing, double starCount)
			{
				var sum = SumDefinitions(defs, spacing, true);

				// Remove all the star defintions from the current size
				foreach (var def in defs)
				{
					if (def.IsStar)
					{
						sum -= def.MinimumSize;
					}
				}

				return (targetSize - sum) / starCount;
			}

			static void ExpandStars(double targetSize, double currentSize, Definition[] defs, double targetStarSize, double starCount, double density)
			{
				Debug.Assert(starCount > 0, "Assume that the caller has already checked for the existence of star rows/columns before using this.");

				var availableSpace = targetSize - currentSize;

				if (availableSpace <= 0)
				{
					return;
				}

				// Figure out which is the biggest star definition in this dimension (absolute value and star scale)
				var maxCurrentSize = 0.0;
				var maxCurrentStarSize = 0.0;
				foreach (var definition in defs)
				{
					if (definition.IsStar)
					{
						double definitionSize = definition.MinimumSize;
						maxCurrentSize = Math.Max(maxCurrentSize, definitionSize);
						maxCurrentStarSize = Math.Max(maxCurrentStarSize, definitionSize / definition.GridLength.Value);
					}
				}

				// The targetStarSize is the size that star values would have to have in order for all
				// the star rows/columns to fit in the targetSize. 

				if (maxCurrentStarSize <= targetStarSize)
				{
					// If the biggest current star size we have in the definitions is less than the
					// targetStarSize, that means we have enough room to expand all of our star rows/columns
					// to their full size.

					var starDefinitions = defs.Where(d => d.IsStar).ToArray();
					var portions = starDefinitions.Select(d => targetStarSize * d.GridLength.Value).ToArray();
					var totalPixels = portions.Sum() * density;
					var pixelAllocations = DensityValue.DistributePixels(totalPixels, density, portions);

					for (int i = 0; i < starDefinitions.Length; i++)
					{
						starDefinitions[i].Size = DensityValue.FromPixels(pixelAllocations[i], density);
					}

					return;
				}

				// We don't have enough room for all the star rows/columns to expand to their full size.
				// But we still need to fill up the rest of the space, so we'll expand them proportionally.

				// Work out the total difference in size between the full star size targets and the 
				// minimum sizes for all definitions where the minimum is less than the full star size
				// target; we'll need that later to distribute available space.
				double totaldiff = 0;
				foreach (var definition in defs)
				{
					if (definition.IsStar)
					{
						double fullTargetSize = targetStarSize * definition.GridLength.Value;
						if (definition.MinimumSize < fullTargetSize)
							totaldiff += fullTargetSize - definition.MinimumSize;
					}
				}

				// Use density-aware distribution for pixel-perfect proportional allocation
				var proportionalStarDefinitions = defs.Where(d => d.IsStar).ToArray();
				var proportionalPortions = new double[proportionalStarDefinitions.Length];

				for (int i = 0; i < proportionalStarDefinitions.Length; i++)
				{
					var definition = proportionalStarDefinitions[i];
					double fullTargetSize = targetStarSize * definition.GridLength.Value;

					if (definition.MinimumSize < fullTargetSize)
					{
						var scale = (fullTargetSize - definition.MinimumSize) / totaldiff;
						var portion = scale * availableSpace;
						proportionalPortions[i] = definition.MinimumSize + portion;
					}
					else
					{
						proportionalPortions[i] = definition.MinimumSize;
					}
				}

				var proportionalTotalPixels = proportionalPortions.Sum() * density;
				var proportionalPixelAllocations = DensityValue.DistributePixels(proportionalTotalPixels, density, proportionalPortions);

				for (int i = 0; i < proportionalStarDefinitions.Length; i++)
				{
					proportionalStarDefinitions[i].Size = DensityValue.FromPixels(proportionalPixelAllocations[i], density);
				}
			}


			/// <summary>
			/// Gets the display density for density-aware calculations.
			/// </summary>
			/// <returns>The display density, or 1.0 if not available.</returns>
			double GetDensity()
			{
				// Try to get density from the grid view if it implements IViewWithWindow
				if (_grid is IViewWithWindow viewWithWindow && viewWithWindow.Window != null)
				{
					return viewWithWindow.Window.RequestDisplayDensity();
				}

				return 1.0;
			}

			static bool AnyAuto(Definition[] definitions)
			{
				foreach (var definition in definitions)
				{
					if (definition.IsAuto)
					{
						return true;
					}
				}

				return false;
			}

			bool IsStarWidthPrecomputable()
			{
				if (double.IsInfinity(_gridWidthConstraint))
				{
					return false;
				}

				return !AnyAuto(_columns);
			}

			bool IsStarHeightPrecomputable()
			{
				if (double.IsInfinity(_gridHeightConstraint))
				{
					return false;
				}

				return !AnyAuto(_rows);
			}

			void UpdateKnownMeasureWidth(Cell cell)
			{
				double measureWidth = 0;
				for (int column = cell.Column; column < cell.Column + cell.ColumnSpan; column++)
				{
					measureWidth += _columns[column].Size.Dp;

					if (column > cell.Column)
					{
						measureWidth += _columnSpacing;
					}
				}

				cell.MeasureWidth = measureWidth;
			}

			void UpdateKnownMeasureHeight(Cell cell)
			{
				double measureHeight = 0;
				for (int row = cell.Row; row < cell.Row + cell.RowSpan; row++)
				{
					measureHeight += _rows[row].Size.Dp;

					if (row > cell.Row)
					{
						measureHeight += _rowSpacing;
					}
				}

				cell.MeasureHeight = measureHeight;
			}

			void DetermineCellMeasureWidth(Cell cell)
			{
				if (cell.ColumnGridLengthType == GridLengthType.Absolute)
				{
					UpdateKnownMeasureWidth(cell);
				}
				else if (TreatCellWidthAsAuto(cell))
				{
					cell.MeasureWidth = double.PositiveInfinity;
				}

				// For all other situations, we'll have to wait until we've measured the Auto columns
			}

			void DetermineCellMeasureHeight(Cell cell)
			{
				if (cell.RowGridLengthType == GridLengthType.Absolute)
				{
					UpdateKnownMeasureHeight(cell);
				}
				else if (TreatCellHeightAsAuto(cell))
				{
					cell.MeasureHeight = double.PositiveInfinity;
				}

				// For all other situations, we'll have to wait until we've measured the Auto rows
			}

			bool TreatCellWidthAsAuto(Cell cell)
			{
				if (cell.IsColumnSpanStar)
				{
					// Because the Grid isn't horizontally constrained, we treat * columns as Auto 
					return double.IsInfinity(_gridWidthConstraint);
				}

				return cell.IsColumnSpanAuto;
			}

			bool TreatCellHeightAsAuto(Cell cell)
			{
				if (cell.IsRowSpanStar)
				{
					// Because the Grid isn't vertically constrained, we treat * rows  as Auto 
					return double.IsInfinity(_gridHeightConstraint);
				}

				return cell.IsRowSpanAuto;
			}

			static double CountStars(Definition[] definitions)
			{
				// Count up the total weight of star values (e.g., "*, 3*, *" == 5)
				var starCount = 0.0;

				foreach (var definition in definitions)
				{
					if (definition.IsStar)
					{
						starCount += definition.GridLength.Value;
					}
				}

				return starCount;
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
			public DensityValue MeasureWidth { get; set; } = new DensityValue(double.NaN);
			public DensityValue MeasureHeight { get; set; } = new DensityValue(double.NaN);
			public bool NeedsSecondPass { get; set; }

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
			private DensityValue _size;

			/// <summary>
			/// The current size of this definition
			/// </summary>
			public DensityValue Size
			{
				get => _size;
				set
				{
					_size = value;
					if (!IsStar)
					{
						MinimumSize = value;
					}
				}
			}

			/// <summary>
			/// The minimum size of this definition
			/// For absolute and auto definitions, this is the same as Size
			/// For star definitions, this is the minimum size which can contain the contents of the row/column
			/// </summary>
			public DensityValue MinimumSize { get; set; }

			public void Update(DensityValue size)
			{
				if (size.RawPx > Size.RawPx)
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
				_gridLength = gridLength;

				if (gridLength.IsAbsolute)
				{
					Size = gridLength.Value;
				}
				else
				{
					// For auto and star, start with size 0
					Size = new DensityValue(0.0);
					MinimumSize = new DensityValue(0.0);
				}
			}
		}
	}
}