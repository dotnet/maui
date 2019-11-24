using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Xamarin.Forms
{
	public class SwipeItems : Element, IList<ISwipeItem>, INotifyCollectionChanged
	{
		readonly ObservableCollection<ISwipeItem> _internal;

		public SwipeItems()
		{
			_internal = new ObservableCollection<ISwipeItem>();
			_internal.CollectionChanged += OnSwipeItemsChanged;
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
			add { _internal.CollectionChanged += value; }
			remove { _internal.CollectionChanged -= value; }
		}

		public ISwipeItem this[int index]
		{
			get => _internal.Count > index ? _internal[index] : null;
			set => _internal[index] = value;
		}

		public int Count => _internal.Count;

		public bool IsReadOnly => false;

		public void Add(ISwipeItem item)
		{
			_internal.Add(item);
		}

		public void Clear()
		{
			_internal.Clear();
		}

		public bool Contains(ISwipeItem item)
		{
			return _internal.Contains(item);
		}

		public void CopyTo(ISwipeItem[] array, int arrayIndex)
		{
			_internal.CopyTo(array, arrayIndex);
		}

		public IEnumerator<ISwipeItem> GetEnumerator()
		{
			return _internal.GetEnumerator();
		}

		public int IndexOf(ISwipeItem item)
		{
			return _internal.IndexOf(item);
		}

		public void Insert(int index, ISwipeItem item)
		{
			_internal.Insert(index, item);
		}

		public bool Remove(ISwipeItem item)
		{
			return _internal.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_internal.RemoveAt(index);
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			object bc = BindingContext;

			foreach (BindableObject item in _internal)
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
			return _internal.GetEnumerator();
		}
	}
}