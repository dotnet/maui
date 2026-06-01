using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	/// <summary>
	/// Custom UniformGridLayout that handles group headers and footers spanning full width/height.
	/// This mimics the behavior of CV1's GridView with GroupStyle support.
	/// </summary>
	internal partial class GroupableUniformGridLayout : UniformGridLayout
	{
		const double DefaultEstimatedRegularExtent = 48;
		const double DefaultEstimatedHeaderExtent = 36;
		const int RealizationBufferRowsOrColumns = 2;

		double _estimatedVerticalRegularExtent = DefaultEstimatedRegularExtent;
		double _estimatedHorizontalRegularExtent = DefaultEstimatedRegularExtent;
		double _estimatedHeaderVerticalExtent = DefaultEstimatedHeaderExtent;
		double _estimatedHeaderHorizontalExtent = DefaultEstimatedHeaderExtent;

		protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
		{
			if (context.ItemCount == 0)
			{
				return new Size(0, 0);
			}

			var span = Math.Max(1, MaximumRowsOrColumns);
			var minColumnSpacing = MinColumnSpacing;
			var minRowSpacing = MinRowSpacing;
			var isVerticalOrientation = Orientation == Orientation.Horizontal;
			var realizationRange = GetRealizationIndexRange(context, span, isVerticalOrientation);

			double totalHeight = 0;
			double totalWidth = 0;
			double maxItemSize = 0;
			int columnIndex = 0;
			int rowIndex = 0;

			// Iterate all indices for layout math, but realize only the visible realization window.
			for (int i = 0; i < context.ItemCount; i++)
			{
				bool shouldRealize = i >= realizationRange.start && i <= realizationRange.end;
				bool isHeaderOrFooter = IsHeaderOrFooterContext(context, i);
				double elementPrimarySize;

				if (shouldRealize)
				{
					var element = context.GetOrCreateElementAt(i);
					element.Measure(availableSize);
					isHeaderOrFooter = IsHeaderOrFooter(element);
					elementPrimarySize = isVerticalOrientation
						? element.DesiredSize.Height
						: element.DesiredSize.Width;

					if (isHeaderOrFooter)
					{
						if (isVerticalOrientation)
							_estimatedHeaderVerticalExtent = elementPrimarySize;
						else
							_estimatedHeaderHorizontalExtent = elementPrimarySize;
					}
					else
					{
						if (isVerticalOrientation)
							_estimatedVerticalRegularExtent = elementPrimarySize;
						else
							_estimatedHorizontalRegularExtent = elementPrimarySize;
					}
				}
				else
				{
					elementPrimarySize = GetEstimatedPrimarySize(isVerticalOrientation, isHeaderOrFooter);
				}

				if (isHeaderOrFooter)
				{
					if (isVerticalOrientation)
					{
						// Complete current row if needed
						if (columnIndex > 0)
						{
							totalHeight += maxItemSize + minRowSpacing;
							columnIndex = 0;
							maxItemSize = 0;
						}
						// Add header height
						totalHeight += elementPrimarySize + minRowSpacing;
					}
					else
					{
						// Complete current column if needed
						if (rowIndex > 0)
						{
							totalWidth += maxItemSize + minColumnSpacing;
							rowIndex = 0;
							maxItemSize = 0;
						}
						// Add header width
						totalWidth += elementPrimarySize + minColumnSpacing;
					}
				}
				else
				{
					if (isVerticalOrientation)
					{
						maxItemSize = Math.Max(maxItemSize, elementPrimarySize);
						columnIndex++;

						if (columnIndex >= span)
						{
							totalHeight += maxItemSize + minRowSpacing;
							columnIndex = 0;
							maxItemSize = 0;
						}
					}
					else
					{
						maxItemSize = Math.Max(maxItemSize, elementPrimarySize);
						rowIndex++;

						if (rowIndex >= span)
						{
							totalWidth += maxItemSize + minColumnSpacing;
							rowIndex = 0;
							maxItemSize = 0;
						}
					}
				}
			}

			// Add final incomplete row/column
			if (isVerticalOrientation)
			{
				if (columnIndex > 0)
				{
					totalHeight += maxItemSize;
				}

				// Calculate width based on span
				totalWidth = availableSize.Width;
			}
			else
			{
				if (rowIndex > 0)
				{
					totalWidth += maxItemSize;
				}

				// Calculate height based on span
				totalHeight = availableSize.Height;
			}

			return new Size(totalWidth, totalHeight);
		}

		protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
		{
			if (context.ItemCount == 0)
			{
				return finalSize;
			}

			var span = Math.Max(1, MaximumRowsOrColumns);
			var minColumnSpacing = MinColumnSpacing;
			var minRowSpacing = MinRowSpacing;
			var isVerticalOrientation = Orientation == Orientation.Horizontal; // Horizontal orientation means vertical scrolling (items flow left-to-right, stacking vertically)
			var realizationRange = GetRealizationIndexRange(context, span, isVerticalOrientation);

			double currentX = 0;
			double currentY = 0;
			double cellWidth = 0;
			double cellHeight = 0;
			int columnIndex = 0;
			int rowIndex = 0;

			// Calculate cell dimensions based on span and available space
			if (isVerticalOrientation)
			{
				// Vertical grid: divide width by span for column width
				double totalSpacing = minColumnSpacing * (span - 1);
				cellWidth = Math.Max(0, (finalSize.Width - totalSpacing) / span);
				cellHeight = _estimatedVerticalRegularExtent;
			}
			else
			{
				// Horizontal grid: divide height by span for row height
				double totalSpacing = minRowSpacing * (span - 1);
				cellHeight = Math.Max(0, (finalSize.Height - totalSpacing) / span);
				cellWidth = _estimatedHorizontalRegularExtent;
			}

			// Iterate all indices for coordinate calculation, but only realize/arrange within the realization window.
			for (int i = 0; i < context.ItemCount; i++)
			{
				bool shouldRealize = i >= realizationRange.start && i <= realizationRange.end;
				bool isHeaderOrFooter = IsHeaderOrFooterContext(context, i);
				UIElement? child = null;

				if (shouldRealize)
				{
					child = context.GetOrCreateElementAt(i);
					isHeaderOrFooter = IsHeaderOrFooter(child);
				}

				if (isHeaderOrFooter)
				{
					// Header/Footer: Span full width (vertical grid) or full height (horizontal grid)
					if (isVerticalOrientation)
					{
						// If we're in the middle of a row, complete the row first
						if (columnIndex > 0)
						{
							currentY += cellHeight + minRowSpacing;
							currentX = 0;
							columnIndex = 0;
						}

						// Vertical grid: header spans full width
						var headerHeight = shouldRealize && child is not null
							? child.DesiredSize.Height
							: _estimatedHeaderVerticalExtent;
						if (shouldRealize && child is not null)
						{
							var headerRect = new Rect(0, currentY, finalSize.Width, headerHeight);
							child.Arrange(headerRect);
							_estimatedHeaderVerticalExtent = headerHeight;
						}

						// Move to next row
						currentY += headerHeight + minRowSpacing;
					}
					else
					{
						// If we're in the middle of a column, complete the column first
						if (rowIndex > 0)
						{
							currentX += cellWidth + minColumnSpacing;
							currentY = 0;
							rowIndex = 0;
						}

						// Horizontal grid: header spans full height
						var headerWidth = shouldRealize && child is not null
							? child.DesiredSize.Width
							: _estimatedHeaderHorizontalExtent;
						if (shouldRealize && child is not null)
						{
							var headerRect = new Rect(currentX, 0, headerWidth, finalSize.Height);
							child.Arrange(headerRect);
							_estimatedHeaderHorizontalExtent = headerWidth;
						}

						// Move to next column
						currentX += headerWidth + minColumnSpacing;
					}
				}
				else
				{
					// Regular item: Place in grid cell
					if (isVerticalOrientation)
					{
						// Vertical grid: items flow left-to-right
						if (shouldRealize && child is not null)
						{
							var itemRect = new Rect(currentX, currentY, cellWidth, cellHeight);
							child.Arrange(itemRect);
							_estimatedVerticalRegularExtent = Math.Max(_estimatedVerticalRegularExtent, child.DesiredSize.Height);
						}

						columnIndex++;
						currentX += cellWidth + minColumnSpacing;

						if (columnIndex >= span)
						{
							// Move to next row
							columnIndex = 0;
							currentX = 0;
							currentY += cellHeight + minRowSpacing;
						}
					}
					else
					{
						// Horizontal grid: items flow top-to-bottom
						if (shouldRealize && child is not null)
						{
							var itemRect = new Rect(currentX, currentY, cellWidth, cellHeight);
							child.Arrange(itemRect);
							_estimatedHorizontalRegularExtent = Math.Max(_estimatedHorizontalRegularExtent, child.DesiredSize.Width);
						}

						rowIndex++;
						currentY += cellHeight + minRowSpacing;

						if (rowIndex >= span)
						{
							// Move to next column
							rowIndex = 0;
							currentY = 0;
							currentX += cellWidth + minColumnSpacing;
						}
					}
				}
			}

			// Calculate total size
			if (isVerticalOrientation)
			{
				// Add final row height if we have items in the current row
				if (columnIndex > 0)
				{
					currentY += cellHeight;
				}
				// Return the actual height needed (don't clamp to finalSize.Height)
				return new Size(finalSize.Width, currentY);
			}
			else
			{
				// Add final column width if we have items in the current column
				if (rowIndex > 0)
				{
					currentX += cellWidth;
				}
				// Return the actual width needed (don't clamp to finalSize.Width)
				return new Size(currentX, finalSize.Height);
			}
		}

		(double start, double end) GetRealizationPrimaryRange(VirtualizingLayoutContext context, bool isVerticalOrientation)
		{
			if (isVerticalOrientation)
			{
				return (context.RealizationRect.Top, context.RealizationRect.Bottom);
			}

			return (context.RealizationRect.Left, context.RealizationRect.Right);
		}

		(int start, int end) GetRealizationIndexRange(VirtualizingLayoutContext context, int span, bool isVerticalOrientation)
		{
			if (context.ItemCount <= 0)
			{
				return (0, -1);
			}

			var (realizationStart, realizationEnd) = GetRealizationPrimaryRange(context, isVerticalOrientation);
			double estimatedRegular = GetEstimatedPrimarySize(isVerticalOrientation, isHeaderOrFooter: false);
			double spacing = isVerticalOrientation ? MinRowSpacing : MinColumnSpacing;

			double step = Math.Max(1, estimatedRegular + spacing);
			int startBand = (int)Math.Floor(realizationStart / step) - RealizationBufferRowsOrColumns;
			int endBand = (int)Math.Ceiling(realizationEnd / step) + RealizationBufferRowsOrColumns;

			startBand = Math.Max(0, startBand);
			endBand = Math.Max(startBand, endBand);

			int startIndex = Math.Max(0, (startBand * span) - span);
			int endIndex = Math.Min(context.ItemCount - 1, ((endBand + 1) * span) - 1 + span);

			return (startIndex, endIndex);
		}

		double GetEstimatedPrimarySize(bool isVerticalOrientation, bool isHeaderOrFooter)
		{
			if (isHeaderOrFooter)
			{
				return isVerticalOrientation ? _estimatedHeaderVerticalExtent : _estimatedHeaderHorizontalExtent;
			}

			return isVerticalOrientation ? _estimatedVerticalRegularExtent : _estimatedHorizontalRegularExtent;
		}

		bool IsHeaderOrFooterContext(VirtualizingLayoutContext context, int index)
		{
			var item = context.GetItemAt(index);
			if (item is ItemTemplateContext2 itemTemplateContext)
			{
				return itemTemplateContext.IsHeader || itemTemplateContext.IsFooter;
			}

			return false;
		}

		/// <summary>
		/// Determines if a child element is a header or footer via ElementWrapper metadata.
		/// </summary>
		bool IsHeaderOrFooter(UIElement child)
		{
			return child is ItemContainer itemContainer
				&& itemContainer.Child is ElementWrapper wrapper
				&& wrapper.IsHeaderOrFooter;
		}
	}
}
