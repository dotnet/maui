using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Handlers.Items2;
/// <summary>
/// An observable collection of <see cref="ItemTemplateContext2"/> that mirrors an
/// <see cref="INotifyCollectionChanged"/> items source, keeping the template collection
/// synchronized with the underlying data and supporting drag/drop reordering.
/// </summary>
internal class ObservableItemTemplateCollection2 : ObservableCollection<ItemTemplateContext2>
{
	readonly IList _itemsSource;
	readonly DataTemplate _itemTemplate;
	readonly BindableObject _container;
	readonly IMauiContext? _mauiContext;
	readonly double _itemHeight;
	readonly double _itemWidth;
	readonly Thickness _itemSpacing;
	readonly NotifyCollectionChangedEventHandler _collectionChanged;
	readonly WeakNotifyCollectionChangedProxy _proxy = new();

	bool _innerCollectionChange = false;
	bool _observeChanges = true;
	bool _isMoving = false;

	~ObservableItemTemplateCollection2() => _proxy.Unsubscribe();

	public ObservableItemTemplateCollection2(IList itemsSource, DataTemplate itemTemplate, BindableObject container,
		double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null, IMauiContext? mauiContext = null)
	{
		_itemsSource = itemsSource;
		_itemTemplate = itemTemplate;
		_container = container;
		_mauiContext = mauiContext;
		_itemHeight = itemHeight ?? 0;
		_itemWidth = itemWidth ?? 0;
		_itemSpacing = itemSpacing ?? default;
		_collectionChanged = InnerCollectionChanged;

		PopulateInitialItems(itemsSource, itemTemplate, container);
		SubscribeToSourceChanges(itemsSource);
	}

	/// <summary>
	/// Subscribes to collection change events on the items source and this collection.
	/// </summary>
	void SubscribeToSourceChanges(IList itemsSource)
	{
		if (itemsSource is INotifyCollectionChanged notifyCollectionChanged)
		{
			_proxy.Subscribe(notifyCollectionChanged, _collectionChanged);
		}

		CollectionChanged += TemplateCollectionChanged;
	}

	/// <summary>
	/// Unsubscribes from collection change events. Must be called when replacing
	/// the items source or when the handler disconnects.
	/// </summary>
	internal void CleanUp()
	{
		CollectionChanged -= TemplateCollectionChanged;
		_proxy.Unsubscribe();
	}

	/// <summary>
	/// Populates the collection with initial <see cref="ItemTemplateContext2"/> entries
	/// for each item in the source list.
	/// </summary>
	void PopulateInitialItems(IList itemsSource, DataTemplate itemTemplate, BindableObject container)
	{
		for (int index = 0; index < itemsSource.Count; index++)
		{
			var item = itemsSource[index];
			if (item is null)
				continue;

			Add(new ItemTemplateContext2(itemTemplate, item, container, _itemHeight, _itemWidth, _itemSpacing,
				false, false, _mauiContext));
		}
	}

	void TemplateCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		if (!_innerCollectionChange)
		{
			// When the template collection changes not as result of an inner collection change.
			// The only time this happens is during a drag/drop item reorder (CanReorderItems).
			// The ListView/GridView has notified us now we need to move those changes into the source.
			// One might think it would be a "Move" event but it is actually a "Remove" followed by "Add".
			_observeChanges = false;

			try
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						AddToSource(args);
						break;
					case NotifyCollectionChangedAction.Remove:
						RemoveFromSource(args);
						break;
					default:
						break;
				}
			}
			finally
			{
				_observeChanges = true;
			}
		}
	}

	void AddToSource(NotifyCollectionChangedEventArgs args)
	{
		if (args.NewItems is null)
		{
			return;
		}

		var firstItem = args.NewItems[0] as ItemTemplateContext2;
		var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : (firstItem is not null ? IndexOf(firstItem) : -1);

		var count = args.NewItems.Count;

		for (int index = 0; index < count; index++)
		{
			var newItem = args.NewItems[index] as ItemTemplateContext2;
			if (newItem is not null)
				_itemsSource.Insert(startIndex + index, newItem.Item);
		}
	}

	void RemoveFromSource(NotifyCollectionChangedEventArgs args)
	{
		if (args.OldItems is null)
		{
			return;
		}

		var startIndex = args.OldStartingIndex;

		if (startIndex < 0)
		{
			// INCC implementation isn't giving us enough information to know where the removed items were in the
			return;
		}

		var count = args.OldItems.Count;

		for (int index = startIndex + count - 1; index >= startIndex; index--)
		{
			_itemsSource.RemoveAt(index);
		}
	}

	void InnerCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		// Guard is re-evaluated on the UI thread (inside the lambda) rather than here.
		// Checking here only would create a race: if _observeChanges is true when this
		// fires on a background thread but is then set to false by MoveItemAndSyncSource
		// on the UI thread before the dispatched action runs, the stale handler would
		// still apply a double-update.
		_container.Dispatcher.DispatchIfRequired(() =>
		{
			if (!_observeChanges)
				return;

			InnerCollectionChanged(args);
		});
	}

	void InnerCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		_innerCollectionChange = true;
		try
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					Reset();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		finally
		{
			_innerCollectionChange = false;
		}
	}

	void Add(NotifyCollectionChangedEventArgs args)
	{
		if (args.NewItems is null)
		{
			return;
		}

		var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);

		var count = args.NewItems.Count;

		for (int index = 0; index < count; index++)
		{
			var item = args.NewItems[index];
			if (item is null)
				continue;

			Insert(startIndex + index, new ItemTemplateContext2(_itemTemplate, item, _container, _itemHeight, _itemWidth, _itemSpacing,
				false, false, _mauiContext));
		}
	}

	void Move(NotifyCollectionChangedEventArgs args)
	{
		if (args.NewItems is null)
		{
			return;
		}

		var count = args.NewItems.Count;
		if (args.OldStartingIndex > args.NewStartingIndex)
		{
			for (int n = 0; n < count; n++)
			{
				Move(args.OldStartingIndex + n, args.NewStartingIndex + n);
			}

			return;
		}

		for (int n = count - 1; n >= 0; n--)
		{
			Move(args.OldStartingIndex + n, args.NewStartingIndex + n);
		}
	}

	/// <summary>
	/// Atomically moves an item in the underlying source <em>and</em> the template collection
	/// without triggering feedback loops or creating new <see cref="ItemTemplateContext2"/> wrappers.
	/// <para>
	/// Using <see cref="System.Collections.IList.RemoveAt"/> + <see cref="System.Collections.IList.Insert"/>
	/// on the source would fire two separate <see cref="System.Collections.Specialized.NotifyCollectionChangedAction"/> events.
	/// <see cref="InnerCollectionChanged(NotifyCollectionChangedEventArgs)"/> relays these to the template collection
	/// as Remove + Add, which creates a brand-new <see cref="ItemTemplateContext2"/> on Insert.
	/// <see cref="Microsoft.UI.Xaml.Controls.ItemsRepeater"/> treats a new item as freshly added and may call
	/// <c>BringIntoView</c>, resetting the scroll position.
	/// </para>
	/// <para>
	/// This method avoids that by suppressing <see cref="InnerCollectionChanged(object?, NotifyCollectionChangedEventArgs)"/>
	/// while mutating the source (via <see cref="_observeChanges"/>), then calling <see cref="Move(int,int)"/>
	/// on the template collection directly. <see cref="MoveItem"/> fires Remove + Add on the
	/// <em>existing</em> wrapper — ItemsRepeater repositions the container in-place without recycling.
	/// </para>
	/// </summary>
	internal void MoveItemAndSyncSource(int oldIndex, int newIndex)
	{
		// Prevent re-entrant calls (e.g. if a CollectionChanged subscriber on the source
		// somehow triggers another reorder while we are mid-mutation).
		if (_isMoving)
			return;

		// Guard against stale indices (e.g. external collection changes between drag-over
		// and drop). Return early rather than throw so a mis-timed drop is a silent no-op.
		if (oldIndex < 0 || oldIndex >= _itemsSource.Count)
			return;

		_isMoving = true;
		try
		{
			// Suppress InnerCollectionChanged so the Remove+Insert we fire on the source
			// doesn't cause the template collection to apply the change a second time.
			_observeChanges = false;
			try
			{
				var sourceItem = _itemsSource[oldIndex];
				_itemsSource.RemoveAt(oldIndex);
				// After RemoveAt the count is one less; clamp so Insert never throws.
				newIndex = Math.Clamp(newIndex, 0, _itemsSource.Count);
				_itemsSource.Insert(newIndex, sourceItem);
			}
			finally
			{
				_observeChanges = true;
			}

			// Suppress TemplateCollectionChanged so the Remove+Add we fire below does not
			// attempt to back-propagate changes to the source again.
			_innerCollectionChange = true;
			try
			{
				Move(oldIndex, newIndex); // → MoveItem override → fires Remove+Add (not Move)
			}
			finally
			{
				_innerCollectionChange = false;
			}
		}
		finally
		{
			_isMoving = false;
		}
	}

	/// <summary>
	/// Overrides <see cref="ObservableCollection{T}.MoveItem"/> to fire
	/// <see cref="NotifyCollectionChangedAction.Remove"/> followed by
	/// <see cref="NotifyCollectionChangedAction.Add"/> instead of the default
	/// <see cref="NotifyCollectionChangedAction.Move"/> event.
	/// <para>
	/// CsWinRT projects <c>CollectionChanged(Move)</c> as <c>VectorChanged(Reset)</c>
	/// because WinRT's <c>IVectorChangedEventArgs.CollectionChange</c> has no Move value.
	/// When ItemsRepeater receives Reset it clears all realized containers, momentarily
	/// collapses its height to zero, and the ScrollViewer auto-clamps its offset to zero —
	/// the scroll-to-top bug.
	/// </para>
	/// <para>
	/// Firing Remove + Add instead causes CsWinRT to emit
	/// <c>VectorChanged(ItemRemoved)</c> + <c>VectorChanged(ItemInserted)</c>, which
	/// ItemsRepeater handles by recycling only the moved container while preserving the
	/// ScrollViewer offset. <see cref="_innerCollectionChange"/> (set by
	/// <see cref="InnerCollectionChanged(NotifyCollectionChangedEventArgs)"/>) prevents
	/// <see cref="TemplateCollectionChanged"/> from back-propagating these events to source.
	/// </para>
	/// </summary>
	protected override void MoveItem(int oldIndex, int newIndex)
	{
		CheckReentrancy();

		var item = this[oldIndex];

		// Update the underlying list directly — same as the base class does before
		// firing CollectionChanged(Move). We avoid calling base.MoveItem() so we
		// control which events are raised.
		Items.RemoveAt(oldIndex);
		Items.Insert(newIndex, item);

		// Notify indexer bindings that indexed items have changed — required by the
		// ObservableCollection<T> contract. Count is unchanged, so no Count notification.
		OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));

		// Wrap both events in BlockReentrancy so no mutation can slip between them.
		// Each OnCollectionChanged call already increments the reentrancy monitor
		// internally, but without an outer block a handler of Remove could trigger
		// a second MoveItem before Add fires.
		using (BlockReentrancy())
		{
			// Fire Remove + Add in place of the default Move event.
			// CollectionChanged(Remove) → CsWinRT → VectorChanged(ItemRemoved)
			// CollectionChanged(Add)    → CsWinRT → VectorChanged(ItemInserted)
			// Neither triggers VectorChanged(Reset), so ItemsRepeater preserves scroll position.
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Remove, item, oldIndex));
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add, item, newIndex));
		}
	}

	void Remove(NotifyCollectionChangedEventArgs args)
	{
		if (args.OldItems is null)
		{
			return;
		}

		var startIndex = args.OldStartingIndex;

		if (startIndex < 0)
		{
			// INCC implementation isn't giving us enough information to know where the removed items were in the
			// collection. So the best we can do is a full Reset.
			Reset();
			return;
		}

		var count = args.OldItems.Count;

		for (int index = startIndex + count - 1; index >= startIndex; index--)
		{
			RemoveAt(index);
		}
	}

	void Replace(NotifyCollectionChangedEventArgs args)
	{
		if (args.OldItems == null || args.NewItems == null)
		{
			return;
		}

		var newItemCount = args.NewItems.Count;

		if (newItemCount == args.OldItems.Count)
		{
			for (int index = 0; index < newItemCount; index++)
			{
				var item = args.NewItems[index];
				if (item is null)
					continue;

				var itemIndex = args.OldStartingIndex + index;
				var oldItem = this[itemIndex];
				var newItem = new ItemTemplateContext2(_itemTemplate, item, _container, _itemHeight, _itemWidth, _itemSpacing,
					false, false, _mauiContext);
				Items[itemIndex] = newItem;
				var update = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, itemIndex);
				OnCollectionChanged(update);
			}
		}
		else
		{
			// If we're replacing one set with an equal size set, we can do a soft reset; if not, we have to completely
			// rebuild the collection
			Reset();
		}
	}

	void Reset()
	{
		Items.Clear();
		foreach (var item in _itemsSource)
		{
			Items.Add(new ItemTemplateContext2(_itemTemplate, item, _container, _itemHeight, _itemWidth, _itemSpacing,
				false, false, _mauiContext));
		}

		var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
		OnCollectionChanged(reset);
	}
}
