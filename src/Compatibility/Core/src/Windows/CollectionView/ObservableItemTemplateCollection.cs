using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class ObservableItemTemplateCollection : ObservableCollection<ItemTemplateContext>
	{
		readonly IList _itemsSource;
		readonly DataTemplate _itemTemplate;
		readonly BindableObject _container;
		readonly double _itemHeight;
		readonly double _itemWidth;
		readonly Thickness _itemSpacing;
		readonly INotifyCollectionChanged _notifyCollectionChanged;

		public ObservableItemTemplateCollection(IList itemsSource, DataTemplate itemTemplate, BindableObject container, 
			double? itemHeight = null, double? itemWidth = null, Thickness? itemSpacing = null)
		{
			if (!(itemsSource is INotifyCollectionChanged notifyCollectionChanged))
			{
				throw new ArgumentException($"{nameof(itemsSource)} must implement {nameof(INotifyCollectionChanged)}");
			}

			_notifyCollectionChanged = notifyCollectionChanged;

			_itemsSource = itemsSource;
			_itemTemplate = itemTemplate;
			_container = container;

			if (itemHeight.HasValue)
				_itemHeight = itemHeight.Value;

			if (itemWidth.HasValue)
				_itemWidth = itemWidth.Value;

			if (itemSpacing.HasValue)
				_itemSpacing = itemSpacing.Value;

			for (int n = 0; n < itemsSource.Count; n++)
			{
				// We're using this as a source for a ListViewBase, and we need INCC to work. So ListViewBase is going
				// to iterate over the entire source list right off the bat, no matter what we do. Creating one
				// ItemTemplateContext per item in the collection is unavoidable. Luckily, ITC is pretty cheap.
				Add(new ItemTemplateContext(itemTemplate, itemsSource[n], container, _itemHeight, _itemWidth, _itemSpacing));
			}

			_notifyCollectionChanged.CollectionChanged += InnerCollectionChanged;
		}

		public void CleanUp()
		{
			_notifyCollectionChanged.CollectionChanged -= InnerCollectionChanged;
		}

		void InnerCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(() => InnerCollectionChanged(args));
			}
			else
			{
				InnerCollectionChanged(args);
			}
		}

		void InnerCollectionChanged(NotifyCollectionChangedEventArgs args)
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

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);

			var count = args.NewItems.Count;

			for(int n = 0; n < count; n++)
			{
				Insert(startIndex, new ItemTemplateContext(_itemTemplate, args.NewItems[n], _container, _itemHeight, _itemWidth, _itemSpacing));
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (args.OldStartingIndex > args.NewStartingIndex)
			{
				for (int n = 0; n < count; n++)
				{
					Move(args.OldStartingIndex + n, args.NewStartingIndex + n);
				}

				return;
			}

			for(int n = count - 1; n >= 0; n--)
			{
				Move(args.OldStartingIndex + n, args.NewStartingIndex + n);
			}
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a full Reset.
				Reset();
				return;
			}

			var count = args.OldItems.Count;

			for(int n = startIndex + count - 1; n >= startIndex; n--)
			{
				RemoveAt(n);
			}
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var newItemCount = args.NewItems.Count;

			if (newItemCount == args.OldItems.Count)
			{
				for (int n = 0; n < newItemCount; n++)
				{
					var index = args.OldStartingIndex + n;
					var oldItem = this[index];
					var newItem = new ItemTemplateContext(_itemTemplate, args.NewItems[n], _container, _itemHeight, _itemWidth, _itemSpacing);
					Items[index] = newItem;
					var update = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index);
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
			for (int n = 0; n < _itemsSource.Count; n++)
			{
				Items.Add(new ItemTemplateContext(_itemTemplate, _itemsSource[n], _container, _itemHeight, _itemWidth, _itemSpacing));
			}

			var reset = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
			OnCollectionChanged(reset);
		}
	}
}