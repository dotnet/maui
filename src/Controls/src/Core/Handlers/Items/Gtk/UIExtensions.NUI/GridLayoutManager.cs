using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Point = Microsoft.Maui.Graphics.Point;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using NView = Gtk.Widget;

namespace Gtk.UIExtensions.NUI
{

	/// <summary>
	/// A <see cref="ICollectionViewLayoutManager"/> implementation which provides grid layout
	/// </summary>
	public class GridLayoutManager : ICollectionViewLayoutManager
	{

		Size _allocatedSize;
		Size _scrollCanvasSize;
		bool _isLayouting;
		Rect _lastLayoutedBound;
		Dictionary<int, RealizedItem> _realizedItem = new();
		List<double> _itemSizes = new();
		List<bool> _cached = new();
		List<double> _accumulatedItemSizes = new();
		bool _hasUnevenRows;
		Size _baseItemBound;

		Size _headerSize;
		NView? _header;
		Size _footerSize;
		NView? _footer;

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="span">Column count</param>
		public GridLayoutManager(bool isHorizontal, int span = 1) : this(isHorizontal, span, ItemSizingStrategy.MeasureFirstItem) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="span">Column count</param>
		/// <param name="sizingStrategy">Item size measuring strategy</param>
		public GridLayoutManager(bool isHorizontal, int span, ItemSizingStrategy sizingStrategy) : this(isHorizontal, span, sizingStrategy, 0, 0) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="span">Column count</param>
		/// <param name="sizingStrategy">Item size measuring strategy</param>
		/// <param name="verticalSpacing">A space size between items</param>
		/// <param name="horizontalSpacing">A space size between items</param>
		public GridLayoutManager(bool isHorizontal, int span, ItemSizingStrategy sizingStrategy, int verticalSpacing, int horizontalSpacing)
		{
			IsHorizontal = isHorizontal;
			Span = span;
			_hasUnevenRows = sizingStrategy == ItemSizingStrategy.MeasureAllItems;
			VerticalItemSpacing = verticalSpacing;
			HorizontalItemSpacing = horizontalSpacing;
		}

		/// <summary>
		/// Column count
		/// </summary>
		public int Span { get; private set; }

		/// <summary>
		/// Whether the item is a layout horizontally
		/// </summary>
		public bool IsHorizontal { get; }

		/// <summary>
		/// A space size between items
		/// </summary>
		public double VerticalItemSpacing { get; }

		/// <summary>
		/// A space size between items
		/// </summary>
		public double HorizontalItemSpacing { get; }

		/// <summary>
		/// CollectionView that interact with layout manager
		/// </summary>
		public ICollectionViewController? CollectionView { get; set; }

		double BaseItemSize
		{
			get => IsHorizontal ? BaseItemBound.Width : BaseItemBound.Height;
		}

		Size BaseItemBound
		{
			get
			{
				if (_baseItemBound == Size.Zero)
				{
					if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
						return Size.Zero;

					var itembound = CollectionView!.GetItemSize(ItemWidthConstraint, ItemHeightConstraint);
					_baseItemBound = itembound;
				}

				return _baseItemBound;
			}
		}

		double ItemSpacing => IsHorizontal ? HorizontalItemSpacing : VerticalItemSpacing;

		double ItemWidthConstraint => IsHorizontal ? double.PositiveInfinity : ColumnSize;

		double ItemHeightConstraint => IsHorizontal ? ColumnSize : double.PositiveInfinity;

		double ColumnSize => (IsHorizontal ? _allocatedSize.Height / Span : _allocatedSize.Width / Span) - ((Span - 1) * ColumnSpacing / Span);

		double ColumnSpacing => IsHorizontal ? VerticalItemSpacing : HorizontalItemSpacing;

		double FooterSize => IsHorizontal ? _footerSize.Width : _footerSize.Height;

		double HeaderSize => IsHorizontal ? _headerSize.Width : _headerSize.Height;

		double ItemStartPoint
		{
			get
			{
				var startPoint = HeaderSize;

				if (startPoint > 0)
				{
					startPoint += ItemSpacing;
				}

				return startPoint;
			}
		}

		double FooterSizeWithSpacing
		{
			get
			{
				var size = FooterSize;

				if (size > 0)
				{
					size += ItemSpacing;
				}

				return size;
			}
		}

		public void SizeAllocated(Size size)
		{
			_allocatedSize = size;
			InitializeMeasureCache();
		}

		public Size GetScrollCanvasSize()
		{
			if (CollectionView!.Count == 0 || _allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
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

				totalItemSize = _accumulatedItemSizes[_accumulatedItemSizes.Count - 1] + FooterSizeWithSpacing;
			}
			else
			{
				totalItemSize = (int)Math.Ceiling(CollectionView!.Count / (double)Span) * (BaseItemSize + ItemSpacing) - ItemSpacing + ItemStartPoint + FooterSizeWithSpacing;
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

		public double GetScrollBlockSize()
		{
			return BaseItemSize + ItemSpacing;
		}

		public double GetScrollColumnSize()
		{
			return (IsHorizontal ? BaseItemBound.Height : BaseItemBound.Width) * Span + ColumnSpacing * (Span - 1);
		}

		public void LayoutItems(Rect bound, bool force)
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
			int endIndex = Math.Min(GetEndIndex(bound) + padding * 2, CollectionView!.Count - 1);

			foreach (var index in _realizedItem.Keys.ToList())
			{
				if (index < startIndex || index > endIndex)
				{
					CollectionView!.UnrealizeView(_realizedItem[index].Holder);
					_realizedItem.Remove(index);
				}
			}

			for (int i = startIndex; i <= endIndex; i++)
			{
				NView? itemView = null;

				if (!_realizedItem.ContainsKey(i))
				{
					var holder = CollectionView!.RealizeView(i);

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

		public void UpdateSpan(int span)
		{
			Span = span;
			InitializeMeasureCache();
			CollectionView!.RequestLayoutItems();
		}

		public void ItemInserted(int inserted)
		{
			var items = _realizedItem.Keys.OrderByDescending(key => key);

			foreach (var index in items)
			{
				if (index >= inserted)
				{
					_realizedItem[index + 1] = _realizedItem[index];
				}
			}

			if (_realizedItem.ContainsKey(inserted))
			{
				_realizedItem.Remove(inserted);
			}
			else
			{
				var last = items.LastOrDefault();

				if (last >= inserted)
				{
					_realizedItem.Remove(last);
				}
			}

			UpdateInsertedSize(inserted);

			_scrollCanvasSize = new Size(0, 0);
			CollectionView!.ContentSizeUpdated();
		}

		public void ItemRemoved(int removed)
		{
			if (_realizedItem.ContainsKey(removed))
			{
				CollectionView!.UnrealizeView(_realizedItem[removed].Holder);
				_realizedItem.Remove(removed);
			}

			var items = _realizedItem.Keys.OrderBy(key => key);

			foreach (var index in items)
			{
				if (index > removed)
				{
					_realizedItem[index - 1] = _realizedItem[index];
				}
			}

			var last = items.LastOrDefault();

			if (last > removed)
			{
				_realizedItem.Remove(last);
			}

			UpdateRemovedSize(removed);

			_scrollCanvasSize = new Size(0, 0);
			CollectionView!.ContentSizeUpdated();
		}

		public void ItemUpdated(int index)
		{
			if (_realizedItem.ContainsKey(index))
			{
				var bound = _realizedItem[index].Holder.Bounds;
				CollectionView!.UnrealizeView(_realizedItem[index].Holder);
				var view = CollectionView!.RealizeView(index);
				_realizedItem[index].Holder = view;
				view.UpdateBounds(bound);
			}
		}

		public Rect GetItemBound(int index)
		{
			int rowIndex = index / Span;
			int columnIndex = index % Span;
			double columnSize = ColumnSize;

			if (double.IsInfinity(columnSize))
				columnSize = BaseItemSize;

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

				var measured = CollectionView!.GetItemSize(index, ItemWidthConstraint, ItemHeightConstraint);
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

					CollectionView!.ContentSizeUpdated();
				}

				rowStartPoint = _accumulatedItemSizes[rowIndex] - updatedMaxItemSize + (updatedMaxItemSize - itemSize) / 2;

				columnStartPoint = columnIndex * (columnSize + ColumnSpacing);

				_cached[index] = true;
			}

			return IsHorizontal ?
				new Rect(rowStartPoint, columnStartPoint, itemSize, columnSize) :
				new Rect(columnStartPoint, rowStartPoint, columnSize, itemSize);
		}

		public void Reset()
		{
			foreach (var realizedItem in _realizedItem.Values.ToList())
			{
				CollectionView!.UnrealizeView(realizedItem.Holder);
			}

			_realizedItem.Clear();
			_scrollCanvasSize = new Size(0, 0);
			CollectionView!.ContentSizeUpdated();
		}

		public void ItemSourceUpdated()
		{
			InitializeMeasureCache();
		}

		public void ItemMeasureInvalidated(int index)
		{
			if (_hasUnevenRows)
			{
				if (index >= 0 && _cached.Count > index)
					_cached[index] = false;

				if (_realizedItem.ContainsKey(index))
				{
					CollectionView!.RequestLayoutItems();
				}
			}
			else if (index == 0) // MeasureFirstItem
			{
				// Reset item size to measure updated size
				InitializeMeasureCache();
				CollectionView!.RequestLayoutItems();
			}
		}

		public int GetVisibleItemIndex(double x, double y)
		{
			int index = 0;

			if (x < 0 || y < 0)
				return index;

			if (_scrollCanvasSize.Width < x || _scrollCanvasSize.Height < y)
				return CollectionView!.Count - 1;

			int first = 0;

			if (!_hasUnevenRows)
			{
				first = Math.Min(Math.Max(0, (int)(((IsHorizontal ? x : y) - ItemStartPoint) / (BaseItemSize + ItemSpacing))), ((CollectionView!.Count - 1) / Span));
			}
			else
			{
				first = _accumulatedItemSizes.FindIndex(current => (IsHorizontal ? x : y) <= current);

				if (first == -1)
					first = (CollectionView!.Count - 1) / Span;
			}

			int second = (int)((IsHorizontal ? y : x) / (ColumnSize + ColumnSpacing));

			if (second == Span)
				second -= 1;

			index = (first * Span) + second;

			if (index < CollectionView!.Count)
				return index;

			return CollectionView!.Count - 1;
		}

		public void SetHeader(NView? header, Size size)
		{
			bool contentSizeChanged = false;

			if (IsHorizontal)
			{
				if (_headerSize.Width != size.Width)
					contentSizeChanged = true;
			}
			else
			{
				if (_headerSize.Height != size.Height)
					contentSizeChanged = true;
			}

			_header = header;
			_headerSize = size;

			if (contentSizeChanged)
			{
				InitializeMeasureCache();
				CollectionView!.ContentSizeUpdated();
			}

			if (_header != null)
			{
				var bound = new Rect(0, 0, _headerSize.Width, _headerSize.Height);

				if (IsHorizontal)
				{
					bound.Height = _allocatedSize.Height;
				}
				else
				{
					bound.Width = _allocatedSize.Width;
				}

				_header.UpdateBounds(bound);
			}
		}

		public void SetFooter(NView? footer, Size size)
		{
			bool contentSizeChanged = false;

			if (IsHorizontal)
			{
				if (_footerSize.Width != size.Width)
					contentSizeChanged = true;
			}
			else
			{
				if (_footerSize.Height != size.Height)
					contentSizeChanged = true;
			}

			_footer = footer;
			_footerSize = size;

			if (contentSizeChanged)
			{
				InitializeMeasureCache();
				CollectionView!.ContentSizeUpdated();
			}

			UpdateFooterPosition();
		}

		public int NextRowItemIndex(int index)
		{
			return Math.Min(index + Span, CollectionView!.Count - 1);
		}

		public int PreviousRowItemIndex(int index)
		{
			return Math.Max(index - Span, 0);
		}

		void UpdateFooterPosition()
		{
			if (_footer == null)
				return;

			var position = new Point();

			if (IsHorizontal)
			{
				position.X += (GetScrollCanvasSize().Width - _footerSize.Width);
			}
			else
			{
				position.Y += (GetScrollCanvasSize().Height - _footerSize.Height);
			}

			var bound = new Rect(position.X, position.Y, _footerSize.Width, _footerSize.Height);

			if (IsHorizontal)
			{
				bound.Height = _allocatedSize.Height;
			}
			else
			{
				bound.Width = _allocatedSize.Width;
			}

			_footer.UpdateBounds(bound);
		}

		void InitializeMeasureCache()
		{
			_baseItemBound = Size.Zero;
			_scrollCanvasSize = new Size(0, 0);
			_lastLayoutedBound = new Rect(0, 0, 0, 0);

			if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return;

			if (!_hasUnevenRows)
			{
				CollectionView!.ContentSizeUpdated();

				return;
			}

			int n = CollectionView!.Count;
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

			CollectionView!.ContentSizeUpdated();
		}

		void BuildAccumulatedSize()
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

		void UpdateInsertedSize(int inserted)
		{
			if (!_hasUnevenRows)
				return;

			_cached.Insert(inserted, false);
			_itemSizes.Insert(inserted, BaseItemSize);

			BuildAccumulatedSize();
		}

		void UpdateRemovedSize(int removed)
		{
			if (!_hasUnevenRows)
				return;

			_itemSizes.RemoveAt(removed);

			_cached.RemoveAt(removed);
			BuildAccumulatedSize();
		}

		void UpdateAccumulatedItemSize(int index, double diff)
		{
			for (int i = index; i < _accumulatedItemSizes.Count; i++)
			{
				_accumulatedItemSizes[i] += diff;
			}

			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
			{
				if (IsHorizontal)
				{
					_scrollCanvasSize.Width += diff;
				}
				else
				{
					_scrollCanvasSize.Height += diff;
				}
			}

			UpdateFooterPosition();
		}

		double GetMaxItemSize(int index)
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

		int GetStartIndex(Rect bound, double itemSize)
		{
			return (int)((ViewPortStartPoint(bound) - ItemStartPoint) / itemSize * Span);
		}

		int GetStartIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetStartIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortStartPoint(bound)) * Span;
		}

		int GetEndIndex(Rect bound, double itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize) * Span - 1;
		}

		int GetEndIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetEndIndex(bound, BaseItemSize + ItemSpacing);
			}

			return (FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound)) + 1) * Span - 1;
		}

		double ViewPortStartPoint(Rect viewPort)
		{
			return IsHorizontal ? viewPort.X : viewPort.Y;
		}

		double ViewPortEndPoint(Rect viewPort)
		{
			return ViewPortStartPoint(viewPort) + ViewPortSize(viewPort);
		}

		double ViewPortSize(Rect viewPort)
		{
			return IsHorizontal ? viewPort.Width : viewPort.Height;
		}

		bool ShouldRearrange(Rect viewport)
		{
			if (_isLayouting)
				return false;

			if (_lastLayoutedBound.Size != viewport.Size)
				return true;

			var diff = IsHorizontal ? Math.Abs(_lastLayoutedBound.X - viewport.X) : Math.Abs(_lastLayoutedBound.Y - viewport.Y);

			if (diff > BaseItemSize)
				return true;

			return false;
		}

		static int FindFirstGreaterOrEqualTo(IList<double> data, double value)
		{
			if (data.Count == 0)
				return 0;

			int start = 0;
			int end = data.Count - 1;

			while (start < end)
			{
				int mid = (start + end) / 2;

				if (data[mid] < value)
				{
					start = mid + 1;
				}
				else
				{
					end = mid - 1;
				}
			}

			if (data[start] < value)
			{
				start++;
			}

			return start;
		}

		class RealizedItem
		{

			public RealizedItem(ViewHolder holder, int index)
			{
				Holder = holder;
				Index = index;
			}

			public ViewHolder Holder { get; set; }

			public int Index { get; set; }

		}

	}

}