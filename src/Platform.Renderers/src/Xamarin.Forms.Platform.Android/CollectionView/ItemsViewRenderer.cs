using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.CollectionView;
using Xamarin.Forms.Platform.Android.FastRenderers;
using ARect = Android.Graphics.Rect;
using AViewCompat = AndroidX.Core.View.ViewCompat;

namespace Xamarin.Forms.Platform.Android
{
	public abstract class ItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource> : RecyclerView, IVisualElementRenderer, IEffectControlProvider
		where TItemsView : ItemsView
		where TAdapter : ItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsViewSource : IItemsViewSource
	{
		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		readonly EffectControlProvider _effectControlProvider;

		protected TAdapter ItemsViewAdapter;

		int? _defaultLabelFor;
		bool _disposed;

		protected TItemsView ItemsView;
		protected IItemsLayout ItemsLayout { get; private set; }

		SnapManager _snapManager;
		ScrollHelper _scrollHelper;
		RecyclerViewScrollListener<TItemsView, TItemsViewSource> _recyclerViewScrollListener;

		EmptyViewAdapter _emptyViewAdapter;
		readonly DataChangeObserver _emptyCollectionObserver;
		readonly DataChangeObserver _itemsUpdateScrollObserver;

		ScrollBarVisibility _defaultHorizontalScrollVisibility = ScrollBarVisibility.Default;
		ScrollBarVisibility _defaultVerticalScrollVisibility = ScrollBarVisibility.Default;

		RecyclerView.ItemDecoration _itemDecoration;

		public ItemsViewRenderer(Context context) : base(
			new ContextThemeWrapper(context, Resource.Style.collectionViewTheme), null,
			Resource.Attribute.collectionViewStyle)
		{
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_effectControlProvider = new EffectControlProvider(this);

			_emptyCollectionObserver = new DataChangeObserver(UpdateEmptyViewVisibility);
			_itemsUpdateScrollObserver = new DataChangeObserver(AdjustScrollForItemUpdate);

			VerticalScrollBarEnabled = false;
			HorizontalScrollBarEnabled = false;
		}

		internal ScrollHelper ScrollHelper => _scrollHelper = _scrollHelper ?? new ScrollHelper(this);

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			AViewCompat.SetClipBounds(this, new ARect(0, 0, Width, Height));

			// After a direct (non-animated) scroll operation, we may need to make adjustments
			// to align the target item; if an adjustment is pending, execute it here.
			// (Deliberately checking the private member here rather than the property accessor; the accessor will
			// create a new ScrollHelper if needed, and there's no reason to do that until a Scroll is requested.)
			_scrollHelper?.AdjustScroll();
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		public VisualElement Element => ItemsView;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!(element is ItemsView))
			{
				throw new ArgumentException($"{nameof(element)} must be of type {typeof(ItemsView).Name}");
			}

			var oldElement = ItemsView;
			var newElement = (TItemsView)element;

			TearDownOldElement(oldElement);
			SetUpNewElement(newElement);

			OnElementChanged(oldElement, newElement);

			// TODO hartez 2018/06/06 20:57:12 Find out what this does, and whether we really need it	
			element.SendViewInitialized(this);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			// TODO hartez 2018/06/06 20:58:54 Rethink whether we need to have _defaultLabelFor as a class member	
			if (_defaultLabelFor == null)
			{
				_defaultLabelFor = LabelFor;
			}

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		public VisualElementTracker Tracker { get; private set; }

		void IVisualElementRenderer.UpdateLayout()
		{
			Tracker?.UpdateLayout();
		}

		public global::Android.Views.View View => this;

		public ViewGroup ViewGroup => null;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_automationPropertiesProvider?.Dispose();
				Tracker?.Dispose();

				if (Element != null)
				{
					TearDownOldElement(Element as ItemsView);

					if (Platform.GetRenderer(Element) == this)
					{
						Element.ClearValue(Platform.RendererProperty);
					}
				}
			}

			base.Dispose(disposing);
		}

		protected virtual LayoutManager SelectLayoutManager(IItemsLayout layoutSpecification)
		{
			switch (layoutSpecification)
			{
				case GridItemsLayout gridItemsLayout:
					return CreateGridLayout(gridItemsLayout);
				case LinearItemsLayout listItemsLayout:
					var orientation = listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
						? LinearLayoutManager.Horizontal
						: LinearLayoutManager.Vertical;

					return new LinearLayoutManager(Context, orientation, false);
			}

			// Fall back to plain old vertical list
			// TODO hartez 2018/08/30 19:34:36 Log a warning when we have to fall back because of an unknown layout	
			return new LinearLayoutManager(Context);
		}

		GridLayoutManager CreateGridLayout(GridItemsLayout gridItemsLayout)
		{
			var gridLayoutManager = new GridLayoutManager(Context, gridItemsLayout.Span,
				gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? LinearLayoutManager.Horizontal
					: LinearLayoutManager.Vertical,
				false);

			// Give the layout a way to determine that headers/footers span multiple rows/columns
			gridLayoutManager.SetSpanSizeLookup(new GridLayoutSpanSizeLookup(gridItemsLayout, this));

			return gridLayoutManager;
		}

		void OnElementChanged(ItemsView oldElement, ItemsView newElement)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, newElement));
			EffectUtilities.RegisterEffectControlProvider(this, oldElement, newElement);
			OnElementChanged(new ElementChangedEventArgs<ItemsView>(oldElement, newElement));
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<ItemsView> elementChangedEvent)
		{
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			ElementPropertyChanged?.Invoke(this, changedProperty);

			if (changedProperty.Is(Xamarin.Forms.ItemsView.ItemsSourceProperty))
			{
				UpdateItemsSource();
			}
			else if (changedProperty.Is(Xamarin.Forms.ItemsView.ItemTemplateProperty))
			{
				GetRecycledViewPool().Clear();
				UpdateAdapter();
			}
			else if (changedProperty.Is(VisualElement.BackgroundColorProperty))
			{
				UpdateBackgroundColor();
			}
			else if (changedProperty.Is(VisualElement.BackgroundProperty))
			{
				UpdateBackground();
			}
			else if (changedProperty.Is(VisualElement.FlowDirectionProperty))
			{
				UpdateFlowDirection();
			}
			else if (changedProperty.IsOneOf(Xamarin.Forms.ItemsView.EmptyViewProperty,
				Xamarin.Forms.ItemsView.EmptyViewTemplateProperty))
			{
				UpdateEmptyView();
			}
			else if (changedProperty.Is(Xamarin.Forms.ItemsView.HorizontalScrollBarVisibilityProperty))
			{
				UpdateHorizontalScrollBarVisibility();
			}
			else if (changedProperty.Is(Xamarin.Forms.ItemsView.VerticalScrollBarVisibilityProperty))
			{
				UpdateVerticalScrollBarVisibility();
			}
			else if (changedProperty.Is(Xamarin.Forms.ItemsView.ItemsUpdatingScrollModeProperty))
			{
				UpdateItemsUpdatingScrollMode();
			}
		}

		protected virtual void UpdateItemsSource()
		{
			if (ItemsView == null)
			{
				return;
			}

			// Stop watching the old adapter 
			var adapter = ItemsViewAdapter ?? GetAdapter();
			_emptyCollectionObserver.Stop(adapter);
			_itemsUpdateScrollObserver.Stop(adapter);

			UpdateAdapter();

			// Set up any properties which require observing data changes in the adapter
			UpdateItemsUpdatingScrollMode();

			UpdateEmptyView();
			AddOrUpdateScrollListener();
		}

		protected virtual TAdapter CreateAdapter()
		{
			return (TAdapter)new ItemsViewAdapter<TItemsView, TItemsViewSource>(ItemsView);
		}

		protected virtual void UpdateAdapter()
		{
			var oldItemViewAdapter = ItemsViewAdapter;

			ItemsViewAdapter = CreateAdapter();

			if (GetAdapter() != _emptyViewAdapter)
			{
				_emptyCollectionObserver.Stop(oldItemViewAdapter);
				_itemsUpdateScrollObserver.Stop(oldItemViewAdapter);

				SetAdapter(null);

				SwapAdapter(ItemsViewAdapter, true);
			}

			oldItemViewAdapter?.Dispose();
		}

		protected virtual void SetUpNewElement(TItemsView newElement)
		{
			if (newElement == null)
			{
				ItemsView = null;
				return;
			}

			ItemsView = newElement;

			ItemsView.PropertyChanged += OnElementPropertyChanged;

			// TODO hartez 2018/06/06 20:49:14 Review whether we can just do this in the constructor	
			if (Tracker == null)
			{
				Tracker = new VisualElementTracker(this);
			}

			this.EnsureId();

			UpdateItemsSource();

			ItemsLayout = GetItemsLayout();
			SetLayoutManager(SelectLayoutManager(ItemsLayout));

			UpdateSnapBehavior();
			UpdateBackgroundColor();
			UpdateBackground();
			UpdateFlowDirection();
			UpdateItemSpacing();

			UpdateHorizontalScrollBarVisibility();
			UpdateVerticalScrollBarVisibility();

			// Keep track of the ItemsLayout's property changes
			if (ItemsLayout != null)
			{
				ItemsLayout.PropertyChanged += LayoutPropertyChanged;
			}

			// Listen for ScrollTo requests
			ItemsView.ScrollToRequested += ScrollToRequested;

			AddOrUpdateScrollListener();
		}

		protected virtual RecyclerViewScrollListener<TItemsView, TItemsViewSource> CreateScrollListener()
		{
			return new RecyclerViewScrollListener<TItemsView, TItemsViewSource>(ItemsView, ItemsViewAdapter);
		}

		protected abstract IItemsLayout GetItemsLayout();

		protected virtual void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == ScrollBarVisibility.Default)
				_defaultVerticalScrollVisibility = VerticalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;

			var newVerticalScrollVisibility = ItemsView.VerticalScrollBarVisibility;

			if (newVerticalScrollVisibility == ScrollBarVisibility.Default)
				newVerticalScrollVisibility = _defaultVerticalScrollVisibility;

			VerticalScrollBarEnabled = newVerticalScrollVisibility == ScrollBarVisibility.Always;
		}

		protected virtual void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == ScrollBarVisibility.Default)
				_defaultHorizontalScrollVisibility =
					HorizontalScrollBarEnabled ? ScrollBarVisibility.Always : ScrollBarVisibility.Never;

			var newHorizontalScrollVisiblility = ItemsView.HorizontalScrollBarVisibility;

			if (newHorizontalScrollVisiblility == ScrollBarVisibility.Default)
				newHorizontalScrollVisiblility = _defaultHorizontalScrollVisibility;

			HorizontalScrollBarEnabled = newHorizontalScrollVisiblility == ScrollBarVisibility.Always;
		}

		protected virtual void TearDownOldElement(ItemsView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			// Stop listening for layout property changes
			if (ItemsLayout != null)
			{
				ItemsLayout.PropertyChanged -= LayoutPropertyChanged;
			}

			// Stop listening for property changes
			oldElement.PropertyChanged -= OnElementPropertyChanged;

			// Stop listening for ScrollTo requests
			oldElement.ScrollToRequested -= ScrollToRequested;

			RemoveScrollListener();

			if (ItemsViewAdapter != null)
			{
				// Stop watching for empty items or scroll adjustments
				_emptyCollectionObserver.Stop(ItemsViewAdapter);
				_itemsUpdateScrollObserver.Stop(ItemsViewAdapter);

				// Unhook whichever adapter is active
				SetAdapter(null);

				_emptyViewAdapter?.Dispose();
				ItemsViewAdapter?.Dispose();
			}

			if (_snapManager != null)
			{
				_snapManager.Dispose();
				_snapManager = null;
			}

			if (_itemDecoration != null)
			{
				RemoveItemDecoration(_itemDecoration);
			}
		}

		protected virtual void LayoutPropertyChanged(object sender, PropertyChangedEventArgs propertyChanged)
		{
			if (propertyChanged.Is(GridItemsLayout.SpanProperty))
			{
				if (GetLayoutManager() is GridLayoutManager gridLayoutManager)
				{
					gridLayoutManager.SpanCount = ((GridItemsLayout)ItemsLayout).Span;
				}
			}
			else if (propertyChanged.IsOneOf(Xamarin.Forms.ItemsLayout.SnapPointsTypeProperty, Xamarin.Forms.ItemsLayout.SnapPointsAlignmentProperty))
			{
				UpdateSnapBehavior();
			}
			else if (propertyChanged.IsOneOf(LinearItemsLayout.ItemSpacingProperty,
				GridItemsLayout.HorizontalItemSpacingProperty, GridItemsLayout.VerticalItemSpacingProperty))
			{
				UpdateItemSpacing();
			}
		}

		protected virtual void UpdateSnapBehavior()
		{
			_snapManager = GetSnapManager();

			_snapManager.UpdateSnapBehavior();
		}

		protected virtual SnapManager GetSnapManager()
		{
			if (_snapManager == null)
			{
				_snapManager = new SnapManager(ItemsLayout, this);
			}
			return _snapManager;
		}

		// TODO hartez 2018/08/09 09:30:17 Package up background color and flow direction providers so we don't have to re-implement them here	
		protected virtual void UpdateBackgroundColor(Color? color = null)
		{
			if (Element == null)
			{
				return;
			}

			SetBackgroundColor((color ?? Element.BackgroundColor).ToAndroid());
		}

		protected virtual void UpdateBackground(Brush brush = null)
		{
			if (Element == null)
				return;

			if (!(this is RecyclerView recyclerView))
				return;

			Brush background = Element.Background;

			recyclerView.UpdateBackground(background);
		}

		protected virtual void UpdateFlowDirection()
		{
			if (Element == null)
			{
				return;
			}

			this.UpdateFlowDirection(Element);

			ReconcileFlowDirectionAndLayout();
		}

		protected virtual void UpdateEmptyView()
		{
			if (ItemsViewAdapter == null || ItemsView == null)
			{
				return;
			}

			var emptyView = ItemsView?.EmptyView;
			var emptyViewTemplate = ItemsView?.EmptyViewTemplate;

			if (emptyView != null || emptyViewTemplate != null)
			{
				if (_emptyViewAdapter == null)
				{
					_emptyViewAdapter = new EmptyViewAdapter(ItemsView);
				}

				if (ItemsView is StructuredItemsView structuredItemsView)
				{
					_emptyViewAdapter.Header = structuredItemsView.Header;
					_emptyViewAdapter.HeaderTemplate = structuredItemsView.HeaderTemplate;

					_emptyViewAdapter.Footer = structuredItemsView.Footer;
					_emptyViewAdapter.FooterTemplate = structuredItemsView.FooterTemplate;
				}

				_emptyViewAdapter.EmptyView = emptyView;
				_emptyViewAdapter.EmptyViewTemplate = emptyViewTemplate;

				_emptyCollectionObserver.Start(ItemsViewAdapter);

				_emptyViewAdapter.NotifyDataSetChanged();
			}
			else
			{
				_emptyCollectionObserver.Stop(ItemsViewAdapter);
			}

			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateItemsUpdatingScrollMode()
		{
			if (ItemsViewAdapter == null || ItemsView == null)
			{
				return;
			}

			if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
			{
				// Keeping the current items in view is the default, so we don't need to watch for data changes
				_itemsUpdateScrollObserver.Stop(ItemsViewAdapter);
			}
			else
			{
				_itemsUpdateScrollObserver.Start(ItemsViewAdapter);
			}
		}

		protected virtual void ReconcileFlowDirectionAndLayout()
		{
			if (!(GetLayoutManager() is LinearLayoutManager linearLayoutManager))
			{
				return;
			}

			if (linearLayoutManager.CanScrollVertically())
			{
				return;
			}
		}

		protected virtual int DetermineTargetPosition(ScrollToRequestEventArgs args)
		{
			if (args.Mode == ScrollToMode.Position)
			{
				// TODO hartez 2018/08/28 15:40:03 Need to handle group indices here as well	
				return args.Index;
			}

			return ItemsViewAdapter.GetPositionForItem(args.Item);
		}

		protected virtual void UpdateItemSpacing()
		{
			if (ItemsLayout == null)
			{
				return;
			}

			if (_itemDecoration != null)
			{
				RemoveItemDecoration(_itemDecoration);
			}

			_itemDecoration = CreateSpacingDecoration(ItemsLayout);
			AddItemDecoration(_itemDecoration);
		}

		protected virtual ItemDecoration CreateSpacingDecoration(IItemsLayout itemsLayout)
		{
			return new SpacingItemDecoration(itemsLayout);
		}

		void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			(GetSnapManager()?.GetCurrentSnapHelper() as SingleSnapHelper)?.ResetCurrentTargetPosition();
			ScrollTo(args);
		}

		protected virtual void ScrollTo(ScrollToRequestEventArgs args)
		{
			if (ItemsView == null)
				return;

			var position = DetermineTargetPosition(args);

			if (args.IsAnimated)
			{
				ScrollHelper.AnimateScrollToPosition(position, args.ScrollToPosition);
			}
			else
			{
				ScrollHelper.JumpScrollToPosition(position, args.ScrollToPosition);
			}
		}

		protected virtual void UpdateLayoutManager()
		{
			ItemsLayout = GetItemsLayout();
			SetLayoutManager(SelectLayoutManager(ItemsLayout));

			UpdateFlowDirection();
			UpdateItemSpacing();
		}

		internal void UpdateEmptyViewVisibility()
		{
			if (ItemsViewAdapter == null)
			{
				return;
			}

			int itemCount = 0;
			if (ItemsView is StructuredItemsView itemsView)
			{
				if (itemsView.Header != null || itemsView.HeaderTemplate != null)
					itemCount++;
				if (itemsView.Footer != null || itemsView.FooterTemplate != null)
					itemCount++;
			}

			var showEmptyView = ItemsView?.EmptyView != null && ItemsViewAdapter.ItemCount == itemCount;

			var currentAdapter = GetAdapter();
			if (showEmptyView && currentAdapter != _emptyViewAdapter)
			{
				SwapAdapter(_emptyViewAdapter, true);

				// TODO hartez 2018/10/24 17:34:36 If this works, cache this layout manager as _emptyLayoutManager	
				SetLayoutManager(new LinearLayoutManager(Context));
			}
			else if (!showEmptyView && currentAdapter != ItemsViewAdapter)
			{
				SwapAdapter(ItemsViewAdapter, true);
				UpdateLayoutManager();
			}
		}

		internal void AdjustScrollForItemUpdate()
		{
			if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				ScrollTo(new ScrollToRequestEventArgs(GetLayoutManager().ItemCount, 0,
					Xamarin.Forms.ScrollToPosition.MakeVisible, true));
			}
			else if (ItemsView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
			{
				ScrollHelper.UndoNextScrollAdjustment();
			}
		}

		void AddOrUpdateScrollListener()
		{
			RemoveScrollListener();

			_recyclerViewScrollListener = CreateScrollListener();
			AddOnScrollListener(_recyclerViewScrollListener);
		}

		void RemoveScrollListener()
		{
			if (_recyclerViewScrollListener == null)
				return;

			_recyclerViewScrollListener.Dispose();
			ClearOnScrollListeners();
			_recyclerViewScrollListener = null;
		}
	}
}