using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using View = Gtk.Widget;

namespace Gtk.UIExtensions.NUI
{

	/// <summary>
	/// A <see cref="ICollectionViewLayoutManager"/> implementation which provides linear layout
	/// </summary>
	public class LinearLayoutManager : ICollectionViewLayoutManager
	{

		Size _allocatedSize;
		bool _isLayouting;
		Rect _lastLayoutedBound;
		Dictionary<int, RealizedItem> _realizedItem = new Dictionary<int, RealizedItem>();
		List<double> _itemSizes = new List<double>();
		List<bool> _cached = new List<bool>();
		List<double> _accumulatedItemSizes = new List<double>();

		bool _hasUnevenRows;
		Size _baseItemBound;

		Size _headerSize;
		View? _header;
		Size _footerSize;
		View? _footer;

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearLayoutManager"/> class.
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		public LinearLayoutManager(bool isHorizontal) : this(isHorizontal, ItemSizingStrategy.MeasureFirstItem) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearLayoutManager"/> class.
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="sizingStrategy">Item size measuring strategy</param>
		public LinearLayoutManager(bool isHorizontal, ItemSizingStrategy sizingStrategy) : this(isHorizontal, sizingStrategy, 0) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="LinearLayoutManager"/> class.
		/// </summary>
		/// <param name="isHorizontal">Layout orientation</param>
		/// <param name="sizingStrategy">Item size measuring strategy</param>
		/// <param name="itemSpacing">A space size between items</param>
		public LinearLayoutManager(bool isHorizontal, ItemSizingStrategy sizingStrategy, int itemSpacing)
		{
			IsHorizontal = isHorizontal;
			_hasUnevenRows = sizingStrategy == ItemSizingStrategy.MeasureAllItems;
			ItemSpacing = itemSpacing;
		}

		/// <summary>
		/// Whether the item is a layout horizontally 
		/// </summary>
		public bool IsHorizontal { get; }

		/// <summary>
		/// A space size between items
		/// </summary>
		public double ItemSpacing { get; }

		/// <summary>
		/// CollectionView that interact with layout manager
		/// </summary>
		public ICollectionViewController? Controller { get; set; }

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

					var itembound = Controller!.GetItemSize(ItemWidthConstraint, ItemHeightConstraint);
					_baseItemBound = itembound;
				}

				return _baseItemBound;
			}
		}

		double ItemWidthConstraint => IsHorizontal ? double.PositiveInfinity : _allocatedSize.Width;

		double ItemHeightConstraint => IsHorizontal ? _allocatedSize.Height : double.PositiveInfinity;

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

		Size _scrollCanvasSize;

		public Size GetScrollCanvasSize()
		{
			if (Controller!.Count == 0 || _allocatedSize.Width <= 0 || _allocatedSize.Height <= 0)
			{
				return _allocatedSize;
			}

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
				totalItemSize = (BaseItemSize + ItemSpacing) * Controller!.Count - ItemSpacing + ItemStartPoint + FooterSizeWithSpacing;
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

			int startIndex = Math.Max(GetStartIndex(bound) - 5, 0);
			int endIndex = Math.Min(GetEndIndex(bound) + 5, Controller!.Count - 1);

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
				View itemView;

				if (!_realizedItem.ContainsKey(i))
				{
					var view = Controller!.RealizeView(i);
					_realizedItem[i] = new RealizedItem(view, i);
					itemView = view;
				}
				else
				{
					itemView = _realizedItem[i].Holder;
				}

				var itemBound = GetItemBound(i);
				itemView.UpdateBounds(itemBound);
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

			_scrollCanvasSize = new Size(0, 0);
			Controller!.ContentSizeUpdated();
		}

		public void ItemRemoved(int removed)
		{
			if (_realizedItem.ContainsKey(removed))
			{
				Controller!.UnrealizeView(_realizedItem[removed].Holder);
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
			Controller!.ContentSizeUpdated();
		}

		public void ItemUpdated(int index)
		{
			if (_realizedItem.ContainsKey(index))
			{
				var bound = _realizedItem[index].Holder.GetBounds();
				Controller!.UnrealizeView(_realizedItem[index].Holder);
				var view = Controller!.RealizeView(index);
				_realizedItem[index].Holder = view;
				view.UpdateBounds(bound);
			}
		}

		public Rect GetItemBound(int index)
		{
			double itemSize = 0;
			double startPoint = 0;

			if (!_hasUnevenRows)
			{
				itemSize = BaseItemSize;
				startPoint = ItemStartPoint + (itemSize + ItemSpacing) * index;
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
				var measured = Controller!.GetItemSize(index, ItemWidthConstraint, ItemHeightConstraint);
				itemSize = IsHorizontal ? measured.Width : measured.Height;

				if (itemSize != _itemSizes[index])
				{
					UpdateAccumulatedItemSize(index, itemSize - _itemSizes[index]);
					_itemSizes[index] = itemSize;

					Controller!.ContentSizeUpdated();
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
				Controller!.UnrealizeView(realizedItem.Holder);
			}

			_realizedItem.Clear();
			_scrollCanvasSize = new Size(0, 0);
			Controller!.ContentSizeUpdated();
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
					Controller!.RequestLayoutItems();
				}
			}
			else if (index == 0)
			{
				// Reset item size to measure updated size
				InitializeMeasureCache();
				Controller!.RequestLayoutItems();
			}
		}

		public int GetVisibleItemIndex(double x, double y)
		{
			double coordinate = IsHorizontal ? x : y;
			double canvasSize = IsHorizontal ? _scrollCanvasSize.Width : _scrollCanvasSize.Height;

			if (coordinate < 0)
				return 0;

			if (canvasSize < coordinate)
				return Controller!.Count - 1;

			if (!_hasUnevenRows)
			{
				return Math.Min(Math.Max(0, (int)((coordinate - ItemStartPoint) / (BaseItemSize + ItemSpacing))), Controller!.Count - 1);
			}
			else
			{
				var index = _accumulatedItemSizes.FindIndex(current => coordinate <= current);

				if (index == -1)
					index = Controller!.Count - 1;

				return index;
			}
		}

		public double GetScrollBlockSize()
		{
			return BaseItemSize + ItemSpacing;
		}

		public double GetScrollColumnSize()
		{
			return (IsHorizontal ? BaseItemBound.Height : BaseItemBound.Width);
		}

		public void SetHeader(View? header, Size size)
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
				Controller!.ContentSizeUpdated();
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

		public void SetFooter(View? footer, Size size)
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
				Controller!.ContentSizeUpdated();
			}

			UpdateFooterPosition();
		}

		public int NextRowItemIndex(int index)
		{
			return Math.Min(index + 1, Controller!.Count - 1);
		}

		public int PreviousRowItemIndex(int index)
		{
			return Math.Max(index - 1, 0);
		}

		void UpdateFooterPosition()
		{
			if (_footer == null)
				return;

			var point = new Point();

			if (IsHorizontal)
			{
				point.X += (GetScrollCanvasSize().Width - _footerSize.Width);
			}
			else
			{
				point.Y += (GetScrollCanvasSize().Height - _footerSize.Height);
			}

			var bound = new Rect(point, _footerSize);

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
				_accumulatedItemSizes.Add((i > 0 ? (_accumulatedItemSizes[i - 1] + ItemSpacing) : ItemStartPoint) + _itemSizes[i]);
			}

			Controller!.ContentSizeUpdated();
		}

		int GetStartIndex(Rect bound, double itemSize)
		{
			return (int)((ViewPortStartPoint(bound) - ItemStartPoint) / itemSize);
		}

		int GetStartIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetStartIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortStartPoint(bound));
		}

		int GetEndIndex(Rect bound, double itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize) - 1;
		}

		int GetEndIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetEndIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound));
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
			_accumulatedItemSizes[inserted] = inserted > 0 ? _accumulatedItemSizes[inserted - 1] : ItemStartPoint;
			UpdateAccumulatedItemSize(inserted, BaseItemSize);
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

	}

}