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

			_observeChanges = true;
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
		if (!_observeChanges)
		{
			return;
		}

		_container.Dispatcher.DispatchIfRequired(() => InnerCollectionChanged(args));
	}

	void InnerCollectionChanged(NotifyCollectionChangedEventArgs args)
	{
		_innerCollectionChange = true;
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
		_innerCollectionChange = false;
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
			for (int index = 0; index < count; index++)
			{
				Move(args.OldStartingIndex + index, args.NewStartingIndex + index);
			}

			return;
		}

		for (int index = count - 1; index >= 0; index--)
		{
			Move(args.OldStartingIndex + index, args.NewStartingIndex + index);
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
