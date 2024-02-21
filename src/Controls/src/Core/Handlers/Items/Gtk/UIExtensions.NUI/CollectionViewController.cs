using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Microsoft.Maui.Layouts;

namespace Gtk.UIExtensions.NUI;

public class CollectionViewController : ICollectionViewController
{

	bool _requestLayoutItems = false;

	public Size AllocatedSize { get; protected set; }

	Size _itemSize = new Size(-1, -1);

	SynchronizationContext _mainloopContext;
	ItemAdaptor? _adaptor;

	/// <summary>
	/// Gets or sets ItemAdaptor to adapt items source
	/// </summary>
	public ItemAdaptor? Adaptor
	{
		get => _adaptor;
		set
		{
			OnAdaptorChanging();
			_adaptor = value;
			OnAdaptorChanged();
		}
	}

	ICollectionViewLayoutManager? _layoutManager;

	/// <summary>
	/// Gets or sets LayoutManager to organize position of item
	/// </summary>
	public ICollectionViewLayoutManager? LayoutManager
	{
		get => _layoutManager;
		set
		{
			OnLayoutManagerChanging();
			_layoutManager = value;
			OnLayoutManagerChanged();
		}
	}

	RecyclerPool _pool = new RecyclerPool();
	Dictionary<ViewHolder, int> _viewHolderIndexTable = new Dictionary<ViewHolder, int>();
	HashSet<int> _selectedItems = new HashSet<int>();

	/// <summary>
	/// The size of the area that layout in advance before it is visible
	/// </summary>
	public float RedundancyLayoutBoundRatio { get; set; } = 2f;

	/// <summary>
	/// The number of items on CollectionView
	/// </summary>
	public int Count => Adaptor?.Count ?? 0;

	internal Rect ViewPort => GetViewPort?.Invoke() ?? Rect.Zero;

	/// <summary>
	/// Selected items
	/// </summary>
	public IEnumerable<int> SelectedItems { get => _selectedItems; }

	public CollectionViewController()
	{
		_mainloopContext = SynchronizationContext.Current ?? throw new InvalidOperationException("Must create on main thread");

	}

	/// <summary>
	/// Create a ViewHolder, override it to customzie a decoration of view
	/// </summary>
	/// <returns>A ViewHolder instance</returns>
	protected virtual ViewHolder CreateViewHolder()
	{
		return new ViewHolder();
	}

	public ViewHolder RealizeView(int index)
	{
		if (Adaptor == null)
			throw new InvalidOperationException("No Adaptor");

		if (LayoutManager == null)
			throw new InvalidOperationException("No LayoutManager");

		var holder = _pool.GetRecyclerView(Adaptor.GetViewCategory(index));

		if (holder != null)
		{
			holder.Visible = true;
		}
		else
		{
			var content = Adaptor.CreateNativeView(index);
			holder = CreateViewHolder();
			holder.RequestSelected += OnRequestItemSelected;
			holder.StateUpdated += OnItemStateUpdated;
			holder.Content = content;
			holder.ViewCategory = Adaptor.GetViewCategory(index);
			AddToContainer?.Invoke(holder);
		}

		Adaptor.SetBinding(holder.Content!, index);
		_viewHolderIndexTable[holder] = index;

		if (_selectedItems.Contains(index))
		{
			holder.UpdateSelected();
		}

		var bounds = LayoutManager.GetItemBound(index);

		if (Adaptor.GetTemplatedView(holder.Content!) is { } view)
		{
			var size = view.Arrange(new Rect(Point.Zero, bounds.Size));

			if (size != bounds.Size)
			{
				;
			}
		}

		holder.Bounds = bounds;

		return holder;
	}

	void OnRequestItemSelected(object? sender, EventArgs e)
	{
		if (sender == null)
			return;

		// Selection request from UI
		var viewHolder = (ViewHolder)sender;

		if (_viewHolderIndexTable.TryGetValue(viewHolder, out var index))
		{

			if (_selectedItems.Contains(index) && SelectionMode != Microsoft.Maui.Controls.SelectionMode.Single)
			{
				RequestItemUnselect(index, viewHolder);
			}
			else
			{
				RequestItemSelect(index, viewHolder);
			}
		}
	}

	void OnItemStateUpdated(object? sender, EventArgs e)
	{
		if (sender == null)
			return;

		ViewHolder holder = (ViewHolder)sender;

		// Hack, in NUI, equal was override and even though not null, if it has no Body , it treat as null
		if (holder.Content is { })
		{
			Adaptor?.UpdateViewState(holder.Content, holder.State);

			if (_viewHolderIndexTable.ContainsKey(holder) && holder.State == ViewHolderState.Focused)
			{
				var index = _viewHolderIndexTable[holder];
				OnScrollTo(index, ScrollToPosition.MakeVisible, true);
			}
		}
	}

	public void UnrealizeView(ViewHolder view)
	{
		if (Adaptor == null)
			throw new InvalidOperationException("No Adaptor");

		_viewHolderIndexTable.Remove(view);
		Adaptor.UnBinding(view.Content!);
		view.ResetState();
		view.Hide();

		if (_pool.Count < Math.Max(Count / 3, _viewHolderIndexTable.Count * 3))
		{
			_pool.AddRecyclerView(view);
		}
		else
		{
			var content = view.Content;

			if (content != null)
				Adaptor.RemoveNativeView(content);

			view.Content = null;
			RemoveFromContainer?.Invoke(view);
			view.Dispose();
		}
	}

	public void RequestLayoutItems()
	{
		if (AllocatedSize.Width <= 0 || AllocatedSize.Height <= 0)
			return;

		if (!_requestLayoutItems)
		{
			_requestLayoutItems = true;

			_mainloopContext.Post((s) =>
			{
				_requestLayoutItems = false;

				if (Adaptor != null && LayoutManager != null)
				{
					ContentSizeUpdated();
					LayoutManager.LayoutItems(ExtendViewPort(ViewPort), true);
				}
			}, null);
		}
	}

	public Rect ExtendViewPort(Rect viewport)
	{
		if (LayoutManager == null)
			return viewport;

		if (LayoutManager.IsHorizontal)
		{
			viewport.X = Math.Max(0, viewport.X - viewport.Width * RedundancyLayoutBoundRatio / 2f);
			viewport.Width += viewport.Width * RedundancyLayoutBoundRatio;
		}
		else
		{
			viewport.Y = Math.Max(0, viewport.Y - viewport.Height * RedundancyLayoutBoundRatio / 2f);
			viewport.Height += viewport.Height * RedundancyLayoutBoundRatio;
		}

		return viewport;
	}

	public Size GetItemSize()
	{
		var widthConstraint = LayoutManager!.IsHorizontal ? double.PositiveInfinity : AllocatedSize.Width;
		var heightConstraint = LayoutManager!.IsHorizontal ? AllocatedSize.Height : double.PositiveInfinity;

		return GetItemSize(widthConstraint, heightConstraint);
	}

	public Size GetItemSize(double widthConstraint, double heightConstraint)
	{
		if (Adaptor == null || Adaptor.Count == 0)
		{
			return new Size(0, 0);
		}

		if (_itemSize.Width > 0 && _itemSize.Height > 0)
		{
			return _itemSize;
		}

		_itemSize = Adaptor.MeasureItem(widthConstraint, heightConstraint);
		_itemSize.Width = Math.Max(_itemSize.Width, 10);
		_itemSize.Height = Math.Max(_itemSize.Height, 10);

		return _itemSize;
	}

	public Size GetItemSize(int index, double widthConstraint, double heightConstraint)
	{
		if (Adaptor == null)
		{
			return new Size(0, 0);
		}

		return Adaptor.MeasureItem(index, widthConstraint, heightConstraint);

	}

	public void ContentSizeUpdated()
	{
		var size = LayoutManager?.GetScrollCanvasSize() ?? AllocatedSize;

		OnContentSizeUpdated(size);
	}

	public void OnLayout(object? sender, SizeAllocatedArgs e)
	{
		//called when resized
		AllocatedSize = e.Allocation.ToRect().Size;
		_itemSize = new Size(-1, -1);

		if (Adaptor != null && LayoutManager != null)
		{
			LayoutManager.SizeAllocated(AllocatedSize);
			OnUpdateHeaderFooter();
			ContentSizeUpdated();
			LayoutManager.LayoutItems(ExtendViewPort(ViewPort));
		}
	}

	public void ItemMeasureInvalidated(int index)
	{
		if (index == -1)
		{
			OnUpdateHeaderFooter();
			RequestLayoutItems();

			return;
		}

		// If a first item size was updated, need to reset _itemSize
		if (index == 0)
		{
			_itemSize = new Size(-1, -1);
		}

		LayoutManager?.ItemMeasureInvalidated(index);
	}

	Microsoft.Maui.Controls.SelectionMode _selectionMode;

	/// <summary>
	/// Gets or sets a value that controls whether and how many items can be selected.
	/// </summary>
	public Microsoft.Maui.Controls.SelectionMode SelectionMode
	{
		get => _selectionMode;
		set
		{
			_selectionMode = value;
			UpdateSelectionMode();
		}
	}

	public void RequestItemSelect(int index)
	{
		RequestItemSelect(index, default);
	}

	void RequestItemSelect(int index, ViewHolder? viewHolder = null)
	{
		if (SelectionMode == Microsoft.Maui.Controls.SelectionMode.None)
			return;

		if (SelectionMode != Microsoft.Maui.Controls.SelectionMode.Multiple && _selectedItems.Any())
		{
			var selected = _selectedItems.First();

			if (selected == index)
			{
				// already selected
				if (SelectionMode == Microsoft.Maui.Controls.SelectionMode.Single)
					return;
			}
			else
			{
				// clear previous selection item
				var prevSelected = FindViewHolder(_selectedItems.First());
				prevSelected?.ResetState();
				_selectedItems.Clear();
			}
		}

		_selectedItems.Add(index);

		if (viewHolder != null)
		{
			viewHolder.UpdateSelected();
		}
		else
		{
			FindViewHolder(index)?.UpdateSelected();
		}

		Adaptor?.SendItemSelected(_selectedItems);
	}

	ViewHolder? FindViewHolder(int index)
	{
		return _viewHolderIndexTable.Where(d => d.Value == index).Select(d => d.Key).FirstOrDefault();
	}

	public void RequestItemUnselect(int index, ViewHolder? viewHolder = null)
	{
		if (SelectionMode == Microsoft.Maui.Controls.SelectionMode.None)
			return;

		if (_selectedItems.Contains(index))
		{
			if (viewHolder == null)
			{
				viewHolder = FindViewHolder(index);
			}

			_selectedItems.Remove(index);
			viewHolder?.ResetState();
		}

		Adaptor?.SendItemSelected(_selectedItems);
	}

	void UpdateSelectionMode()
	{
		if (_selectionMode == Microsoft.Maui.Controls.SelectionMode.None)
		{
			if (_selectedItems.Count > 0)
			{
				foreach (var item in _viewHolderIndexTable)
				{
					if (_selectedItems.Contains(item.Value))
					{
						item.Key.ResetState();
					}
				}
			}

			_selectedItems.Clear();
		}
		else if (_selectionMode == Microsoft.Maui.Controls.SelectionMode.Single)
		{
			if (_selectedItems.Count > 1)
			{
				var first = _selectedItems.First();

				foreach (var item in _viewHolderIndexTable)
				{
					if (_selectedItems.Contains(item.Value) && first != item.Value)
					{
						item.Key.ResetState();
					}
				}

				_selectedItems.Clear();
				_selectedItems.Add(first);
			}
		}

		Adaptor?.SendItemSelected(_selectedItems);
	}

	#region Extra

	public event EventHandler? UpdateHeaderFooter;

	void OnUpdateHeaderFooter()
	{
		UpdateHeaderFooter?.Invoke(this, new());

	}

	public event EventHandler? LayoutManagerChanged;

	void OnLayoutManagerChanged()
	{
		if (_layoutManager == null)
			return;

		_itemSize = new Size(-1, -1);
		_layoutManager.CollectionView = this;

		LayoutManagerChanged?.Invoke(this, new());
		_layoutManager.SizeAllocated(AllocatedSize);
		OnUpdateHeaderFooter();
		RequestLayoutItems();

	}

	public event EventHandler? LayoutManagerChanging;

	void OnLayoutManagerChanging()
	{
		LayoutManagerChanging?.Invoke(this, new());
		_layoutManager?.Reset();
	}

	public event EventHandler? AdaptorChanged;

	void OnAdaptorChanged()
	{
		if (Adaptor == null)
			return;

		_itemSize = new Size(-1, -1);
		Adaptor.CollectionView = this;
		(Adaptor as INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;

		LayoutManager?.ItemSourceUpdated();
		RequestLayoutItems();

		AdaptorChanged?.Invoke(this, new());
	}

	public event EventHandler? AdaptorChanging;

	void OnAdaptorChanging()
	{
		AdaptorChanging?.Invoke(this, new());

		LayoutManager?.Reset();

		if (Adaptor != null)
		{
			_pool.Clear(Adaptor);
			(Adaptor as INotifyCollectionChanged).CollectionChanged -= OnCollectionChanged;
			Adaptor.CollectionView = null;
		}

		_selectedItems.Clear();
	}

	void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (sender != Adaptor)
		{
			return;
		}

		if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
		{
			int idx = e.NewStartingIndex;

			if (idx == -1)
			{
				idx = Adaptor!.Count - e.NewItems.Count;
			}

			foreach (var item in e.NewItems)
			{
				foreach (var viewHolder in _viewHolderIndexTable.Keys.ToList())
				{
					if (_viewHolderIndexTable[viewHolder] >= idx)
					{
						_viewHolderIndexTable[viewHolder]++;
					}
				}

				var updated = new HashSet<int>();

				foreach (var selected in _selectedItems)
				{
					if (selected >= idx)
					{
						updated.Add(selected + 1);
					}
				}

				_selectedItems = updated;
				LayoutManager?.ItemInserted(idx++);
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
		{
			int idx = e.OldStartingIndex;

			// Can't tracking remove if there is no data of old index
			if (idx == -1)
			{
				LayoutManager?.ItemSourceUpdated();
			}
			else
			{
				foreach (var item in e.OldItems)
				{
					LayoutManager?.ItemRemoved(idx);

					foreach (var viewHolder in _viewHolderIndexTable.Keys.ToList())
					{
						if (_viewHolderIndexTable[viewHolder] > idx)
						{
							_viewHolderIndexTable[viewHolder]--;
						}
					}

					if (_selectedItems.Contains(idx))
					{
						_selectedItems.Remove(idx);
					}

					var updated = new HashSet<int>();

					foreach (var selected in _selectedItems)
					{
						if (selected > idx)
						{
							updated.Add(selected - 1);
						}
					}

					_selectedItems = updated;
				}
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Move)
		{
			LayoutManager?.ItemRemoved(e.OldStartingIndex);
			LayoutManager?.ItemInserted(e.NewStartingIndex);
		}
		else if (e.Action == NotifyCollectionChangedAction.Replace && e.OldItems != null)
		{
			// Can't tracking if there is no information old data
			if (e.OldItems.Count > 1 || e.NewStartingIndex == -1)
			{
				LayoutManager?.ItemSourceUpdated();
			}
			else
			{
				LayoutManager?.ItemUpdated(e.NewStartingIndex);
			}
		}
		else if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			LayoutManager?.Reset();
			LayoutManager?.ItemSourceUpdated();
		}

		RequestLayoutItems();
	}

	public event EventHandler<Size>? HasContentSizeUpdated;

	void OnContentSizeUpdated(Size size)
	{
		HasContentSizeUpdated?.Invoke(this, size);
	}

	public Func<Rect>? GetViewPort { get; set; }

	public Action<ViewHolder>? AddToContainer { get; set; }

	public Action<ViewHolder>? RemoveFromContainer { get; set; }

	public Action<(int index, ScrollToPosition position, bool animate)>? ScrollTo { get; set; }

	void OnScrollTo(int index, ScrollToPosition position, bool animate)
	{
		ScrollTo?.Invoke((index, position, animate));
	}

	#endregion

}