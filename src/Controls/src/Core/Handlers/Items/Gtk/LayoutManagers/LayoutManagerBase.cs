using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Gtk.UIExtensions.NUI;

public abstract class LayoutManagerBase : ICollectionViewLayoutManager
{

	protected Size _allocatedSize;
	protected Size _scrollCanvasSize;
	protected bool _isLayouting;
	protected Rect _lastLayoutedBound;
	protected Dictionary<int, RealizedItem> _realizedItem = new();
	protected List<double> _itemSizes = new();
	protected List<bool> _cached = new();
	protected List<double> _accumulatedItemSizes = new();
	protected bool _hasUnevenRows;
	protected Size _baseItemBound;
	protected Size _headerSize;
	protected Widget? _header;
	protected Size _footerSize;
	protected Widget? _footer;

	/// <summary>
	/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
	/// </summary>
	/// <param name="isHorizontal">Layout orientation</param>
	public LayoutManagerBase(bool isHorizontal) : this(isHorizontal, ItemSizingStrategy.MeasureFirstItem) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="GridLayoutManager"/> class
	/// </summary>
	/// <param name="isHorizontal">Layout orientation</param>
	/// <param name="sizingStrategy">Item size measuring strategy</param>
	public LayoutManagerBase(bool isHorizontal, ItemSizingStrategy sizingStrategy)
	{
		IsHorizontal = isHorizontal;
		_hasUnevenRows = sizingStrategy == ItemSizingStrategy.MeasureAllItems;
	}

	/// <summary>
	/// Whether the item is a layout horizontally
	/// </summary>
	public bool IsHorizontal { get; }

	/// <summary>
	/// CollectionView that interact with layout manager
	/// </summary>
	public ICollectionViewController? Controller { get; set; }

	protected double BaseItemSize
	{
		get => IsHorizontal ? BaseItemBound.Width : BaseItemBound.Height;
	}

	protected double BaseColumnSize
	{
		get => IsHorizontal ? BaseItemBound.Height : BaseItemBound.Width;
	}

	protected abstract Size BaseItemBound { get; }

	protected double FooterSize => IsHorizontal ? _footerSize.Width : _footerSize.Height;

	protected double HeaderSize => IsHorizontal ? _headerSize.Width : _headerSize.Height;

	protected double ItemStartPoint
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

	protected double FooterSizeWithSpacing
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

	public abstract Size GetScrollCanvasSize();

	public double GetScrollBlockSize()
	{
		return BaseItemSize + ItemSpacing;
	}

	public abstract double GetScrollColumnSize();

	public abstract void LayoutItems(Rect bound, bool force);

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
			var bound = _realizedItem[index].Holder.Bounds;
			Controller!.UnrealizeView(_realizedItem[index].Holder);
			var view = Controller!.RealizeView(index);
			_realizedItem[index].Holder = view;
			view.UpdateBounds(bound);
		}
	}

	public abstract Rect GetItemBound(int index);

	public void Reset()
	{
		foreach (var realizedItem in _realizedItem.Values.ToList())
		{
			Controller!.UnrealizeView(realizedItem.Holder);
		}

		_realizedItem.Clear();
		_scrollCanvasSize = new Size(0, 0);
		// Controller!.ContentSizeUpdated();
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
		else if (index == 0) // MeasureFirstItem
		{
			// Reset item size to measure updated size
			InitializeMeasureCache();
			Controller!.RequestLayoutItems();
		}
	}

	public abstract int GetVisibleItemIndex(double x, double y);

	public void SetHeader(Widget? header, Size size)
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

	public void SetFooter(Widget? footer, Size size)
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

	public abstract double ItemSpacing { get; }

	protected void UpdateFooterPosition()
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

	protected abstract void InitializeMeasureCache();

	protected abstract void BuildAccumulatedSize();

	protected void UpdateInsertedSize(int inserted)
	{
		if (!_hasUnevenRows)
			return;

		_cached.Insert(inserted, false);
		_itemSizes.Insert(inserted, BaseItemSize);

		BuildAccumulatedSize();
	}

	protected void UpdateRemovedSize(int removed)
	{
		if (!_hasUnevenRows)
			return;

		_itemSizes.RemoveAt(removed);

		_cached.RemoveAt(removed);
		BuildAccumulatedSize();
	}

	protected void UpdateAccumulatedItemSize(int index, double diff)
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

	protected abstract int GetStartIndex(Rect bound, double itemSize);

	protected abstract int GetStartIndex(Rect bound);

	protected abstract int GetEndIndex(Rect bound, double itemSize);

	protected abstract int GetEndIndex(Rect bound);

	public abstract int NextRowItemIndex(int index);

	public abstract int PreviousRowItemIndex(int index);

	protected double ViewPortStartPoint(Rect viewPort)
	{
		return IsHorizontal ? viewPort.X : viewPort.Y;
	}

	protected double ViewPortEndPoint(Rect viewPort)
	{
		return ViewPortStartPoint(viewPort) + ViewPortSize(viewPort);
	}

	protected double ViewPortSize(Rect viewPort)
	{
		return IsHorizontal ? viewPort.Width : viewPort.Height;
	}

	protected bool ShouldRearrange(Rect viewport)
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

	protected static int FindFirstGreaterOrEqualTo(IList<double> data, double value)
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