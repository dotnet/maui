#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeView']/Docs/*" />
	[ContentProperty(nameof(Content))]
	public partial class SwipeView : ContentView, IElementConfiguration<SwipeView>, ISwipeViewController, ISwipeView, IVisualTreeElement
	{
		readonly Lazy<PlatformConfigurationRegistry<SwipeView>> _platformConfigurationRegistry;

		readonly List<ISwipeItem> _swipeItems = new List<ISwipeItem>();

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public SwipeView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SwipeView>>(() => new PlatformConfigurationRegistry<SwipeView>(this));

			// This just disables any of the legacy layout code from running
			DisableLayout = true;

			// SwipeItems is an Element so it participates in the Visual Hierarchy.
			// This is why we add each set of items to the logical children of swipeview
			AddLogicalChild(RightItems);
			AddLogicalChild(LeftItems);
			AddLogicalChild(TopItems);
			AddLogicalChild(BottomItems);
		}

		/// <summary>Bindable property for <see cref="Threshold"/>.</summary>
		public static readonly BindableProperty ThresholdProperty =
			BindableProperty.Create(nameof(Threshold), typeof(double), typeof(SwipeView), default(double));

		/// <summary>Bindable property for <see cref="LeftItems"/>.</summary>
		public static readonly BindableProperty LeftItemsProperty =
			BindableProperty.Create(nameof(LeftItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		/// <summary>Bindable property for <see cref="RightItems"/>.</summary>
		public static readonly BindableProperty RightItemsProperty =
			BindableProperty.Create(nameof(RightItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		/// <summary>Bindable property for <see cref="TopItems"/>.</summary>
		public static readonly BindableProperty TopItemsProperty =
			BindableProperty.Create(nameof(TopItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		/// <summary>Bindable property for <see cref="BottomItems"/>.</summary>
		public static readonly BindableProperty BottomItemsProperty =
			BindableProperty.Create(nameof(BottomItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='Threshold']/Docs/*" />
		public double Threshold
		{
			get { return (double)GetValue(ThresholdProperty); }
			set { SetValue(ThresholdProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='LeftItems']/Docs/*" />
		public SwipeItems LeftItems
		{
			get { return (SwipeItems)GetValue(LeftItemsProperty); }
			set { SetValue(LeftItemsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='RightItems']/Docs/*" />
		public SwipeItems RightItems
		{
			get { return (SwipeItems)GetValue(RightItemsProperty); }
			set { SetValue(RightItemsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='TopItems']/Docs/*" />
		public SwipeItems TopItems
		{
			get { return (SwipeItems)GetValue(TopItemsProperty); }
			set { SetValue(TopItemsProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='BottomItems']/Docs/*" />
		public SwipeItems BottomItems
		{
			get { return (SwipeItems)GetValue(BottomItemsProperty); }
			set { SetValue(BottomItemsProperty, value); }
		}

		bool ISwipeViewController.IsOpen
		{
			get => ((ISwipeView)this).IsOpen;
			set => ((ISwipeView)this).IsOpen = value;
		}

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren()
		{
			List<Maui.IVisualTreeElement> elements = new List<IVisualTreeElement>();

			// I realize we are adding these all via AddLogicalChild but the base Compatibility.Layout
			// Implements IVisualTreeElement.GetVisualChildren() and ruins that so we need to explicitly
			// implement IVTE so we can collect up all the logical children for IVTE
			// This is required for hot reload and live visual tree to work as well

			elements.Add(LeftItems);
			elements.Add(RightItems);
			elements.Add(TopItems);
			elements.Add(BottomItems);

			foreach (var item in LogicalChildrenInternal)
			{
				if (item is IVisualTreeElement vte)
				{
					elements.Add(vte);
				}
			}

			return elements;
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			SetInheritedBindingContext(LeftItems, BindingContext);
			SetInheritedBindingContext(RightItems, BindingContext);
			SetInheritedBindingContext(TopItems, BindingContext);
			SetInheritedBindingContext(BottomItems, BindingContext);
		}

		static void OnSwipeItemsChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not SwipeView swipeView)
				return;

			if (oldValue is SwipeItems oldItems)
			{
				oldItems.CollectionChanged -= SwipeItemsCollectionChanged;
				oldItems.PropertyChanged -= SwipeItemsPropertyChanged;
				swipeView.RemoveLogicalChild(oldItems);
			}

			if (newValue is SwipeItems newItems)
			{
				newItems.CollectionChanged += SwipeItemsCollectionChanged;
				newItems.PropertyChanged += SwipeItemsPropertyChanged;
				swipeView.AddLogicalChild(newItems);
			}

			void SwipeItemsPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				if (sender is SwipeItems swipeItems)
					SendChange(swipeItems);
			}

			void SwipeItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
			{
				if (sender is SwipeItems swipeItems)
					SendChange(swipeItems);
			}

			void SendChange(SwipeItems swipeItems)
			{
				if (swipeItems == swipeView.LeftItems)
					swipeView?.Handler?.UpdateValue(nameof(LeftItems));

				if (swipeItems == swipeView.RightItems)
					swipeView?.Handler?.UpdateValue(nameof(RightItems));

				if (swipeItems == swipeView.TopItems)
					swipeView?.Handler?.UpdateValue(nameof(TopItems));

				if (swipeItems == swipeView.BottomItems)
					swipeView?.Handler?.UpdateValue(nameof(BottomItems));
			}
		}

		public event EventHandler<SwipeStartedEventArgs> SwipeStarted;
		public event EventHandler<SwipeChangingEventArgs> SwipeChanging;
		public event EventHandler<SwipeEndedEventArgs> SwipeEnded;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<OpenRequestedEventArgs> OpenRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<CloseRequestedEventArgs> CloseRequested;

		/// <summary>Opens the SwipeView to reveal the specified swipe item.</summary>
		/// <param name="openSwipeItem">Specifies which side of the SwipeView to open (Left, Right, Top, or Bottom).</param>
		/// <param name="animated">Whether to animate the opening transition. Defaults to true.</param>
		public void Open(OpenSwipeItem openSwipeItem, bool animated = true)
		{
			OpenRequested?.Invoke(this, new OpenRequestedEventArgs(openSwipeItem, animated));
			((ISwipeView)this).RequestOpen(new SwipeViewOpenRequest(openSwipeItem, animated));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeView.xml" path="//Member[@MemberName='Close']/Docs/*" />
		public void Close(bool animated = true)
		{
			CloseRequested?.Invoke(this, new CloseRequestedEventArgs(animated));
			((ISwipeView)this).RequestClose(new SwipeViewCloseRequest(animated));
		}

		void ISwipeViewController.SendSwipeStarted(SwipeStartedEventArgs args) => SwipeStarted?.Invoke(this, args);

		void ISwipeViewController.SendSwipeChanging(SwipeChangingEventArgs args) => SwipeChanging?.Invoke(this, args);

		void ISwipeViewController.SendSwipeEnded(SwipeEndedEventArgs args) => SwipeEnded?.Invoke(this, args);

		SwipeItems SwipeItemsDefaultValueCreator() => new SwipeItems();

		static object SwipeItemsDefaultValueCreator(BindableObject bindable)
		{
			var swipeView = ((SwipeView)bindable);
			return swipeView.SwipeItemsDefaultValueCreator();
		}

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, SwipeView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

#nullable enable
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
					Handler?.UpdateValue(nameof(ISwipeView.IsOpen));
				}
			}
		}

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

#pragma warning disable CS0618 // Type or member is obsolete
				if (_scrollParent is ListView listView)
				{
					listView.Scrolled -= OnParentScrolled;
					return;
				}
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
				_scrollParent = this.FindParentOfType<ListView>();
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
				if (_scrollParent is ListView listView)
				{
					listView.Scrolled += OnParentScrolled;
					return;
				}
#pragma warning restore CS0618 // Type or member is obsolete

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
#nullable disable
	}
}