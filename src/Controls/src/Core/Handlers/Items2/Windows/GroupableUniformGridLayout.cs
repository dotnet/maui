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
#pragma warning disable CsWinRT1028 // Class is not marked partial
	internal class GroupableUniformGridLayout : UniformGridLayout
#pragma warning restore CsWinRT1028 // Class is not marked partial
	{
		protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
		{
			if (context.ItemCount == 0)
			{
				return new Size(0, 0);
			}

			var span = MaximumRowsOrColumns;
			var minColumnSpacing = MinColumnSpacing;
			var minRowSpacing = MinRowSpacing;
			var isVerticalOrientation = Orientation == Orientation.Horizontal;

			double totalHeight = 0;
			double totalWidth = 0;
			double maxItemSize = 0;
			int columnIndex = 0;
			int rowIndex = 0;
			int regularItemCount = 0;

			// Measure all children and calculate dimensions
			for (int i = 0; i < context.ItemCount; i++)
			{
				var element = context.GetOrCreateElementAt(i);
				element.Measure(availableSize);
				
				bool isHeaderOrFooter = IsHeaderOrFooter(element);

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
						totalHeight += element.DesiredSize.Height + minRowSpacing;
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
						totalWidth += element.DesiredSize.Width + minColumnSpacing;
					}
				}
				else
				{
					regularItemCount++;
					
					if (isVerticalOrientation)
					{
						maxItemSize = Math.Max(maxItemSize, element.DesiredSize.Height);
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
						maxItemSize = Math.Max(maxItemSize, element.DesiredSize.Width);
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
				double cellWidth = (availableSize.Width - minColumnSpacing * (span - 1)) / span;
				totalWidth = availableSize.Width;
			}
			else
			{
				if (rowIndex > 0)
				{
					totalWidth += maxItemSize;
				}
				
				// Calculate height based on span
				double cellHeight = (availableSize.Height - minRowSpacing * (span - 1)) / span;
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

			var span = MaximumRowsOrColumns;
			var minColumnSpacing = MinColumnSpacing;
			var minRowSpacing = MinRowSpacing;
			var isVerticalOrientation = Orientation == Orientation.Horizontal; // Horizontal orientation means vertical scrolling (items flow left-to-right, stacking vertically)

			double currentX = 0;
			double currentY = 0;
			double cellWidth = 0;
			double cellHeight = 0;
			double maxItemSize = 0; // Max width (vertical) or height (horizontal) from regular items
			int columnIndex = 0;
			int rowIndex = 0;

			// First pass: Calculate cell dimensions from regular (non-header/footer) items
			for (int i = 0; i < context.ItemCount; i++)
			{
				var child = context.GetOrCreateElementAt(i);
				bool isHeaderOrFooter = IsHeaderOrFooter(child);

				if (!isHeaderOrFooter)
				{
					if (isVerticalOrientation)
					{
						// Vertical grid: items flow left-to-right, wrapping to new rows
						// Track max height for uniform row heights
						maxItemSize = Math.Max(maxItemSize, child.DesiredSize.Height);
					}
					else
					{
						// Horizontal grid: items flow top-to-bottom, wrapping to new columns
						// Track max width for uniform column widths
						maxItemSize = Math.Max(maxItemSize, child.DesiredSize.Width);
					}
				}
			}

			// Calculate cell dimensions based on span and available space
			if (isVerticalOrientation)
			{
				// Vertical grid: divide width by span for column width
				double totalSpacing = minColumnSpacing * (span - 1);
				cellWidth = (finalSize.Width - totalSpacing) / span;
				cellHeight = maxItemSize > 0 ? maxItemSize : 0;
			}
			else
			{
				// Horizontal grid: divide height by span for row height
				double totalSpacing = minRowSpacing * (span - 1);
				cellHeight = (finalSize.Height - totalSpacing) / span;
				cellWidth = maxItemSize > 0 ? maxItemSize : 0;
			}

			// Second pass: Arrange items
			for (int i = 0; i < context.ItemCount; i++)
			{
				var child = context.GetOrCreateElementAt(i);
				bool isHeaderOrFooter = IsHeaderOrFooter(child);

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
						var headerHeight = child.DesiredSize.Height;
						var headerRect = new Rect(0, currentY, finalSize.Width, headerHeight);
						child.Arrange(headerRect);
						
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
						var headerWidth = child.DesiredSize.Width;
						var headerRect = new Rect(currentX, 0, headerWidth, finalSize.Height);
						child.Arrange(headerRect);
						
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
						var itemRect = new Rect(currentX, currentY, cellWidth, cellHeight);
						child.Arrange(itemRect);

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
						var itemRect = new Rect(currentX, currentY, cellWidth, cellHeight);
						child.Arrange(itemRect);

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

		/// <summary>
		/// Determines if a child element is a header or footer by checking for ItemTemplateContext2.
		/// </summary>
		bool IsHeaderOrFooter(UIElement child)
		{
			// Check if the child is an ItemContainer and its content's DataContext is ItemTemplateContext2
			if (child is ItemContainer itemContainer && 
				itemContainer.Child is FrameworkElement fe && 
				fe.DataContext is ItemTemplateContext2 context)
			{
				return context.IsHeader || context.IsFooter;
			}

			return false;
		}
	}
}
