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
			_adapter.NotifyItemMoved(args.OldStartingIndex, args.NewStartingIndex);
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
			var startIndex = args.OldStartingIndex > -1 ? args.OldStartingIndex : _itemsSource.IndexOf(args.OldItems[0]);
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
			var count = args.NewItems.Count;

			if (count == 1)
			{
				_adapter.NotifyItemChanged(startIndex);
				return;
			}

			_adapter.NotifyItemRangeChanged(startIndex, count);
		}
	}
}