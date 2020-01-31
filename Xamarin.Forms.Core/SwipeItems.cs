using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms
{
	public class SwipeItems : Element, IList<ISwipeItem>, INotifyCollectionChanged
	{
		readonly ObservableCollection<ISwipeItem> _swipeItems;

		public SwipeItems(IEnumerable<ISwipeItem> swipeItems)
		{
			_swipeItems = new ObservableCollection<ISwipeItem>(swipeItems) ?? throw new ArgumentNullException(nameof(swipeItems));
			_swipeItems.CollectionChanged += OnSwipeItemsChanged;
		}

		public SwipeItems() : this(Enumerable.Empty<ISwipeItem>())
		{

		}

		public static readonly BindableProperty ModeProperty = BindableProperty.Create(nameof(Mode), typeof(SwipeMode), typeof(SwipeItems), SwipeMode.Reveal);
		public static readonly BindableProperty SwipeBehaviorOnInvokedProperty = BindableProperty.Create(nameof(SwipeBehaviorOnInvoked), typeof(SwipeBehaviorOnInvoked), typeof(SwipeItems), SwipeBehaviorOnInvoked.Auto);

		public SwipeMode Mode
		{
			get { return (SwipeMode)GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}

		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked
		{
			get { return (SwipeBehaviorOnInvoked)GetValue(SwipeBehaviorOnInvokedProperty); }
			set { SetValue(SwipeBehaviorOnInvokedProperty, value); }
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add { _swipeItems.CollectionChanged += value; }
			remove { _swipeItems.CollectionChanged -= value; }
		}

		public ISwipeItem this[int index]
		{
			get => _swipeItems.Count > index ? _swipeItems[index] : null;
			set => _swipeItems[index] = value;
		}

		public int Count => _swipeItems.Count;

		public bool IsReadOnly => false;

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
			return _swipeItems.GetEnumerator();
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

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			object bc = BindingContext;

			foreach (BindableObject item in _swipeItems)
				SetInheritedBindingContext(item, bc);
		}

		void OnSwipeItemsChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			if (notifyCollectionChangedEventArgs.NewItems == null)
				return;

			object bc = BindingContext;

			foreach (BindableObject item in notifyCollectionChangedEventArgs.NewItems)
				SetInheritedBindingContext(item, bc);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _swipeItems.GetEnumerator();
		}
	}
}