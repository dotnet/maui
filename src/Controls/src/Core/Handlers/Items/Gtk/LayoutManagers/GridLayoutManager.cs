using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Point = Microsoft.Maui.Graphics.Point;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using NView = Gtk.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items.Platform
{

	/// <summary>
	/// A <see cref="ICollectionViewLayoutManager"/> implementation which provides grid layout
	/// </summary>
	public class GridLayoutManager : LayoutManagerBase
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="span">Column count</param>
		public GridLayoutManager(bool isHorizontal, int span = 1) : this(isHorizontal, span, ItemSizingStrategy.MeasureFirstItem)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="span">Column count</param>
		/// <param name="sizingStrategy">Item size measuring strategy</param>
		public GridLayoutManager(bool isHorizontal, int span, ItemSizingStrategy sizingStrategy) : this(isHorizontal, span, sizingStrategy, 0, 0)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="span">Column count</param>
		/// <param name="sizingStrategy">Item size measuring strategy</param>
		/// <param name="verticalSpacing">A space size between items</param>
		/// <param name="horizontalSpacing">A space size between items</param>
		public GridLayoutManager(bool isHorizontal, int span, ItemSizingStrategy sizingStrategy, int verticalSpacing, int horizontalSpacing) : base(isHorizontal, sizingStrategy)
		{
			Span = span;
			VerticalItemSpacing = verticalSpacing;
			HorizontalItemSpacing = horizontalSpacing;
		}

		/// <summary>
		/// A space size between items
		/// </summary>
		public double VerticalItemSpacing { get; }

		/// <summary>
		/// A space size between items
		/// </summary>
		public double HorizontalItemSpacing { get; }

		/// <summary>
		/// Column count
		/// </summary>
		public int Span { get; private set; }

		public void UpdateSpan(int span)
		{
			Span = span;
			InitializeMeasureCache();
			Controller!.RequestLayoutItems();
		}

		public override double ItemSpacing => IsHorizontal ? HorizontalItemSpacing : VerticalItemSpacing;

		double ItemWidthConstraint => IsHorizontal ? double.PositiveInfinity : ColumnSize;

		double ItemHeightConstraint => IsHorizontal ? ColumnSize : double.PositiveInfinity;

		double ColumnSize => (IsHorizontal ? _allocatedSize.Height / Span : _allocatedSize.Width / Span) - ((Span - 1) * ColumnSpacing / Span);

		double ColumnSpacing => IsHorizontal ? VerticalItemSpacing : HorizontalItemSpacing;

		protected override Size BaseItemBound
		{
			get
			{
				if (_baseItemBound == Size.Zero)
				{
					if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
						return Size.Zero;

					var itembound = Controller!.GetItemSize(ItemWidthConstraint, ItemHeightConstraint);
					_baseItemBound = itembound;
				}

				return _baseItemBound;
			}
		}

		public override Rect GetItemBound(int index)
		{
			int rowIndex = index / Span;
			int columnIndex = index % Span;
			double columnSize = ColumnSize;

			if (double.IsInfinity(columnSize))
				columnSize = BaseColumnSize;

			double rowStartPoint = 0;
			double columnStartPoint = 0;
			double itemSize = 0;

			if (!_hasUnevenRows)
			{
				itemSize = BaseItemSize;
				rowStartPoint = ItemStartPoint + rowIndex * (BaseItemSize + ItemSpacing);
				columnStartPoint = columnIndex * (columnSize + ColumnSpacing);
			}
			else if (_cached[index])
			{
				var updatedMaxItemSize = GetMaxItemSize(index);
				itemSize = _itemSizes[index];
				rowStartPoint = _accumulatedItemSizes[rowIndex] - updatedMaxItemSize + (updatedMaxItemSize - itemSize) / 2;
				columnStartPoint = columnIndex * (columnSize + ColumnSpacing);
			}
			else
			{
				var oldMaxItemSize = GetMaxItemSize(index);

				var measured = Controller!.GetItemSize(index, ItemWidthConstraint, ItemHeightConstraint);
				itemSize = IsHorizontal ? measured.Width : measured.Height;

				if (itemSize != _itemSizes[index])
				{
					_itemSizes[index] = itemSize;
				}

				var updatedMaxItemSize = GetMaxItemSize(index);

				if (oldMaxItemSize != updatedMaxItemSize)
				{
					UpdateAccumulatedItemSize(rowIndex, updatedMaxItemSize - oldMaxItemSize);
					int columnStart = (index / Span) * Span;

					for (int toUpdate = columnStart; toUpdate < index; toUpdate++)
					{
						if (_realizedItem.ContainsKey(toUpdate))
						{
							var updated = _realizedItem[toUpdate].Holder.Bounds;

							if (IsHorizontal)
							{
								updated.X += (updatedMaxItemSize - oldMaxItemSize) / 2;
							}
							else
							{
								updated.Y += (updatedMaxItemSize - oldMaxItemSize) / 2;
							}

							_realizedItem[toUpdate].Holder.UpdateBounds(updated);
						}
					}

					Controller!.ContentSizeUpdated();
				}

				rowStartPoint = _accumulatedItemSizes[rowIndex] - updatedMaxItemSize + (updatedMaxItemSize - itemSize) / 2;

				columnStartPoint = columnIndex * (columnSize + ColumnSpacing);

				_cached[index] = true;
			}

			return IsHorizontal ?
				new Rect(rowStartPoint, columnStartPoint, itemSize, columnSize) :
				new Rect(columnStartPoint, rowStartPoint, columnSize, itemSize);
		}

		public override int GetVisibleItemIndex(double x, double y)
		{
			int index = 0;

			if (x < 0 || y < 0)
				return index;

			if (_scrollCanvasSize.Width < x || _scrollCanvasSize.Height < y)
				return Controller!.Count - 1;

			int first = 0;

			if (!_hasUnevenRows)
			{
				first = Math.Min(Math.Max(0, (int)(((IsHorizontal ? x : y) - ItemStartPoint) / (BaseItemSize + ItemSpacing))), ((Controller!.Count - 1) / Span));
			}
			else
			{
				first = _accumulatedItemSizes.FindIndex(current => (IsHorizontal ? x : y) <= current);

				if (first == -1)
					first = (Controller!.Count - 1) / Span;
			}

			int second = (int)((IsHorizontal ? y : x) / (ColumnSize + ColumnSpacing));

			if (second == Span)
				second -= 1;

			index = (first * Span) + second;

			if (index < Controller!.Count)
				return index;

			return Controller!.Count - 1;
		}

		public override int NextRowItemIndex(int index)
		{
			return Math.Min(index + Span, Controller!.Count - 1);
		}

		public override int PreviousRowItemIndex(int index)
		{
			return Math.Max(index - Span, 0);
		}

		protected override void InitializeMeasureCache()
		{
			_baseItemBound = Size.Zero;
			_scrollCanvasSize = new Size(0, 0);
			_lastLayoutedBound = new Rect(0, 0, 0, 0);

			if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return;

			if (!_hasUnevenRows)
			{
				Controller!.ContentSizeUpdated();

				return;
			}

			int n = Controller!.Count;
			_itemSizes = new List<double>();
			_cached = new List<bool>();
			_accumulatedItemSizes = new List<double>();

			for (int i = 0; i < n; i++)
			{
				_cached.Add(false);
				_itemSizes.Add(BaseItemSize);

				if (i % Span == 0)
				{
					int accIndex = i / Span;
					_accumulatedItemSizes.Add((accIndex > 0 ? (_accumulatedItemSizes[accIndex - 1] + ItemSpacing) : ItemStartPoint) + _itemSizes[i]);
				}
			}

			Controller!.ContentSizeUpdated();
		}

		protected override void BuildAccumulatedSize()
		{
			_accumulatedItemSizes = new List<double>();
			int n = _itemSizes.Count;

			for (int i = 0; i < n; i++)
			{
				int accIndex = i / Span;
				double prevSize = accIndex > 0 ? (_accumulatedItemSizes[accIndex - 1] + ItemSpacing) : ItemStartPoint;

				if (i % Span == 0)
				{
					_accumulatedItemSizes.Add(prevSize);
				}

				double columnMax = _accumulatedItemSizes[accIndex] - prevSize;

				if (columnMax < _itemSizes[i])
				{
					_accumulatedItemSizes[accIndex] += (_itemSizes[i] - columnMax);
				}
			}
		}

		protected double GetMaxItemSize(int index)
		{
			int columnStart = (index / Span) * Span;
			int columnEnd = columnStart + Span - 1;
			double max = 0;

			for (int i = columnStart; i <= columnEnd && i < _itemSizes.Count; i++)
			{
				max = Math.Max(max, _itemSizes[i]);
			}

			return max;
		}

		protected override int GetStartIndex(Rect bound, double itemSize)
		{
			return (int)((ViewPortStartPoint(bound) - ItemStartPoint) / itemSize * Span);
		}

		protected override int GetStartIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetStartIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortStartPoint(bound)) * Span;
		}

		protected override int GetEndIndex(Rect bound, double itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize) * Span - 1;
		}

		protected override int GetEndIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetEndIndex(bound, BaseItemSize + ItemSpacing);
			}

			return (FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound)) + 1) * Span - 1;
		}

		public override Size GetScrollCanvasSize()
		{
			if (Controller!.Count == 0 || _allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return _allocatedSize;

			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
				return _scrollCanvasSize;

			double totalItemSize = 0;

			if (_hasUnevenRows)
			{
				// If item source was shared between adaptors, in some case CollectionView.Count could be wrong
				if (_accumulatedItemSizes.Count == 0)
				{
					return _allocatedSize;
				}

				totalItemSize = _accumulatedItemSizes[^1] + FooterSizeWithSpacing;
			}
			else
			{
				totalItemSize = (int)Math.Ceiling(Controller!.Count / (double)Span) * (BaseItemSize + ItemSpacing) - ItemSpacing + ItemStartPoint + FooterSizeWithSpacing;
			}

			if (IsHorizontal)
			{
				_scrollCanvasSize = new Size(totalItemSize, _allocatedSize.Height);
			}
			else
			{
				_scrollCanvasSize = new Size(_allocatedSize.Width, totalItemSize);
			}

			return _scrollCanvasSize;
		}

		public override double GetScrollColumnSize()
		{
			return (IsHorizontal ? BaseItemBound.Height : BaseItemBound.Width) * Span + ColumnSpacing * (Span - 1);
		}

		public override void LayoutItems(Rect bound, bool force)
		{
			if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return;

			// TODO : need to optimization. it was frequently called with similar bound value.
			if (!ShouldRearrange(bound) && !force)
			{
				return;
			}

			_isLayouting = true;
			_lastLayoutedBound = bound;

			int padding = Span;
			int startIndex = Math.Max(GetStartIndex(bound) - padding * 2, 0);
			int endIndex = Math.Min(GetEndIndex(bound) + padding * 2, Controller!.Count - 1);

			foreach (var index in _realizedItem.Keys.ToList())
			{
				if (index < startIndex || index > endIndex)
				{
					Controller!.UnrealizeView(_realizedItem[index].Holder);
					_realizedItem.Remove(index);
				}
			}

			for (int i = startIndex; i <= endIndex; i++)
			{
				NView? itemView = null;

				if (!_realizedItem.ContainsKey(i))
				{
					var holder = Controller!.RealizeView(i);

					_realizedItem[i] = new RealizedItem(holder, i);
					itemView = holder;
				}
				else
				{
					itemView = _realizedItem[i].Holder;
					itemView.Visible = true;
				}

				var itemBound = GetItemBound(i);
				itemView.UpdateBounds(itemBound);
			}

			_isLayouting = false;
		}

	}

}