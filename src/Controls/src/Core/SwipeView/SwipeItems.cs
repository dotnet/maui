#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeItems']/Docs/*" />
	public class SwipeItems : Element, IList<ISwipeItem>, INotifyCollectionChanged
	{
		readonly ObservableCollection<Maui.ISwipeItem> _swipeItems;

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public SwipeItems(IEnumerable<ISwipeItem> swipeItems)
		{
			foreach (var item in swipeItems)
				if (item is Element e)
					AddLogicalChild(e);

			_swipeItems = new ObservableCollection<Maui.ISwipeItem>(swipeItems) ?? throw new ArgumentNullException(nameof(swipeItems));
			_swipeItems.CollectionChanged += OnSwipeItemsChanged;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public SwipeItems() : this(Enumerable.Empty<ISwipeItem>())
		{

		}

		/// <summary>Bindable property for <see cref="Mode"/>.</summary>
		public static readonly BindableProperty ModeProperty = BindableProperty.Create(nameof(Mode), typeof(SwipeMode), typeof(SwipeItems), SwipeMode.Reveal);
		/// <summary>Bindable property for <see cref="SwipeBehaviorOnInvoked"/>.</summary>
		public static readonly BindableProperty SwipeBehaviorOnInvokedProperty = BindableProperty.Create(nameof(SwipeBehaviorOnInvoked), typeof(SwipeBehaviorOnInvoked), typeof(SwipeItems), SwipeBehaviorOnInvoked.Auto);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Mode']/Docs/*" />
		public SwipeMode Mode
		{
			get { return (SwipeMode)GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='SwipeBehaviorOnInvoked']/Docs/*" />
		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked
		{
			get { return (SwipeBehaviorOnInvoked)GetValue(SwipeBehaviorOnInvokedProperty); }
			set { SetValue(SwipeBehaviorOnInvokedProperty, value); }
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public ISwipeItem this[int index]
		{
			get => _swipeItems.Count > index ? (ISwipeItem)_swipeItems[index] : null;
			set => _swipeItems[index] = value;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Count']/Docs/*" />
		public int Count => _swipeItems.Count;

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='IsReadOnly']/Docs/*" />
		public bool IsReadOnly => false;

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Add']/Docs/*" />
		public void Add(ISwipeItem item)
		{
			_swipeItems.Add(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Clear']/Docs/*" />
		public void Clear()
		{
			foreach (var item in _swipeItems)
				if (item is Element e)
					RemoveLogicalChild(e);

			_swipeItems.Clear();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Contains']/Docs/*" />
		public bool Contains(ISwipeItem item)
		{
			return _swipeItems.Contains(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='CopyTo']/Docs/*" />
		public void CopyTo(ISwipeItem[] array, int arrayIndex)
		{
			_swipeItems.CopyTo(array, arrayIndex);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='GetEnumerator']/Docs/*" />
		public IEnumerator<ISwipeItem> GetEnumerator()
		{
			foreach (ISwipeItem item in _swipeItems)
				yield return item;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='IndexOf']/Docs/*" />
		public int IndexOf(ISwipeItem item)
		{
			return _swipeItems.IndexOf(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Insert']/Docs/*" />
		public void Insert(int index, ISwipeItem item)
		{
			_swipeItems.Insert(index, item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='Remove']/Docs/*" />
		public bool Remove(ISwipeItem item)
		{
			return _swipeItems.Remove(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeItems.xml" path="//Member[@MemberName='RemoveAt']/Docs/*" />
		public void RemoveAt(int index)
		{
			_swipeItems.RemoveAt(index);
		}

		void OnSwipeItemsChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			if (notifyCollectionChangedEventArgs.NewItems is not null)
			{
				foreach (var item in notifyCollectionChangedEventArgs.NewItems)
					if (item is Element e)
						AddLogicalChild(e);
			}

			if (notifyCollectionChangedEventArgs.OldItems is not null)
			{
				foreach (var item in notifyCollectionChangedEventArgs.OldItems)
					if (item is Element e)
						RemoveLogicalChild(e);
			}

			CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _swipeItems.GetEnumerator();
		}
	}
}
