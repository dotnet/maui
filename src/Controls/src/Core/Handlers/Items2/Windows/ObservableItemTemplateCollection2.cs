using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
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

			if (itemHeight.HasValue)
			{
				_itemHeight = itemHeight.Value;
			}

			if (itemWidth.HasValue)
			{
				_itemWidth = itemWidth.Value;
			}

			if (itemSpacing.HasValue)
			{
				_itemSpacing = itemSpacing.Value;
			}

			for (int index = 0; index < itemsSource.Count; index++)
			{
				// We're using this as a source for a ListViewBase, and we need INCC to work. So ListViewBase is going
				// to iterate over the entire source list right off the bat, no matter what we do. Creating one
				// ItemTemplateContext per item in the collection is unavoidable. Luckily, ITC is pretty cheap.
				Add(new ItemTemplateContext2(itemTemplate, itemsSource[index]!, container, _itemHeight, _itemWidth, _itemSpacing,
					false, false, _mauiContext));
			}

			_collectionChanged = InnerCollectionChanged;
			if (itemsSource is INotifyCollectionChanged notifyCollectionChanged)
			{
				_proxy.Subscribe(notifyCollectionChanged, _collectionChanged);
			}

			CollectionChanged += TemplateCollectionChanged;
		}

		public void CleanUp()
		{
			CollectionChanged -= TemplateCollectionChanged;
			_proxy.Unsubscribe();
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

			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf((ItemTemplateContext2)args.NewItems[0]!);

			var count = args.NewItems.Count;

			for (int index = 0; index < count; index++)
			{
				var newItem = (ItemTemplateContext2?)args.NewItems[index];
				if (newItem != null)
					_itemsSource.Insert(startIndex, newItem.Item);
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
				Insert(startIndex, new ItemTemplateContext2(_itemTemplate, args.NewItems[index]!, _container, _itemHeight, _itemWidth, _itemSpacing, 
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
					var itemIndex = args.OldStartingIndex + index;
					var oldItem = this[itemIndex];
					var newItem = new ItemTemplateContext2(_itemTemplate, args.NewItems[index]!, _container, _itemHeight, _itemWidth, _itemSpacing, 
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
}