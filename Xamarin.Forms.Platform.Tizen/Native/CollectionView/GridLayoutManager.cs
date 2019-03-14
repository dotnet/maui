using System;
using System.Collections.Generic;
using System.Linq;
using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class GridLayoutManager : ICollectionViewLayoutManager
	{
		ESize _allocatedSize;
		ESize _scrollCanvasSize;
		bool _isLayouting;
		Rect _last;
		Dictionary<int, RealizedItem> _realizedItem = new Dictionary<int, RealizedItem>();

		public GridLayoutManager(bool isHorizontal, int span = 1)
		{
			IsHorizontal = isHorizontal;
			Span = span;
		}

		public int Span { get; internal set; }

		public bool IsHorizontal { get; }

		public ICollectionViewController CollectionView { get; set; }

		public void SizeAllocated(ESize size)
		{
			Reset();
			_allocatedSize = size;
			_scrollCanvasSize = new ESize(0, 0);
		}

		public ESize GetScrollCanvasSize()
		{
			if (_scrollCanvasSize.Width > 0 && _scrollCanvasSize.Height > 0)
				return _scrollCanvasSize;

			var itemCount = CollectionView.Count;
			var itemSize = CollectionView.GetItemSize();
			if (IsHorizontal)
			{
				return _scrollCanvasSize = new ESize((int)Math.Ceiling(itemCount / (double)Span) * itemSize.Width , _allocatedSize.Height);
			}
			else
			{
				return _scrollCanvasSize = new ESize(_allocatedSize.Width, (int)Math.Ceiling(itemCount / (double)Span) * itemSize.Height);
			}
		}

		bool ShouldRearrange(Rect viewport)
		{
			if (_isLayouting)
				return false;
			if (_last.Size != viewport.Size)
				return true;

			var diff = IsHorizontal ? Math.Abs(_last.X - viewport.X) : Math.Abs(_last.Y - viewport.Y);
			var margin = IsHorizontal ? CollectionView.GetItemSize().Width : CollectionView.GetItemSize().Height;
			if (diff > margin)
				return true;

			return false;
		}

		public void LayoutItems(Rect bound, bool force)
		{
			// TODO : need to optimization. it was frequently called with similar bound value.
			if (!ShouldRearrange(bound) && !force)
			{
				return;
			}
			_isLayouting = true;
			_last = bound;

			var size = CollectionView.GetItemSize();
			var itemSize = IsHorizontal ? size.Width : size.Height;

			int padding = Span * 2;
			int startIndex = Math.Max(GetStartIndex(bound, itemSize) - padding, 0);
			int endIndex = Math.Min(GetEndIndex(bound, itemSize) + padding, CollectionView.Count - 1);

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
			_scrollCanvasSize = new ESize(0, 0);
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
			var size = CollectionView.GetItemSize();
			if (IsHorizontal)
			{
				size.Height = _allocatedSize.Height / Span;
			}
			else
			{
				size.Width = _allocatedSize.Width / Span;
			}

			int rowIndex = index / Span;
			int colIndex = index % Span;
			var colSize = IsHorizontal ? size.Height : size.Width;

			return
				IsHorizontal ?
				new Rect(rowIndex * size.Width, colIndex * size.Height, size.Width, size.Height) :
				new Rect(colIndex * size.Width, rowIndex * size.Height, size.Width, size.Height);
		}

		public void Reset()
		{
			foreach (var realizedItem in _realizedItem.Values)
			{
				CollectionView.UnrealizeView(realizedItem.View);
			}
			_realizedItem.Clear();
			_scrollCanvasSize = new ESize(0, 0);
		}

		int GetStartIndex(Rect bound, int itemSize)
		{
			return ViewPortStartPoint(bound) / itemSize * Span;
		}

		int GetEndIndex(Rect bound, int itemSize)
		{
			return (int)Math.Ceiling(ViewPortEndPoint(bound) / (double)itemSize) * Span;
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

		class RealizedItem
		{
			public ViewHolder View { get; set; }
			public int Index { get; set; }
		}
	}
}
