using System;
using System.Collections;
using System.Collections.Specialized;

namespace Xamarin.Forms.Platform.Android
{
	internal class ObservableItemsSource : IItemsViewSource
	{
		readonly IEnumerable _itemsSource;
		readonly ICollectionChangedNotifier _notifier;
		bool _disposed;

		public ObservableItemsSource(IEnumerable itemSource, ICollectionChangedNotifier notifier)
		{
			_itemsSource = itemSource as IList ?? itemSource as IEnumerable;
			_notifier = notifier;

			((INotifyCollectionChanged)itemSource).CollectionChanged += CollectionChanged;
		}


		internal event NotifyCollectionChangedEventHandler CollectionItemsSourceChanged;

		public int Count => ItemsCount() + (HasHeader ? 1 : 0) + (HasFooter ? 1 : 0);

		public bool HasHeader { get; set; }
		public bool HasFooter { get; set; }

		public void Dispose()
		{
			Dispose(true);
		}

		public bool IsFooter(int index)
		{
			return HasFooter && index == Count - 1;
		}

		public bool IsHeader(int index)
		{
			return HasHeader && index == 0;
		}

		public int GetPosition(object item)
		{
			for (int n = 0; n < ItemsCount(); n++)
			{
				if (ElementAt(n) == item)
				{
					return AdjustPositionForHeader(n);
				}
			}

			return -1;
		}

		public object GetItem(int position)
		{
			return ElementAt(AdjustIndexForHeader(position));
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				((INotifyCollectionChanged)_itemsSource).CollectionChanged -= CollectionChanged;
			}
		}

		int AdjustIndexForHeader(int index)
		{
			return index - (HasHeader ? 1 : 0);
		}

		int AdjustPositionForHeader(int position)
		{
			return position + (HasHeader ? 1 : 0);
		}

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
		{
			if (Device.IsInvokeRequired)
			{
				Device.BeginInvokeOnMainThread(() => CollectionChanged(args));
			}
			else
			{
				CollectionChanged(args);
			}
			
		}

		void CollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Add(args);
					break;
				case NotifyCollectionChangedAction.Remove:
					Remove(args);
					break;
				case NotifyCollectionChangedAction.Replace:
					Replace(args);
					break;
				case NotifyCollectionChangedAction.Move:
					Move(args);
					break;
				case NotifyCollectionChangedAction.Reset:
					_notifier.NotifyDataSetChanged();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			CollectionItemsSourceChanged?.Invoke(this, args);
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (count == 1)
			{
				// For a single item, we can use NotifyItemMoved and get the animation
				_notifier.NotifyItemMoved(this, AdjustPositionForHeader(args.OldStartingIndex), AdjustPositionForHeader(args.NewStartingIndex));
				return;
			}

			var start = AdjustPositionForHeader(Math.Min(args.OldStartingIndex, args.NewStartingIndex));
			var end = AdjustPositionForHeader(Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count);
			_notifier.NotifyItemRangeChanged(this, start, end);
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf(args.NewItems[0]);
			startIndex = AdjustPositionForHeader(startIndex);
			var count = args.NewItems.Count;

			if (count == 1)
			{
				_notifier.NotifyItemInserted(this, startIndex);
				return;
			}

			_notifier.NotifyItemRangeInserted(this, startIndex, count);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a NotifyDataSetChanged()
				_notifier.NotifyDataSetChanged();
				return;
			}

			startIndex = AdjustPositionForHeader(startIndex);

			// If we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count;

			if (count == 1)
			{
				_notifier.NotifyItemRemoved(this, startIndex);
				return;
			}

			_notifier.NotifyItemRangeRemoved(this, startIndex, count);
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : IndexOf(args.NewItems[0]);
			startIndex = AdjustPositionForHeader(startIndex);
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				// We are replacing one set of items with a set of equal size; we can do a simple item or range 
				// notification to the adapter
				if (newCount == 1)
				{
					_notifier.NotifyItemChanged(this, startIndex);
				}
				else
				{
					_notifier.NotifyItemRangeChanged(this, startIndex, newCount);
				}

				return;
			}

			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use NotifyDataSetChanged and let the RecyclerView update everything
			_notifier.NotifyDataSetChanged();
		}

		internal int ItemsCount()
		{
			if (_itemsSource is IList list)
				return list.Count;

			int count = 0;
			foreach (var item in _itemsSource)
				count++;
			return count;
		}

		internal object ElementAt(int index)
		{
			if (_itemsSource is IList list)
				return list[index];

			int count = 0;
			foreach (var item in _itemsSource)
			{
				if (count == index)
					return item;
				count++;
			}

			return -1;
		}

		internal int IndexOf(object item)
		{
			if (_itemsSource is IList list)
				return list.IndexOf(item);

			int count = 0;
			foreach (var i in _itemsSource)
			{
				if (i == item)
					return count;
				count++;
			}

			return -1;
		}
	}
}
