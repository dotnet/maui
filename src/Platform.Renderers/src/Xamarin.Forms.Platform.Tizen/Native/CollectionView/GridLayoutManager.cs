using System;
using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class GridLayoutManager : ICollectionViewLayoutManager
	{
		ESize _allocatedSize;
		ESize _scrollCanvasSize;
		bool _isLayouting;
		ERect _last;
		Dictionary<int, RealizedItem> _realizedItem = new Dictionary<int, RealizedItem>();

		List<int> _itemSizes;
		List<bool> _cached;
		List<int> _accumulatedItemSizes;
		bool _hasUnevenRows;
		int _baseItemSize;

		ESize _headerSize;
		EvasObject _header;
		ESize _footerSize;
		EvasObject _footer;


		public GridLayoutManager(bool isHorizontal, int span = 1) : this(isHorizontal, span, ItemSizingStrategy.MeasureFirstItem) { }

		public GridLayoutManager(bool isHorizontal, int span, ItemSizingStrategy sizingStrategy) : this(isHorizontal, span, sizingStrategy, 0, 0) { }

		public GridLayoutManager(bool isHorizontal, int span, ItemSizingStrategy sizingStrategy, int verticalSpacing, int horizontalSpacing)
		{
			IsHorizontal = isHorizontal;
			Span = span;
			_hasUnevenRows = sizingStrategy == ItemSizingStrategy.MeasureAllItems;
			VerticalItemSpacing = verticalSpacing;
			HorizontalItemSpacing = horizontalSpacing;
		}

		public int Span { get; private set; }

		public bool IsHorizontal { get; }

		public int VerticalItemSpacing { get; }

		public int HorizontalItemSpacing { get; }

		public ICollectionViewController CollectionView { get; set; }

		public void SizeAllocated(ESize size)
		{
			Reset();
			_allocatedSize = size;
			InitializeMeasureCache();
		}

		public ESize GetScrollCanvasSize()
		{
			if (CollectionView.Count == 0 || _allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return _allocatedSize;

			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
				return _scrollCanvasSize;

			int totalItemSize = 0;

			if (_hasUnevenRows)
			{
				totalItemSize = _accumulatedItemSizes[_accumulatedItemSizes.Count - 1] + FooterSizeWithSpacing;
			}
			else
			{
				totalItemSize = (int)Math.Ceiling(CollectionView.Count / (double)Span) * (BaseItemSize + ItemSpacing) - ItemSpacing + ItemStartPoint + FooterSizeWithSpacing;
			}

			if (IsHorizontal)
			{
				_scrollCanvasSize = new ESize(totalItemSize, _allocatedSize.Height);
			}
			else
			{
				_scrollCanvasSize = new ESize(_allocatedSize.Width, totalItemSize);
			}

			return _scrollCanvasSize;
		}

		public int GetScrollBlockSize()
		{
			return BaseItemSize + ItemSpacing;
		}

		int BaseItemSize
		{
			get
			{
				if (_baseItemSize == 0)
				{
					if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
						return 0;

					var itembound = CollectionView.GetItemSize(ItemWidthConstraint, ItemHeightConstraint);
					_baseItemSize = IsHorizontal ? itembound.Width : itembound.Height;
				}
				return _baseItemSize;
			}
		}

		int ItemSpacing => IsHorizontal ? HorizontalItemSpacing : VerticalItemSpacing;

		int ItemWidthConstraint => IsHorizontal ? _allocatedSize.Width * 100 : ColumnSize;
		int ItemHeightConstraint => IsHorizontal ? ColumnSize : _allocatedSize.Height * 100;

		int ColumnSize
		{
			get
			{
				return (IsHorizontal ? _allocatedSize.Height / Span : _allocatedSize.Width / Span) - ((Span - 1) * ColumnSpacing / Span);
			}
		}

		int ColumnSpacing => IsHorizontal ? VerticalItemSpacing : HorizontalItemSpacing;

		int FooterSize => IsHorizontal ? _footerSize.Width : _footerSize.Height;

		int HeaderSize => IsHorizontal ? _headerSize.Width : _headerSize.Height;

		int ItemStartPoint
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

		int FooterSizeWithSpacing
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

		bool ShouldRearrange(ERect viewport)
		{
			if (_isLayouting)
				return false;
			if (_last.Size != viewport.Size)
				return true;

			var diff = IsHorizontal ? Math.Abs(_last.X - viewport.X) : Math.Abs(_last.Y - viewport.Y);
			if (diff > BaseItemSize)
				return true;

			return false;
		}

		public void LayoutItems(ERect bound, bool force)
		{
			if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return;

			// TODO : need to optimization. it was frequently called with similar bound value.
			if (!ShouldRearrange(bound) && !force)
			{
				return;
			}
			_isLayouting = true;
			_last = bound;

			int padding = Span;
			int startIndex = Math.Max(GetStartIndex(bound) - padding, 0);
			int endIndex = Math.Min(GetEndIndex(bound) + padding, CollectionView.Count - 1);

			foreach (var index in _realizedItem.Keys.ToList())
			{
				if (index < startIndex || index > endIndex)
				{
					CollectionView.UnrealizeView(_realizedItem[index].View);
					_realizedItem.Remove(index);
				}
			}

			var parent = CollectionView.ParentPosition;
			for (int i = startIndex; i <= endIndex; i++)
			{
				EvasObject itemView = null;
				if (!_realizedItem.ContainsKey(i))
				{
					var view = CollectionView.RealizeView(i);

					_realizedItem[i] = new RealizedItem
					{
						View = view,
						Index = i,
					};
					itemView = view;
				}
				else
				{
					itemView = _realizedItem[i].View;
				}

				var itemBound = GetItemBound(i);
				itemBound.X += parent.X;
				itemBound.Y += parent.Y;
				itemView.Geometry = itemBound;
			}
			_isLayouting = false;
		}

		public void UpdateSpan(int span)
		{
			Span = span;
			InitializeMeasureCache();
			CollectionView.RequestLayoutItems();
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

			_scrollCanvasSize = new ESize(0, 0);
		}

		public void ItemRemoved(int removed)
		{
			if (_realizedItem.ContainsKey(removed))
			{
				CollectionView.UnrealizeView(_realizedItem[removed].View);
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
			_scrollCanvasSize = new ESize(0, 0);
		}

		public void ItemUpdated(int index)
		{
			if (_realizedItem.ContainsKey(index))
			{
				var bound = _realizedItem[index].View.Geometry;
				CollectionView.UnrealizeView(_realizedItem[index].View);
				var view = CollectionView.RealizeView(index);
				_realizedItem[index].View = view;
				view.Geometry = bound;
			}
		}

		public ERect GetItemBound(int index)
		{
			int rowIndex = index / Span;
			int columnIndex = index % Span;
			var columnSize = ColumnSize;

			int rowStartPoint = 0;
			int columnStartPoint = 0;
			int itemSize = 0;

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

				var measured = CollectionView.GetItemSize(index, ItemWidthConstraint, ItemHeightConstraint);
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
							var updated = _realizedItem[toUpdate].View.Geometry;
							if (IsHorizontal)
							{
								updated.X += (updatedMaxItemSize - oldMaxItemSize) / 2;
							}
							else
							{
								updated.Y += (updatedMaxItemSize - oldMaxItemSize) / 2;
							}
							_realizedItem[toUpdate].View.Geometry = updated;
						}
					}
					CollectionView.ContentSizeUpdated();
				}
				rowStartPoint = _accumulatedItemSizes[rowIndex] - updatedMaxItemSize + (updatedMaxItemSize - itemSize) / 2;
				columnStartPoint = columnIndex * (columnSize + ColumnSpacing);

				_cached[index] = true;
			}

			return IsHorizontal ?
				new ERect(rowStartPoint, columnStartPoint, itemSize, columnSize) :
				new ERect(columnStartPoint, rowStartPoint, columnSize, itemSize);
		}

		public void Reset()
		{
			foreach (var realizedItem in _realizedItem.Values.ToList())
			{
				CollectionView.UnrealizeView(realizedItem.View);
			}
			_realizedItem.Clear();
			_scrollCanvasSize = new ESize(0, 0);
		}

		public void ItemSourceUpdated()
		{
			InitializeMeasureCache();
		}

		public void ItemMeasureInvalidated(int index)
		{
			if (_realizedItem.ContainsKey(index))
			{
				CollectionView.RequestLayoutItems();
			}
			if (_hasUnevenRows)
			{
				if (_cached.Count > index)
					_cached[index] = false;
			}
		}

		public int GetVisibleItemIndex(int x, int y)
		{
			int index = 0;
			if (x < 0 || y < 0)
				return index;
			if (_scrollCanvasSize.Width < x || _scrollCanvasSize.Height < y)
				return CollectionView.Count - 1;

			int first = 0;
			if (!_hasUnevenRows)
			{
				first = Math.Min(Math.Max(0, ((IsHorizontal ? x : y) - ItemStartPoint) / (BaseItemSize + ItemSpacing)), ((CollectionView.Count - 1) / Span));
			}
			else
			{
				first = _accumulatedItemSizes.FindIndex(current => (IsHorizontal ? x : y) <= current);
				if (first == -1)
					first = (CollectionView.Count - 1) / Span;
			}

			int second = (IsHorizontal ? y : x) / (ColumnSize + ColumnSpacing);
			if (second == Span)
				second -= 1;

			index = (first * Span) + second;

			if (index < CollectionView.Count)
				return index;
			return CollectionView.Count - 1;
		}

		public void SetHeader(EvasObject header, ESize size)
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
				CollectionView.ContentSizeUpdated();
			}

			var position = CollectionView.ParentPosition;
			if (_header != null)
			{
				var bound = new ERect(position.X, position.Y, _headerSize.Width, _headerSize.Height);
				if (IsHorizontal)
				{
					bound.Height = _allocatedSize.Height;
				}
				else
				{
					bound.Width = _allocatedSize.Width;
				}
				_header.Geometry = bound;
			}
		}

		public void SetFooter(EvasObject footer, ESize size)
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
				CollectionView.ContentSizeUpdated();
			}

			UpdateFooterPosition();
		}

		void UpdateFooterPosition()
		{
			if (_footer == null)
				return;

			var position = CollectionView.ParentPosition;
			if (IsHorizontal)
			{
				position.X += (GetScrollCanvasSize().Width - _footerSize.Width);
			}
			else
			{
				position.Y += (GetScrollCanvasSize().Height - _footerSize.Height);
			}

			var bound = new ERect(position.X, position.Y, _footerSize.Width, _footerSize.Height);
			if (IsHorizontal)
			{
				bound.Height = _allocatedSize.Height;
			}
			else
			{
				bound.Width = _allocatedSize.Width;
			}
			_footer.Geometry = bound;
		}

		void InitializeMeasureCache()
		{
			_baseItemSize = 0;
			_scrollCanvasSize = new ESize(0, 0);
			_last = new ERect(0, 0, 0, 0);

			if (!_hasUnevenRows)
				return;

			if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
				return;

			int n = CollectionView.Count;
			_itemSizes = new List<int>();
			_cached = new List<bool>();
			_accumulatedItemSizes = new List<int>();

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
		}

		void BuildAccumulatedSize()
		{
			_accumulatedItemSizes = new List<int>();
			int n = _itemSizes.Count;
			for (int i = 0; i < n; i++)
			{
				int accIndex = i / Span;
				int prevSize = accIndex > 0 ? (_accumulatedItemSizes[accIndex - 1] + ItemSpacing) : 0;
				if (i % Span == 0)
				{
					_accumulatedItemSizes.Add(prevSize);
				}
				int columnMax = _accumulatedItemSizes[accIndex] - prevSize;
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

		void UpdateAccumulatedItemSize(int index, int diff)
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

		int GetMaxItemSize(int index)
		{
			int columnStart = (index / Span) * Span;
			int columnEnd = columnStart + Span - 1;
			int max = 0;
			for (int i = columnStart; i <= columnEnd && i < _itemSizes.Count; i++)
			{
				max = Math.Max(max, _itemSizes[i]);
			}
			return max;
		}

		int GetStartIndex(ERect bound, int itemSize)
		{
			return (ViewPortStartPoint(bound) - ItemStartPoint) / itemSize * Span;
		}

		int GetStartIndex(ERect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetStartIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortStartPoint(bound)) * Span;
		}

		int GetEndIndex(ERect bound, int itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize) * Span - 1;
		}

		int GetEndIndex(ERect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetEndIndex(bound, BaseItemSize + ItemSpacing);
			}
			var tmp = FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound));

			return (FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound)) + 1) * Span - 1;
		}

		int ViewPortStartPoint(ERect viewPort)
		{
			return IsHorizontal ? viewPort.X : viewPort.Y;
		}

		int ViewPortEndPoint(ERect viewPort)
		{
			return ViewPortStartPoint(viewPort) + ViewPortSize(viewPort);
		}

		int ViewPortSize(ERect viewPort)
		{
			return IsHorizontal ? viewPort.Width : viewPort.Height;
		}

		static int FindFirstGreaterOrEqualTo(IList<int> data, int value)
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
			public ViewHolder View { get; set; }
			public int Index { get; set; }
		}
	}
}
