#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a collection of <see cref="ISwipeItem"/> objects used by a <see cref="SwipeView"/>.
	/// </summary>
	public class SwipeItems : Element, IList<ISwipeItem>, INotifyCollectionChanged
	{
		readonly ObservableCollection<Maui.ISwipeItem> _swipeItems;

		/// <summary>
		/// Initializes a new instance of the <see cref="SwipeItems"/> class with the specified swipe items.
		/// </summary>
		/// <param name="swipeItems">The initial collection of swipe items.</param>
		public SwipeItems(IEnumerable<ISwipeItem> swipeItems)
		{
			foreach (var item in swipeItems)
				if (item is Element e)
				{
					CheckParent(e);
					AddLogicalChild(e);
				}

			_swipeItems = new ObservableCollection<Maui.ISwipeItem>(swipeItems) ?? throw new ArgumentNullException(nameof(swipeItems));
			_swipeItems.CollectionChanged += OnSwipeItemsChanged;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SwipeItems"/> class.
		/// </summary>
		public SwipeItems() : this(Enumerable.Empty<ISwipeItem>())
		{

		}

		/// <summary>Bindable property for <see cref="Mode"/>.</summary>
		public static readonly BindableProperty ModeProperty = BindableProperty.Create(nameof(Mode), typeof(SwipeMode), typeof(SwipeItems), SwipeMode.Reveal);
		/// <summary>Bindable property for <see cref="SwipeBehaviorOnInvoked"/>.</summary>
		public static readonly BindableProperty SwipeBehaviorOnInvokedProperty = BindableProperty.Create(nameof(SwipeBehaviorOnInvoked), typeof(SwipeBehaviorOnInvoked), typeof(SwipeItems), SwipeBehaviorOnInvoked.Auto);

		/// <summary>
		/// Gets or sets a value that indicates how the swipe items are displayed. This is a bindable property.
		/// </summary>
		public SwipeMode Mode
		{
			get { return (SwipeMode)GetValue(ModeProperty); }
			set { SetValue(ModeProperty, value); }
		}

		/// <summary>
		/// Gets or sets the behavior when a swipe item is invoked. This is a bindable property.
		/// </summary>
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

		/// <inheritdoc/>
		public int Count => _swipeItems.Count;

		/// <inheritdoc/>
		public bool IsReadOnly => false;

		/// <inheritdoc/>
		public void Add(ISwipeItem item)
		{
			_swipeItems.Add(item);
		}

		/// <inheritdoc/>
		public void Clear()
		{
			foreach (var item in _swipeItems)
				if (item is Element e)
					RemoveLogicalChild(e);

			_swipeItems.Clear();
		}

		/// <inheritdoc/>
		public bool Contains(ISwipeItem item)
		{
			return _swipeItems.Contains(item);
		}

		/// <inheritdoc/>
		public void CopyTo(ISwipeItem[] array, int arrayIndex)
		{
			_swipeItems.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc/>
		public IEnumerator<ISwipeItem> GetEnumerator()
		{
			foreach (ISwipeItem item in _swipeItems)
				yield return item;
		}

		/// <inheritdoc/>
		public int IndexOf(ISwipeItem item)
		{
			return _swipeItems.IndexOf(item);
		}

		/// <inheritdoc/>
		public void Insert(int index, ISwipeItem item)
		{
			_swipeItems.Insert(index, item);
		}

		/// <inheritdoc/>
		public bool Remove(ISwipeItem item)
		{
			return _swipeItems.Remove(item);
		}

		/// <inheritdoc/>
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
					{
						CheckParent(e);
						AddLogicalChild(e);
					}
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

		// If a SwipeItem occupies multiple SwipeItems, we only want the logical hierarchy
		// to wire up to the SwipeItems that are currently part of a SwipeView.
		// We could throw an exception here but that would be too hostile of a breaking behavior.
		// TODO NET9 This warning should probably be elevated to `Element` for NET9
		void CheckParent(Element e)
		{
			if (e.Parent is not null && e.Parent != this)
			{
				this.CreateLogger<SwipeItems>()
					?.LogWarning($"{e} is already part of {e.Parent}. Remove from {e.Parent} to avoid inconsistent behavior.");
			}
		}
	}
}
