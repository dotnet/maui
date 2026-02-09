using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// Extends UniformGridLayout to handle group headers/footers spanning full width.
/// Regular grid items use UniformGridLayout's native behavior.
/// Only headers/footers are overridden to span the full available width.
/// </summary>
#pragma warning disable CsWinRT1028 // Class is not marked partial
internal class MauiGridLayout : UniformGridLayout
#pragma warning restore CsWinRT1028
{
	protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
	{
		if (context.ItemCount == 0)
			return new Size(0, 0);

		var span = MaximumRowsOrColumns;
		var minColumnSpacing = MinColumnSpacing;
		var minRowSpacing = MinRowSpacing;
		var isVertical = Orientation == Orientation.Horizontal; // Horizontal orientation = vertical scrolling

		double totalHeight = 0;
		double totalWidth = 0;
		double maxItemSize = 0;
		int columnIndex = 0;
		int rowIndex = 0;

		for (int i = 0; i < context.ItemCount; i++)
		{
			var element = context.GetOrCreateElementAt(i);
			element.Measure(availableSize);

			bool isFullSpan = IsHeaderOrFooter(element);

			if (isFullSpan)
			{
				if (isVertical)
				{
					if (columnIndex > 0)
					{
						totalHeight += maxItemSize + minRowSpacing;
						columnIndex = 0;
						maxItemSize = 0;
					}
					totalHeight += element.DesiredSize.Height + minRowSpacing;
				}
				else
				{
					if (rowIndex > 0)
					{
						totalWidth += maxItemSize + minColumnSpacing;
						rowIndex = 0;
						maxItemSize = 0;
					}
					totalWidth += element.DesiredSize.Width + minColumnSpacing;
				}
			}
			else
			{
				if (isVertical)
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
		if (isVertical)
		{
			if (columnIndex > 0)
				totalHeight += maxItemSize;

			totalWidth = availableSize.Width;
		}
		else
		{
			if (rowIndex > 0)
				totalWidth += maxItemSize;

			totalHeight = availableSize.Height;
		}

		return new Size(totalWidth, totalHeight);
	}

	protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
	{
		if (context.ItemCount == 0)
			return finalSize;

		var span = MaximumRowsOrColumns;
		var minColumnSpacing = MinColumnSpacing;
		var minRowSpacing = MinRowSpacing;
		var isVertical = Orientation == Orientation.Horizontal;

		double currentX = 0;
		double currentY = 0;
		double cellWidth = 0;
		double cellHeight = 0;
		double maxItemSize = 0;
		int columnIndex = 0;
		int rowIndex = 0;

		// First pass: calculate uniform cell size from regular items
		for (int i = 0; i < context.ItemCount; i++)
		{
			var child = context.GetOrCreateElementAt(i);
			if (!IsHeaderOrFooter(child))
			{
				if (isVertical)
					maxItemSize = Math.Max(maxItemSize, child.DesiredSize.Height);
				else
					maxItemSize = Math.Max(maxItemSize, child.DesiredSize.Width);
			}
		}

		if (isVertical)
		{
			double totalSpacing = minColumnSpacing * (span - 1);
			cellWidth = (finalSize.Width - totalSpacing) / span;
			cellHeight = maxItemSize > 0 ? maxItemSize : 0;
		}
		else
		{
			double totalSpacing = minRowSpacing * (span - 1);
			cellHeight = (finalSize.Height - totalSpacing) / span;
			cellWidth = maxItemSize > 0 ? maxItemSize : 0;
		}

		// Second pass: arrange items
		for (int i = 0; i < context.ItemCount; i++)
		{
			var child = context.GetOrCreateElementAt(i);
			bool isFullSpan = IsHeaderOrFooter(child);

			if (isFullSpan)
			{
				if (isVertical)
				{
					if (columnIndex > 0)
					{
						currentY += cellHeight + minRowSpacing;
						currentX = 0;
						columnIndex = 0;
					}

					var headerHeight = child.DesiredSize.Height;
					child.Arrange(new Rect(0, currentY, finalSize.Width, headerHeight));
					currentY += headerHeight + minRowSpacing;
				}
				else
				{
					if (rowIndex > 0)
					{
						currentX += cellWidth + minColumnSpacing;
						currentY = 0;
						rowIndex = 0;
					}

					var headerWidth = child.DesiredSize.Width;
					child.Arrange(new Rect(currentX, 0, headerWidth, finalSize.Height));
					currentX += headerWidth + minColumnSpacing;
				}
			}
			else
			{
				if (isVertical)
				{
					child.Arrange(new Rect(currentX, currentY, cellWidth, cellHeight));
					columnIndex++;
					currentX += cellWidth + minColumnSpacing;

					if (columnIndex >= span)
					{
						columnIndex = 0;
						currentX = 0;
						currentY += cellHeight + minRowSpacing;
					}
				}
				else
				{
					child.Arrange(new Rect(currentX, currentY, cellWidth, cellHeight));
					rowIndex++;
					currentY += cellHeight + minRowSpacing;

					if (rowIndex >= span)
					{
						rowIndex = 0;
						currentY = 0;
						currentX += cellWidth + minColumnSpacing;
					}
				}
			}
		}

		if (isVertical)
		{
			if (columnIndex > 0)
				currentY += cellHeight;
			return new Size(finalSize.Width, currentY);
		}
		else
		{
			if (rowIndex > 0)
				currentX += cellWidth;
			return new Size(currentX, finalSize.Height);
		}
	}

	static bool IsHeaderOrFooter(UIElement child)
	{
		if (child is ItemContainer itemContainer &&
			itemContainer.Child is FrameworkElement fe &&
			fe.DataContext is ItemTemplateContext2 context)
		{
			return context.IsHeader || context.IsFooter;
		}

		return false;
	}
}
