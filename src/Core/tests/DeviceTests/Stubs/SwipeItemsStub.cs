using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class SwipeItemsStub : StubBase, ISwipeItems
	{
		readonly ObservableCollection<Maui.ISwipeItem> _swipeItems;

		public SwipeItemsStub(IEnumerable<ISwipeItem> swipeItems)
		{
			_swipeItems = new ObservableCollection<Maui.ISwipeItem>(swipeItems) ?? throw new ArgumentNullException(nameof(swipeItems));
			_swipeItems.CollectionChanged += OnSwipeItemsChanged;
		}

		public SwipeItemsStub() : this(Enumerable.Empty<ISwipeItem>())
		{

		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public ISwipeItem this[int index]
		{
			get => _swipeItems.Count > index ? (ISwipeItem)_swipeItems[index] : null;
			set => _swipeItems[index] = value;
		}

		public int Count => _swipeItems.Count;

		public bool IsReadOnly => false;

		public SwipeMode Mode { get; set; }

		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked { get; set; }

		public void Add(ISwipeItem item)
		{
			_swipeItems.Add(item);
		}

		public void Clear()
		{
			_swipeItems.Clear();
		}

		public bool Contains(ISwipeItem item)
		{
			return _swipeItems.Contains(item);
		}

		public void CopyTo(ISwipeItem[] array, int arrayIndex)
		{
			_swipeItems.CopyTo(array, arrayIndex);
		}

		public IEnumerator<ISwipeItem> GetEnumerator()
		{
			foreach (ISwipeItem item in _swipeItems)
				yield return item;
		}

		public int IndexOf(ISwipeItem item)
		{
			return _swipeItems.IndexOf(item);
		}

		public void Insert(int index, ISwipeItem item)
		{
			_swipeItems.Insert(index, item);
		}

		public bool Remove(ISwipeItem item)
		{
			return _swipeItems.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_swipeItems.RemoveAt(index);
		}

		void OnSwipeItemsChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _swipeItems.GetEnumerator();
		}
	}
}