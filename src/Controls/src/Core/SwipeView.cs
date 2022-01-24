using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Content))]
	public partial class SwipeView : ContentView, IElementConfiguration<SwipeView>, ISwipeViewController
	{
		readonly Lazy<PlatformConfigurationRegistry<SwipeView>> _platformConfigurationRegistry;

		public SwipeView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<SwipeView>>(() => new PlatformConfigurationRegistry<SwipeView>(this));
		}

		public static readonly BindableProperty ThresholdProperty =
			BindableProperty.Create(nameof(Threshold), typeof(double), typeof(SwipeView), default(double));

		public static readonly BindableProperty LeftItemsProperty =
			BindableProperty.Create(nameof(LeftItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		public static readonly BindableProperty RightItemsProperty =
			BindableProperty.Create(nameof(RightItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		public static readonly BindableProperty TopItemsProperty =
			BindableProperty.Create(nameof(TopItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		public static readonly BindableProperty BottomItemsProperty =
			BindableProperty.Create(nameof(BottomItems), typeof(SwipeItems), typeof(SwipeView), null, BindingMode.OneWay, null, defaultValueCreator: SwipeItemsDefaultValueCreator,
				propertyChanged: OnSwipeItemsChanged);

		public double Threshold
		{
			get { return (double)GetValue(ThresholdProperty); }
			set { SetValue(ThresholdProperty, value); }
		}

		public SwipeItems LeftItems
		{
			get { return (SwipeItems)GetValue(LeftItemsProperty); }
			set { SetValue(LeftItemsProperty, value); }
		}

		public SwipeItems RightItems
		{
			get { return (SwipeItems)GetValue(RightItemsProperty); }
			set { SetValue(RightItemsProperty, value); }
		}

		public SwipeItems TopItems
		{
			get { return (SwipeItems)GetValue(TopItemsProperty); }
			set { SetValue(TopItemsProperty, value); }
		}

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

		static void OnSwipeItemsChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is not SwipeView swipeView)
				return;

			swipeView.UpdateSwipeItemsParent((SwipeItems)newValue);

			if (oldValue is SwipeItems oldItems)
			{
				oldItems.CollectionChanged -= SwipeItemsCollectionChanged;
				oldItems.PropertyChanged -= SwipeItemsPropertyChanged;
			}

			if (newValue is SwipeItems newItems)
			{
				newItems.CollectionChanged += SwipeItemsCollectionChanged;
				newItems.PropertyChanged += SwipeItemsPropertyChanged;
			}

			void SwipeItemsPropertyChanged(object sender, PropertyChangedEventArgs e) =>
				SendChange((SwipeItems)sender);

			void SwipeItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
				SendChange((SwipeItems)sender);

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

		public void Open(OpenSwipeItem openSwipeItem, bool animated = true)
		{
			OpenRequested?.Invoke(this, new OpenRequestedEventArgs(openSwipeItem, animated));
			((ISwipeView)this).RequestOpen(new SwipeViewOpenRequest(openSwipeItem, animated));
		}

		public void Close(bool animated = true)
		{
			CloseRequested?.Invoke(this, new CloseRequestedEventArgs(animated));
			((ISwipeView)this).RequestClose(new SwipeViewCloseRequest(animated));
		}

		void ISwipeViewController.SendSwipeStarted(SwipeStartedEventArgs args) => SwipeStarted?.Invoke(this, args);

		void ISwipeViewController.SendSwipeChanging(SwipeChangingEventArgs args) => SwipeChanging?.Invoke(this, args);

		void ISwipeViewController.SendSwipeEnded(SwipeEndedEventArgs args) => SwipeEnded?.Invoke(this, args);

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			object bc = BindingContext;

			if (LeftItems != null)
				SetInheritedBindingContext(LeftItems, bc);

			if (RightItems != null)
				SetInheritedBindingContext(RightItems, bc);

			if (TopItems != null)
				SetInheritedBindingContext(TopItems, bc);

			if (BottomItems != null)
				SetInheritedBindingContext(BottomItems, bc);
		}

		SwipeItems SwipeItemsDefaultValueCreator() => new SwipeItems();

		static object SwipeItemsDefaultValueCreator(BindableObject bindable)
		{
			return ((SwipeView)bindable).SwipeItemsDefaultValueCreator();
		}

		public IPlatformElementConfiguration<T, SwipeView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void UpdateSwipeItemsParent(SwipeItems swipeItems)
		{
			if (swipeItems.Parent == null)
				swipeItems.Parent = this;

			foreach (var item in swipeItems)
			{
				if (item is Element swipeItem && swipeItem.Parent == null)
					swipeItem.Parent = swipeItems;
			}
		}
	}
}