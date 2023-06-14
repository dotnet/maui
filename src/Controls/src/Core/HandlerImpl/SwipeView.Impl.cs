using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeView']/Docs/*" />
	public partial class SwipeView : ISwipeView
	{
		const float SwipeMinimumDelta = 10f;

		bool _isOpen;
		double _previousScrollX;
		double _previousScrollY;
		View? _scrollParent;
		SwipeDirection? _swipeDirection;

		ISwipeItems ISwipeView.LeftItems => new HandlerSwipeItems(LeftItems);

		ISwipeItems ISwipeView.RightItems => new HandlerSwipeItems(RightItems);

		ISwipeItems ISwipeView.TopItems => new HandlerSwipeItems(TopItems);

		ISwipeItems ISwipeView.BottomItems => new HandlerSwipeItems(BottomItems);

		bool ISwipeView.IsOpen
		{
			get => _isOpen;
			set
			{
				if (_isOpen != value)
				{
					_isOpen = value;
					UpdateLogicalChildren();
					Handler?.UpdateValue(nameof(ISwipeView.IsOpen));
				}
			}
		}

		readonly List<ISwipeItem> _swipeItems = new List<ISwipeItem>();

		private protected override IList<Element> LogicalChildrenInternalBackingStore
			=> new CastingList<Element, ISwipeItem>(_swipeItems);
#if IOS
		SwipeTransitionMode ISwipeView.SwipeTransitionMode =>
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.SwipeView.GetSwipeTransitionMode(this);
#elif ANDROID
		SwipeTransitionMode ISwipeView.SwipeTransitionMode =>
			Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.SwipeView.GetSwipeTransitionMode(this);
#else
		SwipeTransitionMode ISwipeView.SwipeTransitionMode => SwipeTransitionMode.Reveal;
#endif


		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			child.PropertyChanged += OnPropertyChanged;
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			child.PropertyChanged -= OnPropertyChanged;
		}

		void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == IsEnabledProperty.PropertyName)
				Handler?.UpdateValue(nameof(IsEnabled));
			else if (e.PropertyName == MarginProperty.PropertyName)
				UpdateMargin();
		}

		private protected override void OnParentChangedCore()
		{
			if (_scrollParent != null)
			{
				if (_scrollParent is ScrollView scrollView)
				{
					scrollView.Scrolled -= OnParentScrolled;
				}

				if (_scrollParent is ListView listView)
				{
					listView.Scrolled -= OnParentScrolled;
					return;
				}

				if (_scrollParent is Microsoft.Maui.Controls.CollectionView collectionView)
				{
					collectionView.Scrolled -= OnParentScrolled;
				}

				_scrollParent = null;
			}

			base.OnParentChangedCore();

			if (_scrollParent == null)
			{
				_scrollParent = this.FindParentOfType<ScrollView>();

				if (_scrollParent is ScrollView scrollView)
				{
					scrollView.Scrolled += OnParentScrolled;
					return;
				}

				_scrollParent = this.FindParentOfType<ListView>();

				if (_scrollParent is ListView listView)
				{
					listView.Scrolled += OnParentScrolled;
					return;
				}

				_scrollParent = this.FindParentOfType<Microsoft.Maui.Controls.CollectionView>();

				if (_scrollParent is Microsoft.Maui.Controls.CollectionView collectionView)
				{
					collectionView.Scrolled += OnParentScrolled;
				}
			}
		}

		void UpdateMargin()
		{
			if (this is not ISwipeView swipeView)
				return;

			if (swipeView.IsOpen)
				swipeView.RequestClose(new SwipeViewCloseRequest(false));
		}

		void UpdateLogicalChildren()
		{
			if (!_isOpen)
			{
				ClearLogicalChildren();
				return;
			}

			var swipeItems = GetSwipeItemsByDirection(_swipeDirection);

			if (swipeItems is null)
				return;

			foreach (var swipeItem in swipeItems)
				AddLogicalChild((Element)swipeItem);
		}

		SwipeItems? GetSwipeItemsByDirection(SwipeDirection? swipeDirection)
		{
			SwipeItems? swipeItems = null;

			switch (swipeDirection)
			{
				case SwipeDirection.Left:
					swipeItems = RightItems;
					break;
				case SwipeDirection.Right:
					swipeItems = LeftItems;
					break;
				case SwipeDirection.Up:
					swipeItems = BottomItems;
					break;
				case SwipeDirection.Down:
					swipeItems = TopItems;
					break;
			}

			return swipeItems;
		}

		internal void AddLogicalChild(Element element)
			=> AddLogicalChildInternal(element);

		internal new void ClearLogicalChildren()
		{
			// Reverse for-loop, so children can be removed while iterating
			for (int i = _swipeItems.Count - 1; i >= 0; i--)
			{
				RemoveLogicalChildByIndex(i);
			}
		}

		void RemoveLogicalChildByIndex(int index)
		{
			if (_swipeItems.Count < index)
			{
				return;
			}

			var element = _swipeItems[index] as Element;

			if (element is null)
			{
				return;
			}

			RemoveLogicalChildInternal(element);
		}

		void OnParentScrolled(object? sender, ScrolledEventArgs e)
		{
			var horizontalDelta = e.ScrollX - _previousScrollX;
			var verticalDelta = e.ScrollY - _previousScrollY;

			if (horizontalDelta > SwipeMinimumDelta || verticalDelta > SwipeMinimumDelta)
				((ISwipeView)this).RequestClose(new SwipeViewCloseRequest(true));

			_previousScrollX = e.ScrollX;
			_previousScrollY = e.ScrollY;
		}

		void OnParentScrolled(object? sender, ItemsViewScrolledEventArgs e)
		{
			if (e.HorizontalDelta > SwipeMinimumDelta || e.VerticalDelta > SwipeMinimumDelta)
				((ISwipeView)this).RequestClose(new SwipeViewCloseRequest(true));
		}

		void ISwipeView.SwipeStarted(SwipeViewSwipeStarted swipeStarted)
		{
			_swipeDirection = swipeStarted.SwipeDirection;
			var swipeStartedEventArgs = new SwipeStartedEventArgs(swipeStarted.SwipeDirection);
			((ISwipeViewController)this).SendSwipeStarted(swipeStartedEventArgs);
		}

		void ISwipeView.SwipeChanging(SwipeViewSwipeChanging swipeChanging)
		{
			var swipeChangingEventArgs = new SwipeChangingEventArgs(swipeChanging.SwipeDirection, swipeChanging.Offset);
			((ISwipeViewController)this).SendSwipeChanging(swipeChangingEventArgs);
		}

		void ISwipeView.SwipeEnded(SwipeViewSwipeEnded swipeEnded)
		{
			_swipeDirection = swipeEnded.SwipeDirection;
			var swipeEndedEventArgs = new SwipeEndedEventArgs(swipeEnded.SwipeDirection, swipeEnded.IsOpen);
			((ISwipeViewController)this).SendSwipeEnded(swipeEndedEventArgs);
		}

		void ISwipeView.RequestOpen(SwipeViewOpenRequest swipeOpenRequest)
		{
			switch (swipeOpenRequest.OpenSwipeItem)
			{
				case OpenSwipeItem.LeftItems:
					_swipeDirection = SwipeDirection.Right;
					break;
				case OpenSwipeItem.TopItems:
					_swipeDirection = SwipeDirection.Down;
					break;
				case OpenSwipeItem.RightItems:
					_swipeDirection = SwipeDirection.Left;
					break;
				case OpenSwipeItem.BottomItems:
					_swipeDirection = SwipeDirection.Up;
					break;
				default:
					_swipeDirection = null;
					break;
			}

			Handler?.Invoke(nameof(ISwipeView.RequestOpen), swipeOpenRequest);
		}

		void ISwipeView.RequestClose(SwipeViewCloseRequest swipeCloseRequest)
		{
			Handler?.Invoke(nameof(ISwipeView.RequestClose), swipeCloseRequest);
		}

		class HandlerSwipeItems : List<Maui.ISwipeItem>, ISwipeItems
		{
			readonly SwipeItems _swipeItems;

			public HandlerSwipeItems(SwipeItems swipeItems) : base(swipeItems)
			{
				_swipeItems = swipeItems;
			}

			public SwipeMode Mode => _swipeItems.Mode;

			public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked => _swipeItems.SwipeBehaviorOnInvoked;
		}
	}
}