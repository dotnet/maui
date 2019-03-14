using System;
using System.Collections.Specialized;
using ElmSharp;
using EBox = ElmSharp.Box;
using EScroller = ElmSharp.Scroller;
using ESize = ElmSharp.Size;
using EPoint = ElmSharp.Point;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class CollectionView : EBox, ICollectionViewController
	{
		RecyclerPool _pool = new RecyclerPool();
		ICollectionViewLayoutManager _layoutManager;
		ItemAdaptor _adaptor;
		EBox _innerLayout;

		Dictionary<ViewHolder, int> _viewHolderIndexTable = new Dictionary<ViewHolder, int>();
		ViewHolder _lastSelectedViewHolder;
		int _selectedItemIndex = -1;
		CollectionViewSelectionMode _selectionMode = CollectionViewSelectionMode.None;

		bool _requestLayoutItems = false;
		SnapPointsType _snapPoints;
		ESize _itemSize = new ESize(-1, -1);

		public CollectionView(EvasObject parent) : base(parent)
		{
			SetLayoutCallback(OnLayout);
			Scroller = CreateScroller(parent);
			Scroller.Show();
			PackEnd(Scroller);
			Scroller.Scrolled += OnScrolled;

			_innerLayout = new EBox(parent);
			_innerLayout.SetLayoutCallback(OnInnerLayout);
			_innerLayout.Show();
			Scroller.SetContent(_innerLayout);
		}

		public CollectionViewSelectionMode SelectionMode
		{
			get => _selectionMode;
			set
			{
				_selectionMode = value;
				UpdateSelectionMode();
			}
		}

		public int SelectedItemIndex
		{
			get => _selectedItemIndex;
			set
			{
				if (_selectedItemIndex != value)
				{
					_selectedItemIndex = value;
					UpdateSelectedItemIndex();
				}
			}
		}

		public SnapPointsType SnapPointsType
		{
			get => _snapPoints;
			set
			{
				_snapPoints = value;
				UpdateSnapPointsType(_snapPoints);
			}
		}

		protected EScroller Scroller { get; }

		public ICollectionViewLayoutManager LayoutManager
		{
			get => _layoutManager;
			set
			{
				OnLayoutManagerChanging();
				_layoutManager = value;
				OnLayoutManagerChanged();
			}
		}

		public ItemAdaptor Adaptor
		{
			get => _adaptor;
			set
			{
				OnAdaptorChanging();
				_adaptor = value;
				_adaptor.CollectionView = this;
				OnAdaptorChanged();
			}
		}

		int ICollectionViewController.Count => Adaptor?.Count ?? 0;

		EPoint ICollectionViewController.ParentPosition => new EPoint
		{
			X = Scroller.Geometry.X - Scroller.CurrentRegion.X,
			Y = Scroller.Geometry.Y - Scroller.CurrentRegion.Y
		};

		ESize AllocatedSize { get; set; }

		Rect ViewPort => Scroller.CurrentRegion;

		public void ScrollTo(int index, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			var itemBound = LayoutManager.GetItemBound(index);
			int itemStart;
			int itemEnd;
			int scrollStart;
			int scrollEnd;
			int itemPadding = 0;
			int itemSize;
			int viewportSize;

			if (LayoutManager.IsHorizontal)
			{
				itemStart = itemBound.Left;
				itemEnd = itemBound.Right;
				itemSize = itemBound.Width;
				scrollStart = Scroller.CurrentRegion.Left;
				scrollEnd = Scroller.CurrentRegion.Right;
				viewportSize = AllocatedSize.Width;
			}
			else
			{
				itemStart = itemBound.Top;
				itemEnd = itemBound.Bottom;
				itemSize = itemBound.Height;
				scrollStart = Scroller.CurrentRegion.Top;
				scrollEnd = Scroller.CurrentRegion.Bottom;
				viewportSize = AllocatedSize.Height;
			}

			if (position == ScrollToPosition.MakeVisible)
			{
				if (itemStart < scrollStart)
				{
					position = ScrollToPosition.Start;
				}
				else if (itemEnd > scrollEnd)
				{
					position = ScrollToPosition.End;
				}
				else
				{
					// already visible
					return;
				}
			}

			if (itemSize < viewportSize)
			{
				switch (position)
				{
					case ScrollToPosition.Center:
						itemPadding = (viewportSize - itemSize) / 2;
						break;
					case ScrollToPosition.End:
						itemPadding = (viewportSize - itemSize);
						break;
				}
				itemSize = viewportSize;
			}

			if (LayoutManager.IsHorizontal)
			{
				itemBound.X -= itemPadding;
				itemBound.Width = itemSize;
			}
			else
			{
				itemBound.Y -= itemPadding;
				itemBound.Height = itemSize;
			}

			Scroller.ScrollTo(itemBound, animate);
		}

		public void ScrollTo(object item, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			ScrollTo(Adaptor.GetItemIndex(item), position, animate);
		}

		void ICollectionViewController.RequestLayoutItems() => RequestLayoutItems();


		ESize ICollectionViewController.GetItemSize()
		{
			if (Adaptor == null)
			{
				return new ESize(0, 0);
			}
			if (_itemSize.Width > 0 && _itemSize.Height > 0)
			{
				return _itemSize;
			}

			_itemSize = Adaptor.MeasureItem(AllocatedSize.Width, AllocatedSize.Height);
			_itemSize.Width = Math.Max(_itemSize.Width, 10);
			_itemSize.Height = Math.Max(_itemSize.Height, 10);

			if (_snapPoints != SnapPointsType.None)
			{
				Scroller.SetPageSize(_itemSize.Width, _itemSize.Height);
			}
			return _itemSize;
		}

		ViewHolder ICollectionViewController.RealizeView(int index)
		{
			if (Adaptor == null)
				return null;

			var holder = _pool.GetRecyclerView();
			if (holder != null)
			{
				holder.Show();
			}
			else
			{
				var content = Adaptor.CreateNativeView(this);
				holder = new ViewHolder(this);
				holder.RequestSelected += OnRequestItemSelection;
				holder.Content = content;
				_innerLayout.PackEnd(holder);
			}

			Adaptor.SetBinding(holder.Content, index);
			_viewHolderIndexTable[holder] = index;
			if (index == SelectedItemIndex)
			{
				OnRequestItemSelection(holder, EventArgs.Empty);
			}
			return holder;
		}

		void OnRequestItemSelection(object sender, EventArgs e)
		{
			if (SelectionMode == CollectionViewSelectionMode.None)
				return;


			if (_lastSelectedViewHolder != null)
			{
				_lastSelectedViewHolder.State = ViewHolderState.Normal;
			}

			_lastSelectedViewHolder = sender as ViewHolder;
			if (_lastSelectedViewHolder != null)
			{
				_lastSelectedViewHolder.State = ViewHolderState.Selected;
				if (_viewHolderIndexTable.TryGetValue(_lastSelectedViewHolder, out int index))
				{
					_selectedItemIndex = index;
					Adaptor?.SendItemSelected(index);
				}
			}
		}

		void ICollectionViewController.UnrealizeView(ViewHolder view)
		{
			_viewHolderIndexTable.Remove(view);
			view.ResetState();
			view.Hide();
			_pool.AddRecyclerView(view);
		}

		protected virtual EScroller CreateScroller(EvasObject parent)
		{
			return new EScroller(parent);
		}

		void UpdateSelectedItemIndex()
		{
			if (SelectionMode == CollectionViewSelectionMode.None)
				return;

			ViewHolder holder = null;
			foreach (var item in _viewHolderIndexTable)
			{
				if (item.Value == SelectedItemIndex)
				{
					holder = item.Key;
					break;
				}
			}
			OnRequestItemSelection(holder, EventArgs.Empty);
		}

		void UpdateSelectionMode()
		{
			if (SelectionMode == CollectionViewSelectionMode.None)
			{
				if (_lastSelectedViewHolder != null)
				{
					_lastSelectedViewHolder.State = ViewHolderState.Normal;
					_lastSelectedViewHolder = null;
				}
				_selectedItemIndex = -1;
			}
		}


		void OnLayoutManagerChanging()
		{
			_layoutManager?.Reset();
		}

		void OnLayoutManagerChanged()
		{
			if (_layoutManager == null)
				return;

			_layoutManager.CollectionView = this;
			_layoutManager.SizeAllocated(AllocatedSize);
			RequestLayoutItems();
		}

		void OnAdaptorChanging()
		{
			_layoutManager?.Reset();
			if (Adaptor != null)
			{
				_pool.Clear(Adaptor);
				(Adaptor as INotifyCollectionChanged).CollectionChanged -= OnCollectionChanged;
				Adaptor.CollectionView = null;
			}
		}
		void OnAdaptorChanged()
		{
			if (_adaptor == null)
				return;

			_itemSize = new ESize(-1, -1);
			(Adaptor as INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;

			RequestLayoutItems();

			if (LayoutManager != null)
			{
				var itemSize = (this as ICollectionViewController).GetItemSize();
			}
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Add)
			{
				int idx = e.NewStartingIndex;
				foreach (var item in e.NewItems)
				{
					LayoutManager.ItemInserted(idx++);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				int idx = e.OldStartingIndex;
				foreach (var item in e.OldItems)
				{
					LayoutManager.ItemRemoved(idx);
				}
			}
			else if (e.Action == NotifyCollectionChangedAction.Move)
			{
				LayoutManager.ItemRemoved(e.OldStartingIndex);
				LayoutManager.ItemInserted(e.NewStartingIndex);
			}
			else if (e.Action == NotifyCollectionChangedAction.Replace)
			{
				LayoutManager.ItemUpdated(e.NewStartingIndex);
			}
			else if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				LayoutManager.Reset();
			}
			RequestLayoutItems();
		}

		Rect _lastGeometry;
		void OnLayout()
		{
			if (_lastGeometry == Geometry)
			{
				return;
			}

			_lastGeometry = Geometry;
			Scroller.Geometry = Geometry;
			Scroller.ScrollBlock = ScrollBlock.None;
			AllocatedSize = Geometry.Size;
			_itemSize = new ESize(-1, -1);

			if (_adaptor != null && _layoutManager != null)
			{
				_layoutManager?.SizeAllocated(Geometry.Size);
				_layoutManager?.LayoutItems(ViewPort);
			}
		}

		void RequestLayoutItems()
		{
			if (!_requestLayoutItems)
			{
				_requestLayoutItems = true;
				Device.BeginInvokeOnMainThread(() =>
				{
					_requestLayoutItems = false;
					if (_adaptor != null && _layoutManager != null)
					{
						OnInnerLayout();
						_layoutManager?.LayoutItems(ViewPort, true);
					}
				});
			}
		}

		void OnInnerLayout()
		{
			var size = _layoutManager.GetScrollCanvasSize();
			_innerLayout.MinimumWidth = size.Width;
			_innerLayout.MinimumHeight = size.Height;
		}

		void OnScrolled(object sender, EventArgs e)
		{
			_layoutManager.LayoutItems(Scroller.CurrentRegion);
		}

		void UpdateSnapPointsType(SnapPointsType snapPoints)
		{
			var itemSize = new ESize(0, 0);
			switch (snapPoints)
			{
				case SnapPointsType.None:
					Scroller.HorizontalPageScrollLimit = 0;
					Scroller.VerticalPageScrollLimit = 0;
					break;
				case SnapPointsType.MandatorySingle:
					Scroller.HorizontalPageScrollLimit = 1;
					Scroller.VerticalPageScrollLimit = 1;
					itemSize = (this as ICollectionViewController).GetItemSize();
					break;
				case SnapPointsType.Mandatory:
					Scroller.HorizontalPageScrollLimit = 0;
					Scroller.VerticalPageScrollLimit = 0;
					itemSize = (this as ICollectionViewController).GetItemSize();
					break;
			}
			Scroller.SetPageSize(itemSize.Width, itemSize.Height);
		}
	}

	public interface ICollectionViewController
	{
		EPoint ParentPosition { get; }

		ViewHolder RealizeView(int index);

		void UnrealizeView(ViewHolder view);

		void RequestLayoutItems();

		int Count { get; }

		ESize GetItemSize();
	}

	public enum CollectionViewSelectionMode
	{
		None,
		Single,
	}

}
