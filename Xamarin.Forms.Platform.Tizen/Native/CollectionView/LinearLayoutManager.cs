using System;
using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using ESize = ElmSharp.Size;


namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class LinearLayoutManager : ICollectionViewLayoutManager
	{
		ESize _allocatedSize;
		bool _isLayouting;
		Rect _last;
		Dictionary<int, RealizedItem> _realizedItem = new Dictionary<int, RealizedItem>();
		List<int> _itemSizes;
		List<bool> _cached;
		List<int> _accumulatedItemSizes;

		bool _hasUnevenRows;
		int _baseItemSize;

		public LinearLayoutManager(bool isHorizontal) : this(isHorizontal, ItemSizingStrategy.MeasureFirstItem) { }

		public LinearLayoutManager(bool isHorizontal, ItemSizingStrategy sizingStrategy)
		{
			IsHorizontal = isHorizontal;
			_hasUnevenRows = sizingStrategy == ItemSizingStrategy.MeasureAllItems;
		}

		public bool IsHorizontal { get; }

		public ICollectionViewController CollectionView { get; set; }

		public void SizeAllocated(ESize size)
		{
			Reset();
			_allocatedSize = size;
			InitializeMeasureCache();
		}

		ESize _scrollCanvasSize;

		public ESize GetScrollCanvasSize()
		{
			if (CollectionView.Count == 0 || _allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
			{
				return _allocatedSize;
			}
				

			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
				return _scrollCanvasSize;

			int totalItemSize = 0;

			if (_hasUnevenRows)
			{
				totalItemSize = _accumulatedItemSizes[_accumulatedItemSizes.Count - 1];
			}
			else
			{
				totalItemSize = BaseItemSize * CollectionView.Count;
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

		int BaseItemSize
		{
			get
			{
				if (_baseItemSize == 0)
				{
					if (_allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
						return 0;

					var itemBound = CollectionView.GetItemSize(ItemWidthConstraint, ItemHeightConstraint);
					_baseItemSize = IsHorizontal ? itemBound.Width : itemBound.Height;
				}
				return _baseItemSize;
			}
		}

		int ItemWidthConstraint => IsHorizontal ? _allocatedSize.Width * 100 : _allocatedSize.Width;
		int ItemHeightConstraint => IsHorizontal ? _allocatedSize.Height : _allocatedSize.Height * 100;

		bool ShouldRearrange(Rect viewport)
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
			_last = bound;

			int startIndex = Math.Max(GetStartIndex(bound) - 2, 0);
			int endIndex = Math.Min(GetEndIndex(bound) + 2, CollectionView.Count - 1);

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

		public Rect GetItemBound(int index)
		{
			int itemSize = 0;
			int startPoint = 0;

			if (!_hasUnevenRows)
			{
				itemSize = BaseItemSize;
				startPoint = itemSize * index;
			}
			else if (index >= _itemSizes.Count)
			{
				return new Rect(0, 0, 0, 0);
			}
			else if (_cached[index])
			{
				itemSize = _itemSizes[index];
				startPoint = _accumulatedItemSizes[index] - itemSize;
			}
			else
			{
				var measured = CollectionView.GetItemSize(index, ItemWidthConstraint, ItemHeightConstraint);
				itemSize = IsHorizontal ? measured.Width : measured.Height;

				if (itemSize != _itemSizes[index])
				{
					UpdateAccumulatedItemSize(index, itemSize - _itemSizes[index]);
					_itemSizes[index] = itemSize;

					CollectionView.ContentSizeUpdated();
				}
				startPoint = _accumulatedItemSizes[index] - itemSize;
				_cached[index] = true;
			}

			return IsHorizontal ?
				new Rect(startPoint, 0, itemSize, _allocatedSize.Height) :
				new Rect(0, startPoint, _allocatedSize.Width, itemSize);
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

		void InitializeMeasureCache()
		{
			_baseItemSize = 0;
			_scrollCanvasSize = new ESize(0, 0);

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
				_accumulatedItemSizes.Add((i > 0 ? _accumulatedItemSizes[i - 1] : 0) + _itemSizes[i]);
			}
		}

		int GetStartIndex(Rect bound, int itemSize)
		{
			return ViewPortStartPoint(bound) / itemSize;
		}

		int GetStartIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetStartIndex(bound, BaseItemSize);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortStartPoint(bound));
		}

		int GetEndIndex(Rect bound, int itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize);
		}

		int GetEndIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetEndIndex(bound, BaseItemSize);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound));
		}

		int ViewPortStartPoint(Rect viewPort)
		{
			return IsHorizontal ? viewPort.X : viewPort.Y;
		}

		int ViewPortEndPoint(Rect viewPort)
		{
			return ViewPortStartPoint(viewPort) + ViewPortSize(viewPort);
		}

		int ViewPortSize(Rect viewPort)
		{
			return IsHorizontal ? viewPort.Width : viewPort.Height;
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
		}

		void UpdateRemovedSize(int removed)
		{
			if (!_hasUnevenRows)
				return;
			var removedSize = _itemSizes[removed];
			_itemSizes.RemoveAt(removed);
			UpdateAccumulatedItemSize(removed, -removedSize);
			_accumulatedItemSizes.RemoveAt(removed);
			_cached.RemoveAt(removed);
		}

		void UpdateInsertedSize(int inserted)
		{
			if (!_hasUnevenRows)
				return;

			_cached.Insert(inserted, false);
			_itemSizes.Insert(inserted, BaseItemSize);
			_accumulatedItemSizes.Insert(inserted, 0);
			_accumulatedItemSizes[inserted] = inserted > 0 ? _accumulatedItemSizes[inserted - 1] : 0;
			UpdateAccumulatedItemSize(inserted, BaseItemSize);
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
