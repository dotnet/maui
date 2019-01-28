using System;
using System.Collections;
using System.Collections.Specialized;
using Android.Support.V7.Widget;

namespace Xamarin.Forms.Platform.Android
{
	internal class ObservableItemsSource : IItemsViewSource
	{
		// TODO hartez 2018/07/30 14:40:11 We may need to implement IDisposable to make sure this all gets cleaned up	
		readonly RecyclerView.Adapter _adapter;
		readonly IList _itemsSource;

		public ObservableItemsSource(IEnumerable itemSource, RecyclerView.Adapter adapter)
		{
			_itemsSource = (IList)itemSource;
			_adapter = adapter;

			((INotifyCollectionChanged)itemSource).CollectionChanged += CollectionChanged;
		}

		public int Count => _itemsSource.Count;

		public object this[int index] => _itemsSource[index];

		void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
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
					_adapter.NotifyDataSetChanged();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void Move(NotifyCollectionChangedEventArgs args)
		{
			var count = args.NewItems.Count;

			if (count == 1)
			{
				// For a single item, we can use NotifyItemMoved and get the animation
				_adapter.NotifyItemMoved(args.OldStartingIndex, args.NewStartingIndex);
				return;
			}

			var start = Math.Min(args.OldStartingIndex, args.NewStartingIndex);
			var end = Math.Max(args.OldStartingIndex, args.NewStartingIndex) + count;
			_adapter.NotifyItemRangeChanged(start, end);
		}

		void Add(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);
			var count = args.NewItems.Count;

			if (count == 1)
			{
				_adapter.NotifyItemInserted(startIndex);
				return;
			}

			_adapter.NotifyItemRangeInserted(startIndex, count);
		}

		void Remove(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.OldStartingIndex;

			if (startIndex < 0)
			{
				// INCC implementation isn't giving us enough information to know where the removed items were in the
				// collection. So the best we can do is a NotifyDataSetChanged()
				_adapter.NotifyDataSetChanged();
				return;
			}

			// If we have a start index, we can be more clever about removing the item(s) (and get the nifty animations)
			var count = args.OldItems.Count;

			if (count == 1)
			{
				_adapter.NotifyItemRemoved(startIndex);
				return;
			}

			_adapter.NotifyItemRangeRemoved(startIndex, count);
		}

		void Replace(NotifyCollectionChangedEventArgs args)
		{
			var startIndex = args.NewStartingIndex > -1 ? args.NewStartingIndex : _itemsSource.IndexOf(args.NewItems[0]);
			var newCount = args.NewItems.Count;

			if (newCount == args.OldItems.Count)
			{
				// We are replacing one set of items with a set of equal size; we can do a simple item or range 
				// notification to the adapter
				if (newCount == 1)
				{
					_adapter.NotifyItemChanged(startIndex);
				}
				else
				{
					_adapter.NotifyItemRangeChanged(startIndex, newCount);
				}

				return;
			}
			
			// The original and replacement sets are of unequal size; this means that everything currently in view will 
			// have to be updated. So we just have to use NotifyDataSetChanged and let the RecyclerView update everything
			_adapter.NotifyDataSetChanged();
		}
	}
}