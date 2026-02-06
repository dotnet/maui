using System;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Microsoft.Maui.Controls.Handlers.Items2;

/// <summary>
/// A custom VirtualizingLayout for grid layout that supports full-span items for group headers and footers.
/// Similar to UniformGridLayout but with the ability to make specific items span the full width.
/// </summary>
internal class MauiGridLayout : VirtualizingLayout
{
	int _span = 1;
	double _minItemWidth = 0;
	double _minItemHeight = 0;
	double _minColumnSpacing = 0;
	double _minRowSpacing = 0;
	Orientation _orientation = Orientation.Horizontal;

	public int Span
	{
		get => _span;
		set
		{
			if (_span != value)
			{
				_span = Math.Max(1, value);
				InvalidateMeasure();
			}
		}
	}

	public double MinItemWidth
	{
		get => _minItemWidth;
		set
		{
			if (_minItemWidth != value)
			{
				_minItemWidth = value;
				InvalidateMeasure();
			}
		}
	}

	public double MinItemHeight
	{
		get => _minItemHeight;
		set
		{
			if (_minItemHeight != value)
			{
				_minItemHeight = value;
				InvalidateMeasure();
			}
		}
	}

	public double HorizontalSpacing
	{
		get => _minColumnSpacing;
		set
		{
			if (_minColumnSpacing != value)
			{
				_minColumnSpacing = value;
				InvalidateMeasure();
			}
		}
	}

	public double VerticalSpacing
	{
		get => _minRowSpacing;
		set
		{
			if (_minRowSpacing != value)
			{
				_minRowSpacing = value;
				InvalidateMeasure();
			}
		}
	}

	public Orientation Orientation
	{
		get => _orientation;
		set
		{
			if (_orientation != value)
			{
				_orientation = value;
				InvalidateMeasure();
			}
		}
	}

	protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
	{
		InvalidateMeasure();
		base.OnItemsChangedCore(context, source, args);
	}

	protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
	{
		int itemCount = context.ItemCount;
		if (itemCount == 0)
		{
			return new Size(0, 0);
		}

		// Determine scroll direction based on orientation
		// Orientation.Horizontal means items flow left-to-right, scrolling is vertical
		// Orientation.Vertical means items flow top-to-bottom, scrolling is horizontal  
		bool isVerticalScroll = _orientation == Orientation.Horizontal;

		double availableMainAxis = isVerticalScroll ? availableSize.Width : availableSize.Height;
		if (double.IsInfinity(availableMainAxis))
		{
			availableMainAxis = 400;
		}

		double columnSpacing = _minColumnSpacing;
		double rowSpacing = _minRowSpacing;

		// Calculate item size based on span
		double itemWidth = (availableMainAxis - (columnSpacing * (_span - 1))) / _span;
		double itemHeight = 0;

		// Get realization rect for virtualization
		Rect realizationRect = context.RealizationRect;

		// Calculate layout positions and realize visible items
		double currentMainOffset = 0; // Y for vertical scroll, X for horizontal
		int currentColumn = 0;
		double currentRowHeight = 0;

		double totalExtent = 0;

		for (int i = 0; i < itemCount; i++)
		{
			bool isFullSpan = IsFullSpanItem(context, i);

			if (isFullSpan)
			{
				// Complete current row if we have items
				if (currentColumn > 0)
				{
					currentMainOffset += currentRowHeight + rowSpacing;
					currentColumn = 0;
					currentRowHeight = 0;
				}

				// Check if this item is in the visible range
				bool isVisible = IsInRealizationRect(realizationRect, 0, currentMainOffset, availableMainAxis, 50, isVerticalScroll);

				if (isVisible)
				{
					var element = context.GetOrCreateElementAt(i, ElementRealizationOptions.ForceCreate);
					element.Measure(new Size(availableMainAxis, double.PositiveInfinity));
					currentRowHeight = element.DesiredSize.Height;
				}
				else
				{
					currentRowHeight = 50; // Estimated height for non-visible items
					RecycleElementIfExists(context, i);
				}

				currentMainOffset += currentRowHeight + rowSpacing;
				currentRowHeight = 0;
			}
			else
			{
				// Regular grid item
				double x = currentColumn * (itemWidth + columnSpacing);
				double y = currentMainOffset;

				// Check if this item is in the visible range
				bool isVisible = IsInRealizationRect(realizationRect, x, y, itemWidth, 50, isVerticalScroll);

				if (isVisible)
				{
					var element = context.GetOrCreateElementAt(i, ElementRealizationOptions.ForceCreate);
					element.Measure(new Size(itemWidth, double.PositiveInfinity));
					currentRowHeight = Math.Max(currentRowHeight, element.DesiredSize.Height);
					itemHeight = Math.Max(itemHeight, element.DesiredSize.Height);
				}
				else
				{
					RecycleElementIfExists(context, i);
				}

				currentColumn++;
				if (currentColumn >= _span)
				{
					currentMainOffset += (currentRowHeight > 0 ? currentRowHeight : itemHeight > 0 ? itemHeight : 50) + rowSpacing;
					currentColumn = 0;
					currentRowHeight = 0;
				}
			}
		}

		// Account for last incomplete row
		if (currentColumn > 0)
		{
			currentMainOffset += currentRowHeight > 0 ? currentRowHeight : itemHeight > 0 ? itemHeight : 50;
		}

		totalExtent = currentMainOffset;

		return isVerticalScroll
			? new Size(availableMainAxis, totalExtent)
			: new Size(totalExtent, availableMainAxis);
	}

	protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
	{
		int itemCount = context.ItemCount;
		if (itemCount == 0)
		{
			return finalSize;
		}

		bool isVerticalScroll = _orientation == Orientation.Horizontal;
		double availableMainAxis = isVerticalScroll ? finalSize.Width : finalSize.Height;

		double columnSpacing = _minColumnSpacing;
		double rowSpacing = _minRowSpacing;

		double itemWidth = (availableMainAxis - (columnSpacing * (_span - 1))) / _span;

		Rect realizationRect = context.RealizationRect;

		double currentMainOffset = 0;
		int currentColumn = 0;
		double currentRowHeight = 0;
		int rowStartIndex = 0;

		for (int i = 0; i < itemCount; i++)
		{
			bool isFullSpan = IsFullSpanItem(context, i);

			if (isFullSpan)
			{
				// Complete current row - arrange pending items
				if (currentColumn > 0)
				{
					ArrangeRowItems(context, realizationRect, rowStartIndex, i - 1, currentMainOffset, 
						itemWidth, currentRowHeight, columnSpacing, isVerticalScroll, availableMainAxis);
					currentMainOffset += currentRowHeight + rowSpacing;
					currentColumn = 0;
					currentRowHeight = 0;
				}

				// Arrange full-span item
				bool isVisible = IsInRealizationRect(realizationRect, 0, currentMainOffset, availableMainAxis, 100, isVerticalScroll);
				if (isVisible)
				{
					var element = context.GetOrCreateElementAt(i, ElementRealizationOptions.ForceCreate);
					double height = element.DesiredSize.Height;

					Rect rect = isVerticalScroll
						? new Rect(0, currentMainOffset, availableMainAxis, height)
						: new Rect(currentMainOffset, 0, height, availableMainAxis);

					element.Arrange(rect);
					currentMainOffset += height + rowSpacing;
				}
				else
				{
					currentMainOffset += 50 + rowSpacing; // Skip with estimated height
				}

				rowStartIndex = i + 1;
			}
			else
			{
				// Track row info
				if (currentColumn == 0)
				{
					rowStartIndex = i;
				}

				// Get element to measure row height
				bool isVisible = IsInRealizationRect(realizationRect, currentColumn * (itemWidth + columnSpacing), 
					currentMainOffset, itemWidth, 100, isVerticalScroll);
				
				if (isVisible)
				{
					var element = context.GetOrCreateElementAt(i, ElementRealizationOptions.ForceCreate);
					currentRowHeight = Math.Max(currentRowHeight, element.DesiredSize.Height);
				}

				currentColumn++;
				if (currentColumn >= _span)
				{
					// Arrange this complete row
					ArrangeRowItems(context, realizationRect, rowStartIndex, i, currentMainOffset, 
						itemWidth, currentRowHeight > 0 ? currentRowHeight : 50, columnSpacing, isVerticalScroll, availableMainAxis);
					
					currentMainOffset += (currentRowHeight > 0 ? currentRowHeight : 50) + rowSpacing;
					currentColumn = 0;
					currentRowHeight = 0;
					rowStartIndex = i + 1;
				}
			}
		}

		// Arrange last incomplete row
		if (currentColumn > 0)
		{
			ArrangeRowItems(context, realizationRect, rowStartIndex, itemCount - 1, currentMainOffset, 
				itemWidth, currentRowHeight > 0 ? currentRowHeight : 50, columnSpacing, isVerticalScroll, availableMainAxis);
		}

		return finalSize;
	}

	void ArrangeRowItems(VirtualizingLayoutContext context, Rect realizationRect, int startIndex, int endIndex,
		double rowY, double itemWidth, double rowHeight, double columnSpacing, bool isVerticalScroll, double availableWidth)
	{
		int column = 0;
		for (int i = startIndex; i <= endIndex; i++)
		{
			// Skip full-span items (they're handled separately)
			if (IsFullSpanItem(context, i))
			{
				continue;
			}

			double x = column * (itemWidth + columnSpacing);

			bool isVisible = IsInRealizationRect(realizationRect, x, rowY, itemWidth, rowHeight, isVerticalScroll);
			if (isVisible)
			{
				var element = context.GetOrCreateElementAt(i, ElementRealizationOptions.ForceCreate);

				Rect rect = isVerticalScroll
					? new Rect(x, rowY, itemWidth, rowHeight)
					: new Rect(rowY, x, rowHeight, itemWidth);

				element.Arrange(rect);
			}

			column++;
		}
	}

	bool IsInRealizationRect(Rect realizationRect, double x, double y, double width, double height, bool isVerticalScroll)
	{
		if (realizationRect.IsEmpty)
		{
			return true; // Realize all if no rect
		}

		// Add buffer for smoother scrolling
		double buffer = Math.Max(realizationRect.Height, realizationRect.Width) * 0.5;

		if (isVerticalScroll)
		{
			double itemTop = y;
			double itemBottom = y + height;
			double viewTop = realizationRect.Y - buffer;
			double viewBottom = realizationRect.Y + realizationRect.Height + buffer;

			return itemBottom >= viewTop && itemTop <= viewBottom;
		}
		else
		{
			double itemLeft = y; // y is actually x in horizontal scroll
			double itemRight = y + height;
			double viewLeft = realizationRect.X - buffer;
			double viewRight = realizationRect.X + realizationRect.Width + buffer;

			return itemRight >= viewLeft && itemLeft <= viewRight;
		}
	}

	void RecycleElementIfExists(VirtualizingLayoutContext context, int index)
	{
		var element = context.GetOrCreateElementAt(index, ElementRealizationOptions.None);
		if (element is not null)
		{
			context.RecycleElement(element);
		}
	}

	static bool IsFullSpanItem(VirtualizingLayoutContext context, int index)
	{
		var data = context.GetItemAt(index);
		if (data is ItemTemplateContext2 templateContext)
		{
			return templateContext.IsHeader || templateContext.IsFooter;
		}
		return false;
	}
}
