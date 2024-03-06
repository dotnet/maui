using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Gtk.UIExtensions.NUI
{

	/// <summary>
	/// A <see cref="ICollectionViewLayoutManager"/> implementation which provides linear layout
	/// </summary>
	public class LinearLayoutManager : LayoutManagerBase
	{

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
		public LinearLayoutManager(bool isHorizontal, ItemSizingStrategy sizingStrategy, int itemSpacing) : base(isHorizontal, sizingStrategy)
		{
			ItemSpacing = itemSpacing;
		}

		/// <summary>
		/// A space size between items
		/// </summary>
		public override double ItemSpacing { get; }

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

		double ColumnSize => (IsHorizontal ? _allocatedSize.Height  : _allocatedSize.Width );
		
		double ItemWidthConstraint => IsHorizontal ? double.PositiveInfinity : _allocatedSize.Width;

		double ItemHeightConstraint => IsHorizontal ? _allocatedSize.Height : double.PositiveInfinity;

		public override Size GetScrollCanvasSize()
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

				totalItemSize = _accumulatedItemSizes[^1] + FooterSizeWithSpacing;
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
				Widget itemView;

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

		public override Rect GetItemBound(int index)
		{
			double itemSize = 0;
			double startPoint = 0;

			double columnSize = ColumnSize;

			if (double.IsInfinity(columnSize))
				columnSize = BaseColumnSize;
			
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
				new Rect(startPoint, 0, itemSize, columnSize) :
				new Rect(0, startPoint, columnSize, itemSize);
		}

		public override int GetVisibleItemIndex(double x, double y)
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

		public override double GetScrollColumnSize()
		{
			return (IsHorizontal ? BaseItemBound.Height : BaseItemBound.Width);
		}

		public override int NextRowItemIndex(int index)
		{
			return Math.Min(index + 1, Controller!.Count - 1);
		}

		public override int PreviousRowItemIndex(int index)
		{
			return Math.Max(index - 1, 0);
		}

		protected override void InitializeMeasureCache()
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

		protected override int GetStartIndex(Rect bound, double itemSize)
		{
			return (int)((ViewPortStartPoint(bound) - ItemStartPoint) / itemSize);
		}

		protected override int GetStartIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetStartIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortStartPoint(bound));
		}

		protected override int GetEndIndex(Rect bound, double itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize) - 1;
		}

		protected override int GetEndIndex(Rect bound)
		{
			if (!_hasUnevenRows)
			{
				return GetEndIndex(bound, BaseItemSize + ItemSpacing);
			}

			return FindFirstGreaterOrEqualTo(_accumulatedItemSizes, ViewPortEndPoint(bound));
		}

		protected override void BuildAccumulatedSize()
		{
			_accumulatedItemSizes = new List<double>();
			int n = _itemSizes.Count;

			for (int i = 0; i < n; i++)
			{
				_accumulatedItemSizes.Add((i > 0 ? (_accumulatedItemSizes[i - 1] + ItemSpacing) : ItemStartPoint) + _itemSizes[i]);
			}
		}

	}

}