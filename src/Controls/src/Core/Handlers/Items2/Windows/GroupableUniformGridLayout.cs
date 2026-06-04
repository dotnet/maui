using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

		readonly List<double> _bandStartOffsets = new();
		readonly List<double> _bandEndOffsets = new();
		readonly List<double> _bandExtents = new();
		readonly List<int> _bandStartIndices = new();
		readonly List<int> _bandEndIndices = new();

		int[] _itemToBand = Array.Empty<int>();
		bool _bandCacheValid;
		int _bandCacheItemCount = -1;
		int _bandCacheSpan;
		bool _bandCacheIsVertical;
		double _bandCacheRegularExtent;
		double _bandCacheHeaderExtent;
		double _bandCacheSpacing;
		double _cachedTotalPrimaryExtent;

		protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
		{
			if (context.ItemCount == 0)
			{
				InvalidateBandCache();
				return new Size(0, 0);
			}

			var span = Math.Max(1, MaximumRowsOrColumns);
			var isVerticalOrientation = Orientation == Orientation.Horizontal;

			EnsureBandCache(context, span, isVerticalOrientation);
			var realizationRange = GetRealizationIndexRange(context, span, isVerticalOrientation);

			double maxMeasuredRegular = 0;
			double maxMeasuredHeader = 0;

			for (int i = realizationRange.start; i <= realizationRange.end; i++)
			{
				var element = context.GetOrCreateElementAt(i);
				element.Measure(availableSize);

				bool isHeaderOrFooter = IsHeaderOrFooter(element);
				double primaryExtent = isVerticalOrientation ? element.DesiredSize.Height : element.DesiredSize.Width;

				if (isHeaderOrFooter)
				{
					maxMeasuredHeader = Math.Max(maxMeasuredHeader, primaryExtent);
				}
				else
				{
					maxMeasuredRegular = Math.Max(maxMeasuredRegular, primaryExtent);
				}
			}

			bool changed = UpdateEstimates(isVerticalOrientation, maxMeasuredRegular, maxMeasuredHeader);
			if (changed)
			{
				// Keep the newly grown estimate; only invalidate structure so EnsureBandCache
				// rebuilds using it. InvalidateBandCache() would reset the estimate we just set.
				InvalidateBandCacheStructure();
				EnsureBandCache(context, span, isVerticalOrientation);
			}

			if (isVerticalOrientation)
			{
				return new Size(availableSize.Width, _cachedTotalPrimaryExtent);
			}

			return new Size(_cachedTotalPrimaryExtent, availableSize.Height);
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
			var isVerticalOrientation = Orientation == Orientation.Horizontal;

			EnsureBandCache(context, span, isVerticalOrientation);
			var realizationRange = GetRealizationIndexRange(context, span, isVerticalOrientation);

			// Pre-measure all visible headers before the arrange loop. This lets us detect a
			// downward correction (header actual height < current estimate) and rebuild the band
			// cache inline so item bandStart offsets are already correct in the single arrange
			// pass below. Without this, items would be placed at wrong positions on the first
			// frame and a second layout pass would be needed to correct them (Issue #20855).
			double maxPreMeasuredHeader = 0;
			for (int i = realizationRange.start; i <= realizationRange.end; i++)
			{
				var child = context.GetOrCreateElementAt(i);
				if (!IsHeaderOrFooter(child))
					continue;

				if (isVerticalOrientation)
					child.Measure(new Size(finalSize.Width, double.PositiveInfinity));
				else
					child.Measure(new Size(double.PositiveInfinity, finalSize.Height));

				double measured = isVerticalOrientation ? child.DesiredSize.Height : child.DesiredSize.Width;
				maxPreMeasuredHeader = Math.Max(maxPreMeasuredHeader, measured);
			}

			// Apply downward header correction before arranging so band offsets are correct.
			if (maxPreMeasuredHeader > 0)
			{
				double currentEstimate = isVerticalOrientation
					? _estimatedHeaderVerticalExtent
					: _estimatedHeaderHorizontalExtent;
				if (maxPreMeasuredHeader < currentEstimate - 1.0)
				{
					if (isVerticalOrientation)
						_estimatedHeaderVerticalExtent = maxPreMeasuredHeader;
					else
						_estimatedHeaderHorizontalExtent = maxPreMeasuredHeader;

					// Rebuild inline with the corrected estimate so all bandStart offsets used
					// below already reflect the true header height.
					double regularExtent = GetEstimatedPrimarySize(isVerticalOrientation, isHeaderOrFooter: false);
					double headerExtent = maxPreMeasuredHeader;
					double spacing = isVerticalOrientation ? MinRowSpacing : MinColumnSpacing;
					RebuildBandCache(context, span, isVerticalOrientation, regularExtent, headerExtent, spacing);
				}
			}

			double cellWidth;
			double cellHeight;

			if (isVerticalOrientation)
			{
				double totalSpacing = minColumnSpacing * (span - 1);
				cellWidth = Math.Max(0, (finalSize.Width - totalSpacing) / span);
				cellHeight = _estimatedVerticalRegularExtent;
			}
			else
			{
				double totalSpacing = minRowSpacing * (span - 1);
				cellHeight = Math.Max(0, (finalSize.Height - totalSpacing) / span);
				cellWidth = _estimatedHorizontalRegularExtent;
			}

			double maxMeasuredRegular = 0;

			for (int i = realizationRange.start; i <= realizationRange.end; i++)
			{
				var child = context.GetOrCreateElementAt(i);
				bool isHeaderOrFooter = IsHeaderOrFooter(child);
				int bandIndex = GetBandIndex(i);

				if (bandIndex < 0)
				{
					continue;
				}

				double bandStart = _bandStartOffsets[bandIndex];
				double bandExtent = _bandExtents[bandIndex];

				if (isHeaderOrFooter)
				{
					// Already measured in the pre-measure pass above; use DesiredSize directly.
					if (isVerticalOrientation)
					{
						double headerHeight = child.DesiredSize.Height;
						child.Arrange(new Rect(0, bandStart, finalSize.Width, headerHeight > 0 ? headerHeight : bandExtent));
					}
					else
					{
						double headerWidth = child.DesiredSize.Width;
						child.Arrange(new Rect(bandStart, 0, headerWidth > 0 ? headerWidth : bandExtent, finalSize.Height));
					}
				}
				else
				{
					int slot = i - _bandStartIndices[bandIndex];
					if (isVerticalOrientation)
					{
						double x = slot * (cellWidth + minColumnSpacing);
						child.Arrange(new Rect(x, bandStart, cellWidth, bandExtent));
						maxMeasuredRegular = Math.Max(maxMeasuredRegular, child.DesiredSize.Height);
					}
					else
					{
						double y = slot * (cellHeight + minRowSpacing);
						child.Arrange(new Rect(bandStart, y, bandExtent, cellHeight));
						maxMeasuredRegular = Math.Max(maxMeasuredRegular, child.DesiredSize.Width);
					}
				}
			}

			// Only check for upward estimate growth; downward correction was already handled
			// in the pre-measure pass above before we began arranging.
			if (UpdateEstimates(isVerticalOrientation, maxMeasuredRegular, maxPreMeasuredHeader))
			{
				// Estimate grew upward — keep it, just invalidate structure so next pass rebuilds.
				InvalidateBandCacheStructure();
			}

			if (isVerticalOrientation)
			{
				return new Size(finalSize.Width, _cachedTotalPrimaryExtent);
			}

			return new Size(_cachedTotalPrimaryExtent, finalSize.Height);
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

			EnsureBandCache(context, span, isVerticalOrientation);
			if (_bandStartIndices.Count == 0)
			{
				return (0, context.ItemCount - 1);
			}

			var (realizationStart, realizationEnd) = GetRealizationPrimaryRange(context, isVerticalOrientation);

			int startBand = FindBandByOffset(realizationStart) - RealizationBufferRowsOrColumns;
			int endBand = FindBandByOffset(realizationEnd) + RealizationBufferRowsOrColumns;

			startBand = Math.Max(0, startBand);
			endBand = Math.Min(_bandStartIndices.Count - 1, endBand);
			if (endBand < startBand)
			{
				endBand = startBand;
			}

			return (_bandStartIndices[startBand], _bandEndIndices[endBand]);
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

		void EnsureBandCache(VirtualizingLayoutContext context, int span, bool isVerticalOrientation)
		{
			double regularExtent = GetEstimatedPrimarySize(isVerticalOrientation, isHeaderOrFooter: false);
			double headerExtent = GetEstimatedPrimarySize(isVerticalOrientation, isHeaderOrFooter: true);
			double spacing = isVerticalOrientation ? MinRowSpacing : MinColumnSpacing;

			if (_bandCacheValid
				&& _bandCacheItemCount == context.ItemCount
				&& _bandCacheSpan == span
				&& _bandCacheIsVertical == isVerticalOrientation
				&& Math.Abs(_bandCacheRegularExtent - regularExtent) < 0.1
				&& Math.Abs(_bandCacheHeaderExtent - headerExtent) < 0.1
				&& Math.Abs(_bandCacheSpacing - spacing) < 0.1)
			{
				return;
			}

			RebuildBandCache(context, span, isVerticalOrientation, regularExtent, headerExtent, spacing);
		}

		void RebuildBandCache(VirtualizingLayoutContext context, int span, bool isVerticalOrientation, double regularExtent, double headerExtent, double spacing)
		{
			_bandStartOffsets.Clear();
			_bandEndOffsets.Clear();
			_bandExtents.Clear();
			_bandStartIndices.Clear();
			_bandEndIndices.Clear();
			_cachedTotalPrimaryExtent = 0;

			if (_itemToBand.Length != context.ItemCount)
			{
				_itemToBand = new int[context.ItemCount];
			}

			Array.Fill(_itemToBand, -1);

			double currentPrimary = 0;
			int runStart = -1;
			int runCount = 0;

			for (int i = 0; i < context.ItemCount; i++)
			{
				if (IsHeaderOrFooterContext(context, i))
				{
					if (runCount > 0)
					{
						AddRegularBands(runStart, runCount, span, regularExtent, spacing, ref currentPrimary);
						runStart = -1;
						runCount = 0;
					}

					AddBand(i, i, isHeader: true, headerExtent, spacing, ref currentPrimary);
				}
				else
				{
					if (runStart < 0)
					{
						runStart = i;
					}

					runCount++;
				}
			}

			if (runCount > 0)
			{
				AddRegularBands(runStart, runCount, span, regularExtent, spacing, ref currentPrimary);
			}

			_cachedTotalPrimaryExtent = currentPrimary;
			_bandCacheValid = true;
			_bandCacheItemCount = context.ItemCount;
			_bandCacheSpan = span;
			_bandCacheIsVertical = isVerticalOrientation;
			_bandCacheRegularExtent = regularExtent;
			_bandCacheHeaderExtent = headerExtent;
			_bandCacheSpacing = spacing;
		}

		void AddRegularBands(int runStart, int runCount, int span, double regularExtent, double spacing, ref double currentPrimary)
		{
			int regularRunEnd = runStart + runCount - 1;
			int bandCount = (runCount + span - 1) / span;

			for (int band = 0; band < bandCount; band++)
			{
				int bandStart = runStart + (band * span);
				int bandEnd = Math.Min(regularRunEnd, bandStart + span - 1);
				AddBand(bandStart, bandEnd, isHeader: false, regularExtent, spacing, ref currentPrimary);
			}
		}

		void AddBand(int startIndex, int endIndex, bool isHeader, double extent, double spacing, ref double currentPrimary)
		{
			if (_bandStartOffsets.Count > 0)
			{
				currentPrimary += spacing;
			}

			double startOffset = currentPrimary;
			currentPrimary += extent;

			int bandIndex = _bandStartOffsets.Count;
			_bandStartOffsets.Add(startOffset);
			_bandEndOffsets.Add(currentPrimary);
			_bandExtents.Add(extent);
			_bandStartIndices.Add(startIndex);
			_bandEndIndices.Add(endIndex);

			for (int i = startIndex; i <= endIndex; i++)
			{
				_itemToBand[i] = bandIndex;
			}
		}

		int FindBandByOffset(double offset)
		{
			int lo = 0;
			int hi = _bandEndOffsets.Count - 1;
			if (hi < 0)
			{
				return 0;
			}

			while (lo <= hi)
			{
				int mid = lo + ((hi - lo) / 2);
				if (_bandEndOffsets[mid] < offset)
				{
					lo = mid + 1;
				}
				else
				{
					hi = mid - 1;
				}
			}

			return Math.Min(_bandEndOffsets.Count - 1, Math.Max(0, lo));
		}

		int GetBandIndex(int itemIndex)
		{
			if ((uint)itemIndex >= (uint)_itemToBand.Length)
			{
				return -1;
			}

			return _itemToBand[itemIndex];
		}

		bool UpdateEstimates(bool isVerticalOrientation, double measuredRegular, double measuredHeader)
		{
			bool changed = false;

			if (isVerticalOrientation)
			{
				if (measuredRegular > _estimatedVerticalRegularExtent + 0.1)
				{
					_estimatedVerticalRegularExtent = measuredRegular;
					changed = true;
				}

				if (measuredHeader > _estimatedHeaderVerticalExtent + 0.1)
				{
					_estimatedHeaderVerticalExtent = measuredHeader;
					changed = true;
				}
			}
			else
			{
				if (measuredRegular > _estimatedHorizontalRegularExtent + 0.1)
				{
					_estimatedHorizontalRegularExtent = measuredRegular;
					changed = true;
				}

				if (measuredHeader > _estimatedHeaderHorizontalExtent + 0.1)
				{
					_estimatedHeaderHorizontalExtent = measuredHeader;
					changed = true;
				}
			}

			return changed;
		}

		// Invalidates band cache structure only. Keeps current estimates so the next
		// RebuildBandCache uses the grown values. Use this after an upward estimate change.
		void InvalidateBandCacheStructure()
		{
			_bandCacheValid = false;
			_bandCacheItemCount = -1;
			_cachedTotalPrimaryExtent = 0;
		}

		// Full reset: invalidates structure AND resets estimates to defaults.
		// Use this only when estimates need to shrink (downward correction) or
		// when the item collection changes entirely.
		void InvalidateBandCache()
		{
			InvalidateBandCacheStructure();
			_estimatedVerticalRegularExtent = DefaultEstimatedRegularExtent;
			_estimatedHorizontalRegularExtent = DefaultEstimatedRegularExtent;
			_estimatedHeaderVerticalExtent = DefaultEstimatedHeaderExtent;
			_estimatedHeaderHorizontalExtent = DefaultEstimatedHeaderExtent;
		}

		protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
		{
			InvalidateBandCache();
			base.OnItemsChangedCore(context, source, args);
		}
	}
}
