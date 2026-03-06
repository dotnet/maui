using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using WASDKScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using WItemsView = Microsoft.UI.Xaml.Controls.ItemsView;
using WScrollPresenter = Microsoft.UI.Xaml.Controls.Primitives.ScrollPresenter;
using WScrollSnapPointsAlignment = Microsoft.UI.Xaml.Controls.Primitives.ScrollSnapPointsAlignment;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Handlers.Items2;
/// <summary>
/// Base handler for ItemsView controls on Windows, providing item source management,
/// layout, scrolling, snap points, empty views, headers, and footers.
/// </summary>
public abstract class ItemsViewHandler2<TItemsView> : ViewHandler<TItemsView, WItemsView> where TItemsView : ItemsView
{
	/// <summary>
	/// Alignment ratio that positions the item at the start (top/left) of the viewport.
	/// </summary>
	const double AlignToStart = 0.0;

	/// <summary>
	/// Alignment ratio that positions the item at the center of the viewport.
	/// </summary>
	const double AlignToCenter = 0.5;

	/// <summary>
	/// Alignment ratio that positions the item at the end (bottom/right) of the viewport.
	/// </summary>
	const double AlignToEnd = 1.0;

	CollectionViewSource? _collectionViewSource;
	IList? _itemsSource;
	ItemFactory? _itemFactory;

	FrameworkElement? _emptyView;
	View? _mauiEmptyView;
	bool _emptyViewDisplayed;
	double _previousHorizontalOffset;
	double _previousVerticalOffset;

	FrameworkElement? _footer;
	View? _mauiFooter;
	bool _footerDisplayed;

	FrameworkElement? _header;
	View? _mauiHeader;
	bool _headerDisplayed;

	ScrollingScrollBarVisibility? _defaultHorizontalScrollVisibility;
	ScrollingScrollBarVisibility? _defaultVerticalScrollVisibility;

	int _lastRemainingItemsThresholdIndex = -1;

	WeakNotifyPropertyChangedProxy? _layoutPropertyChangedProxy;
	PropertyChangedEventHandler? _layoutPropertyChanged;
	bool _isScrollingForItemsUpdate;
	int _pendingScrollToIndex = -1;
	RoutedEventHandler? _pendingLoadedHandler;

	// Tracks the last rendered width so SizeChanged only triggers layout recalc when width actually changes.
	double _lastGridLayoutWidth = -1;

	protected TItemsView ItemsView => VirtualView;
	protected TItemsView Element => VirtualView;

	protected abstract IItemsLayout Layout { get; }

	bool IsLayoutHorizontal => Layout switch
	{
		LinearItemsLayout linearLayout => linearLayout.Orientation == ItemsLayoutOrientation.Horizontal,
		GridItemsLayout gridLayout => gridLayout.Orientation == ItemsLayoutOrientation.Horizontal,
		_ => false
	};

	public ItemsViewHandler2() : base(ItemsViewMapper)
	{

	}

	public ItemsViewHandler2(PropertyMapper? mapper = null) : base(mapper ?? ItemsViewMapper)
	{

	}

	public static PropertyMapper<TItemsView, ItemsViewHandler2<TItemsView>> ItemsViewMapper = new(ViewMapper)
	{
		[Controls.ItemsView.ItemsSourceProperty.PropertyName] = MapItemsSource,
		[Controls.ItemsView.HorizontalScrollBarVisibilityProperty.PropertyName] = MapHorizontalScrollBarVisibility,
		[Controls.ItemsView.VerticalScrollBarVisibilityProperty.PropertyName] = MapVerticalScrollBarVisibility,
		[Controls.ItemsView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
		[Controls.ItemsView.EmptyViewProperty.PropertyName] = MapEmptyView,
		[Controls.ItemsView.EmptyViewTemplateProperty.PropertyName] = MapEmptyViewTemplate,
		[Controls.ItemsView.FlowDirectionProperty.PropertyName] = MapFlowDirection,
		[Controls.ItemsView.IsVisibleProperty.PropertyName] = MapIsVisible,
		[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode,
		[Controls.StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
		[Controls.StructuredItemsView.HeaderProperty.PropertyName] = MapHeader,
		[Controls.StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
		[Controls.StructuredItemsView.FooterProperty.PropertyName] = MapFooter,
		[Controls.StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
	};

	bool _scrollUpdatePending;

	public static void MapItemsSource(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateItemsSource();
	}

	// Intentionally empty: ItemsUpdatingScrollMode is handled during scroll events
	// via ApplyItemsUpdatingScrollMode, not as a direct property map.
	public static void MapItemsUpdatingScrollMode(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
	}

	public static void MapHorizontalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateHorizontalScrollBarVisibility();
	}

	public static void MapVerticalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateVerticalScrollBarVisibility();
	}

	public static void MapItemTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateItemsSource();
	}

	public static void MapEmptyView(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateEmptyView();
	}

	public static void MapEmptyViewTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateEmptyView();
	}

	public static void MapFlowDirection(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.PlatformView.UpdateFlowDirection(itemsView);
	}

	public static void MapIsVisible(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.PlatformView.UpdateVisibility(itemsView);
	}

	public static void MapItemsLayout(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateItemsLayout();
	}

	public static void MapHeader(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateHeader();
	}

	public static void MapHeaderTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateHeader();
	}

	public static void MapFooter(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateFooter();
	}

	public static void MapFooterTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
	{
		handler.UpdateFooter();
	}

	protected override WItemsView CreatePlatformView()
	{
		var itemsView = SelectListViewBase();

		return itemsView;
	}

	protected override void ConnectHandler(WItemsView platformView)
	{
		base.ConnectHandler(platformView);

		if (Layout is not null)
		{
			_layoutPropertyChanged ??= LayoutPropertyChanged;
			_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
		}
		else
		{
			_layoutPropertyChangedProxy?.Unsubscribe();
			_layoutPropertyChangedProxy = null;
		}

		// When WidthRequest is set with HorizontalOptions=Start the MAUI layout engine applies
		// the width constraint to the platform view AFTER initial construction. UniformGridLayout
		// may have already calculated column counts using the initial width (which can be 0).
		// Subscribing to SizeChanged and forcing UpdateLayout() ensures the grid recalculates
		// the correct number of columns once the constrained width is known.
		platformView.SizeChanged += OnPlatformSizeChanged;

		VirtualView.ScrollToRequested += ScrollToRequested;
		FindScrollViewer();
	}


	protected override void DisconnectHandler(WItemsView platformView)
	{
		// Phase 1: Unsubscribe ALL events first to prevent callbacks during cleanup
		if (_collectionViewSource?.Source is INotifyCollectionChanged incc)
		{
			incc.CollectionChanged -= ItemsChanged;
		}

		platformView.SizeChanged -= OnPlatformSizeChanged;

		if (platformView.ScrollView is not null)
		{
			platformView.ScrollView.ViewChanged -= ScrollViewChanged;
			platformView.ScrollView.PointerWheelChanged -= PointerScrollChanged;
			platformView.ScrollView.ExtentChanged -= ScrollViewExtentChanged;
		}

		// Unsubscribe pending Loaded handler if ScrollView wasn't found yet
		if (_pendingLoadedHandler is not null)
		{
			platformView.Loaded -= _pendingLoadedHandler;
			_pendingLoadedHandler = null;
		}

		if (_scrollUpdatePending)
		{
			platformView.LayoutUpdated -= OnLayoutUpdated;
			_scrollUpdatePending = false;
		}

		_layoutPropertyChangedProxy?.Unsubscribe();
		_layoutPropertyChangedProxy = null;

		if (VirtualView is not null)
		{
			VirtualView.ScrollToRequested -= ScrollToRequested;
		}

		// Safe subscription cleanup only — do NOT call CleanUpCollectionViewSource() here.
		//
		// CleanUpCollectionViewSource() performs two operations that trigger WinUI side effects
		// during teardown:
		//   1. Sets _collectionViewSource.Source = null → causes WinUI to recycle/release elements
		//   2. Calls PlatformView.GetChildren<ItemContentControl>() → triggers element enumeration
		//      while the ItemsRepeater is tearing down, leading to collection change notifications
		//      and potential InvalidOperationException.
		//
		// The CleanUp() methods below are safe because they ONLY unsubscribe event handlers
		// (CollectionChanged, group change notifications) without touching WinUI state.
		if (_collectionViewSource?.Source is ObservableItemTemplateCollection2 observableCollection)
		{
			observableCollection.CleanUp();
		}
		else if (_collectionViewSource?.Source is GroupedItemTemplateCollection2 groupedCollection)
		{
			groupedCollection.CleanUp();
		}

		_itemFactory?.CleanUp();
		_itemFactory = null;

		// Clean up logical children for empty view, header, and footer to prevent memory leaks
		if (_mauiEmptyView is not null && _emptyViewDisplayed)
		{
			ItemsView?.RemoveLogicalChild(_mauiEmptyView);
		}
		_mauiEmptyView = null;
		_emptyView = null;
		_emptyViewDisplayed = false;

		if (_mauiHeader is not null && _headerDisplayed)
		{
			ItemsView?.RemoveLogicalChild(_mauiHeader);
		}
		_mauiHeader = null;
		_header = null;
		_headerDisplayed = false;

		if (_mauiFooter is not null && _footerDisplayed)
		{
			ItemsView?.RemoveLogicalChild(_mauiFooter);
		}
		_mauiFooter = null;
		_footer = null;
		_footerDisplayed = false;

		_cachedSnapPointOffsets = null;
		_lastSnapPointExtent = 0;

		base.DisconnectHandler(platformView);
	}

	CollectionViewSource CreateCollectionViewSource()
	{
		var itemsSource = Element.ItemsSource;
		var itemTemplate = Element.ItemTemplate;

		if (itemTemplate is not null && itemsSource is not null)
		{
			if (ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory2.CreateGrouped(itemsSource, itemTemplate,
						groupableItemsView.GroupHeaderTemplate, groupableItemsView.GroupFooterTemplate,
						Element, mauiContext: MauiContext)
						,
					IsSourceGrouped = false
				};
			}
			else
			{
				var flattenedSource = itemsSource;
				if (itemsSource is not null && IsItemsSourceGrouped(itemsSource))
				{
					flattenedSource = FlattenGroupedItemsSource(itemsSource);
				}
				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory2.Create(flattenedSource, itemTemplate, Element, mauiContext: MauiContext),
					IsSourceGrouped = false
				};
			}
		}

		return new CollectionViewSource
		{
			Source = itemsSource,
			IsSourceGrouped = false
		};
	}

	bool IsItemsSourceGrouped(object itemsSource)
	{
		if (itemsSource is IEnumerable enumerable)
		{
			foreach (var item in enumerable)
			{
				if (item is IEnumerable && item is not string)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}

	IEnumerable FlattenGroupedItemsSource(IEnumerable groupedSource)
	{
		return groupedSource.Cast<object>().
			Where(group => group is IEnumerable && group is not string).
			SelectMany(group => ((IEnumerable)group).Cast<object>());
	}


	protected virtual void UpdateItemsSource()
	{
		if (PlatformView is null)
		{
			return;
		}

		CleanUpCollectionViewSource();

		_collectionViewSource = CreateCollectionViewSource();
		_itemsSource = _collectionViewSource?.Source as IList;

		if (_collectionViewSource?.Source is INotifyCollectionChanged incc)
		{
			incc.CollectionChanged += ItemsChanged;
		}

		PlatformView.ItemsSource = null;
		PlatformView.ItemsSource = _collectionViewSource?.View;


		if (VirtualView.ItemTemplate is not null)
		{
			_itemFactory = new ItemFactory(Element);
			PlatformView.ItemTemplate = _itemFactory;
		}
		else if (PlatformView.ItemTemplate is not null)
		{
			PlatformView.ItemTemplate = null;
		}

		UpdateEmptyViewVisibility();
	}

	void CleanUpCollectionViewSource()
	{
		// Clean up the recycle pool in the old ItemFactory to release pooled elements
		_itemFactory?.CleanUp();
		_itemFactory = null;

		if (_collectionViewSource is not null)
		{
			if (_collectionViewSource.Source is ObservableItemTemplateCollection2 observableItemTemplateCollection)
			{
				observableItemTemplateCollection.CleanUp();
			}
			else if (_collectionViewSource.Source is GroupedItemTemplateCollection2 groupedItemTemplateCollection)
			{
				groupedItemTemplateCollection.CleanUp();
			}

			if (_collectionViewSource.Source is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged -= ItemsChanged;
			}

			_collectionViewSource.Source = null;
			_collectionViewSource = null;
		}

		// Remove all children inside the ItemsSource
		if (VirtualView is not null && PlatformView is not null)
		{
			foreach (var item in PlatformView.GetChildren<ItemContentControl>())
			{
				if (item is not null)
				{
					var element = item.GetVisualElement();
					VirtualView.RemoveLogicalChild(element);
				}
			}
		}

		if (VirtualView?.ItemsSource is null && PlatformView is not null)
		{
			PlatformView.ItemsSource = null;
		}
	}

	void ItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		// Skip if handler is disconnected
		if (PlatformView is null || VirtualView is null)
		{
			return;
		}

		UpdateEmptyViewVisibility();

		if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset)
		{
			_lastRemainingItemsThresholdIndex = -1;
		}

		if (!_scrollUpdatePending && PlatformView is not null)
		{
			_scrollUpdatePending = true;

			PlatformView.LayoutUpdated += OnLayoutUpdated;
		}
	}
	void OnLayoutUpdated(object? s, object args)
	{
		if (PlatformView is null)
		{
			_scrollUpdatePending = false;
			return;
		}

		PlatformView.LayoutUpdated -= OnLayoutUpdated;
		_scrollUpdatePending = false;
		ApplyItemsUpdatingScrollMode();
	}

	void ApplyItemsUpdatingScrollMode()
	{
		if (PlatformView is null || VirtualView is null)
		{
			return;
		}

		if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepScrollOffset)
		{
			return;
		}

		if (_itemsSource is null || _itemsSource.Count == 0)
		{
			return;
		}

		// Get the actual count from the CollectionView's view
		var viewCount = _collectionViewSource?.View?.Count ?? 0;
		if (viewCount == 0)
		{
			return;
		}

		// Force layout update to ensure items are properly positioned
		PlatformView.UpdateLayout();

		if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
		{
			_isScrollingForItemsUpdate = true;

			// Use dispatcher to ensure the scroll happens after layout is fully complete
			VirtualView.Dispatcher.Dispatch(() =>
			{
				if (PlatformView is null)
				{
					return;
				}

				// Keeps the first item in the list displayed when new items are added.
				PlatformView.StartBringItemIntoView(0, new BringIntoViewOptions()
				{
					AnimationDesired = false,
					VerticalAlignmentRatio = AlignToStart,
					HorizontalAlignmentRatio = AlignToStart
				});
			});
		}
		else if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
		{
			_isScrollingForItemsUpdate = true;

			// Use the view count to ensure we're scrolling to the correct last item
			var lastIndex = viewCount - 1;
			if (lastIndex >= 0)
			{
				_pendingScrollToIndex = lastIndex;

				// Use dispatcher to ensure the scroll happens after layout is fully complete
				VirtualView.Dispatcher.Dispatch(() =>
				{
					if (PlatformView is null || _pendingScrollToIndex < 0)
					{
						_pendingScrollToIndex = -1;
						return;
					}

					var currentViewCount = _collectionViewSource?.View?.Count ?? 0;
					var scrollIndex = Math.Min(_pendingScrollToIndex, currentViewCount - 1);

					if (scrollIndex >= 0)
					{
						PlatformView.StartBringItemIntoView(scrollIndex, new BringIntoViewOptions()
						{
							AnimationDesired = false,
							VerticalAlignmentRatio = AlignToEnd,
							HorizontalAlignmentRatio = AlignToEnd
						});
					}

					_pendingScrollToIndex = -1;
				});
			}
		}
	}

	MauiItemsView SelectListViewBase()
	{
		var itemsView = new MauiItemsView()
		{
			Layout = CreateItemsLayout()
		};

		if (IsLayoutHorizontal)
		{
			ScrollViewer.SetHorizontalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Enabled);
			ScrollViewer.SetHorizontalScrollBarVisibility(itemsView, WASDKScrollBarVisibility.Visible);

			ScrollViewer.SetVerticalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Disabled);
			ScrollViewer.SetVerticalScrollBarVisibility(itemsView, WASDKScrollBarVisibility.Disabled);

		}
		else
		{
			ScrollViewer.SetHorizontalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Disabled);
			ScrollViewer.SetHorizontalScrollBarVisibility(itemsView, WASDKScrollBarVisibility.Disabled);

			ScrollViewer.SetVerticalScrollMode(itemsView, UI.Xaml.Controls.ScrollMode.Enabled);
			ScrollViewer.SetVerticalScrollBarVisibility(itemsView, WASDKScrollBarVisibility.Visible);
		}

		itemsView.SetLayoutOrientation(IsLayoutHorizontal);

		// Constrain the ItemsRepeater cross-axis early so the first layout pass
		// uses the correct available width, preventing items from stretching to
		// fill a wider-than-intended parent before WidthRequest takes effect.
		if (Layout is GridItemsLayout)
		{
			double crossAxisSize = IsLayoutHorizontal
				? VirtualView?.HeightRequest ?? -1
				: VirtualView?.WidthRequest ?? -1;

			if (crossAxisSize >= 0)
				itemsView.SetItemsRepeaterCrossAxisMaxSize(IsLayoutHorizontal, crossAxisSize);
		}

		return itemsView;
	}

	protected void UpdateItemsLayout()
	{
		FindScrollViewer();

		_defaultHorizontalScrollVisibility = null;
		_defaultVerticalScrollVisibility = null;

		// Unsubscribe from the old layout's property changes and subscribe to the new layout
		_layoutPropertyChangedProxy?.Unsubscribe();
		_layoutPropertyChangedProxy = null;

		if (Layout is not null)
		{
			_layoutPropertyChanged ??= LayoutPropertyChanged;
			_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
		}

		PlatformView.Layout = CreateItemsLayout();

		// Reset cached cross-axis size so OnPlatformSizeChanged re-evaluates column count
		// after the layout type or orientation changes.
		_lastGridLayoutWidth = -1;

		// Update header/footer orientation
		if (PlatformView is MauiItemsView mauiItemsView)
		{
			if (IsLayoutHorizontal)
			{
				ScrollViewer.SetHorizontalScrollMode(mauiItemsView, UI.Xaml.Controls.ScrollMode.Enabled);
				ScrollViewer.SetVerticalScrollMode(mauiItemsView, UI.Xaml.Controls.ScrollMode.Disabled);
			}
			else
			{
				ScrollViewer.SetHorizontalScrollMode(mauiItemsView, UI.Xaml.Controls.ScrollMode.Disabled);
				ScrollViewer.SetVerticalScrollMode(mauiItemsView, UI.Xaml.Controls.ScrollMode.Enabled);
			}

			// Re-apply the cross-axis constraint for the new layout. Use WidthRequest
			// (or HeightRequest for horizontal) if set, otherwise clear the constraint.
			if (Layout is GridItemsLayout)
			{
				double crossAxisSize = IsLayoutHorizontal
					? VirtualView?.HeightRequest ?? -1
					: VirtualView?.WidthRequest ?? -1;

				mauiItemsView.SetItemsRepeaterCrossAxisMaxSize(IsLayoutHorizontal,
					crossAxisSize >= 0 ? crossAxisSize : double.PositiveInfinity);
			}
			else
			{
				mauiItemsView.SetItemsRepeaterCrossAxisMaxSize(IsLayoutHorizontal, double.PositiveInfinity);
			}

			mauiItemsView.SetLayoutOrientation(IsLayoutHorizontal);
		}

		UpdateVerticalScrollBarVisibility();
		UpdateHorizontalScrollBarVisibility();
		UpdateEmptyView();
	}

	UI.Xaml.Controls.Layout CreateItemsLayout()
	{
		switch (Layout)
		{
			case GridItemsLayout gridItemsLayout:
				var gridLayout = CreateGridView(gridItemsLayout);

				// Pre-compute MinItemWidth/Height when WidthRequest/HeightRequest is
				// explicitly set, so UniformGridLayout uses the correct column/row count
				// from the very first layout pass — before SizeChanged fires.
				// Without this, ItemsStretch=Fill can stretch items to a large cached
				// effectiveItemWidth during the initial unconstrained pass, causing the
				// column formula to yield fewer columns than Span once the constrained
				// width takes effect.
				if (VirtualView is not null)
				{
					double crossAxisSize = IsLayoutHorizontal
						? VirtualView.HeightRequest
						: VirtualView.WidthRequest;

					if (crossAxisSize >= 0)
					{
						ApplyMinItemSizeForSpan(gridLayout, gridItemsLayout, crossAxisSize);
					}
				}

				return gridLayout;
			case LinearItemsLayout listItemsLayout:
				return CreateStackLayout(listItemsLayout);
			default:
				break;
		}

		throw new NotImplementedException("The layout is not implemented");
	}

	static UI.Xaml.Controls.StackLayout CreateStackLayout(LinearItemsLayout listItemsLayout)
	{
		return new UI.Xaml.Controls.StackLayout()
		{
			Orientation = listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? Orientation.Horizontal : Orientation.Vertical,
			Spacing = listItemsLayout.ItemSpacing
		};
	}

	static UniformGridLayout CreateGridView(GridItemsLayout gridItemsLayout)
	{
		return new UniformGridLayout()
		{
			Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? Orientation.Vertical : Orientation.Horizontal,
			MaximumRowsOrColumns = gridItemsLayout.Span,
			MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing,
			MinRowSpacing = gridItemsLayout.VerticalItemSpacing,
			ItemsStretch = UniformGridLayoutItemsStretch.Fill,
			ItemsJustification = UniformGridLayoutItemsJustification.Start,
		};
	}

	/// <summary>
	/// Computes and sets <c>MinItemWidth</c> (or <c>MinItemHeight</c>) on the given
	/// <see cref="UniformGridLayout"/> so that the column/row count formula always
	/// yields the configured <see cref="GridItemsLayout.Span"/>.
	/// </summary>
	void ApplyMinItemSizeForSpan(UniformGridLayout uniformGrid, GridItemsLayout gridItemsLayout, double crossAxisSize)
	{
		if (crossAxisSize <= 0 || double.IsNaN(crossAxisSize) || double.IsInfinity(crossAxisSize))
			return;

		int span = gridItemsLayout.Span;
		bool isHorizontal = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
		double spacing = isHorizontal
			? gridItemsLayout.VerticalItemSpacing
			: gridItemsLayout.HorizontalItemSpacing;

		// Floor to avoid floating-point precision issues in UniformGridLayout's column
		// count formula: min(Span, floor((available+spacing) / (effectiveItemWidth+spacing))).
		// Without flooring, a MinItemWidth of e.g. 63.999 can cause floor(239.999/79.999)=2
		// instead of the expected 3.
		double itemSize = Math.Max(1, Math.Floor((crossAxisSize - (span - 1) * spacing) / span));

		if (isHorizontal)
			uniformGrid.MinItemHeight = itemSize;
		else
			uniformGrid.MinItemWidth = itemSize;
	}

	/// <summary>
	/// Handles platform view size changes to ensure <see cref="UniformGridLayout"/> recalculates
	/// the correct column (or row) count after the MAUI layout engine applies a <c>WidthRequest</c>
	/// constraint. Without this, the grid may show fewer columns than <see cref="GridItemsLayout.Span"/>
	/// because the initial layout pass can see a zero or stale available width.
	/// </summary>
	void OnPlatformSizeChanged(object sender, UI.Xaml.SizeChangedEventArgs e)
	{
		if (Layout is not GridItemsLayout gridItemsLayout || PlatformView is null || VirtualView is null)
			return;

		// When no explicit size is set, UniformGridLayout receives the correct available
		// size naturally from its parent — no forced re-layout needed.
		bool hasExplicitCrossAxisSize = IsLayoutHorizontal
			? VirtualView.HeightRequest >= 0
			: VirtualView.WidthRequest >= 0;

		if (!hasExplicitCrossAxisSize)
			return;

		double newCrossAxisSize = IsLayoutHorizontal ? e.NewSize.Height : e.NewSize.Width;
		if (newCrossAxisSize == _lastGridLayoutWidth)
			return;

		_lastGridLayoutWidth = newCrossAxisSize;

		// Recreating the UniformGridLayout resets its internal EffectiveItemWidth cache.
		//
		// UniformGridLayout computes column count as:
		//   min(Span, floor((available + spacing) / (effectiveItemWidth + spacing)))
		//
		// After any layout pass it caches the realized item width as effectiveItemWidth.
		// If the first pass used a wrong available width (e.g. the view was initially
		// measured before WidthRequest was applied), items may be stretched to the wrong
		// size (e.g. 104px for 2 columns). On the next pass the cached 104px feeds back
		// into the formula and re-produces 2 columns — a stable fixed point that
		// InvalidateMeasure() alone cannot break.
		//
		// A fresh UniformGridLayout starts with effectiveItemWidth=0, so the formula
		// reduces to min(Span, floor((available+spacing)/spacing)), giving the correct
		// column count for the actual available width.
		//
		// Additionally, we set MaxWidth (or MaxHeight for horizontal layouts) directly on
		// the ItemsRepeater. This ensures UniformGridLayout always sees the correct
		// available size even when the parent StackPanel passes a larger constraint during
		// an intermediate layout pass (e.g., when FrameworkElement.Width is still NaN).
		if (PlatformView is MauiItemsView mauiItemsView)
			mauiItemsView.SetItemsRepeaterCrossAxisMaxSize(IsLayoutHorizontal, newCrossAxisSize);

		var newLayout = CreateGridView(gridItemsLayout);
		ApplyMinItemSizeForSpan(newLayout, gridItemsLayout, newCrossAxisSize);
		PlatformView.Layout = newLayout;
	}

	void FindScrollViewer()
	{
		if (PlatformView is null)
		{
			return;
		}

		if (PlatformView.ScrollView is not null)
		{
			OnScrollViewerFound();
			return;
		}

		// Unsubscribe any previous pending Loaded handler
		if (_pendingLoadedHandler is not null)
		{
			PlatformView.Loaded -= _pendingLoadedHandler;
		}

		_pendingLoadedHandler = (sender, e) =>
		{
			var lv = (WItemsView)sender;
			lv.Loaded -= _pendingLoadedHandler;
			_pendingLoadedHandler = null;
			FindScrollViewer();
		};

		PlatformView.Loaded += _pendingLoadedHandler;
	}

	void OnScrollViewerFound()
	{
		if (PlatformView is null || PlatformView.ScrollView is null)
		{
			return;
		}

		PlatformView.ScrollView.ViewChanged -= ScrollViewChanged;
		PlatformView.ScrollView.PointerWheelChanged -= PointerScrollChanged;
		PlatformView.ScrollView.ExtentChanged -= ScrollViewExtentChanged;

		PlatformView.ScrollView.ViewChanged += ScrollViewChanged;
		PlatformView.ScrollView.PointerWheelChanged += PointerScrollChanged;
		PlatformView.ScrollView.ExtentChanged += ScrollViewExtentChanged;

		UpdateVerticalScrollBarVisibility();
		UpdateHorizontalScrollBarVisibility();
		UpdateSnapPoints();
	}

	void LayoutPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == GridItemsLayout.SpanProperty.PropertyName)
		{
			UpdateItemsLayoutSpan();
		}
		else if (e.PropertyName == GridItemsLayout.HorizontalItemSpacingProperty.PropertyName || e.PropertyName == GridItemsLayout.VerticalItemSpacingProperty.PropertyName)
		{
			UpdateItemsLayoutItemSpacing();
		}
		else if (e.PropertyName == LinearItemsLayout.ItemSpacingProperty.PropertyName)
		{
			UpdateItemsLayoutItemSpacing();
		}
		else if (e.PropertyName == nameof(ItemsLayout.SnapPointsType) ||
				 e.PropertyName == nameof(ItemsLayout.SnapPointsAlignment))
		{
			UpdateSnapPoints();
		}
		else if (e.PropertyName == nameof(ItemsLayout.Orientation))
		{
			// Orientation change swaps which axis is "cross-axis", so the cached size is
			// no longer valid — reset it so OnPlatformSizeChanged re-evaluates correctly.
			_lastGridLayoutWidth = -1;
		}
	}

	void UpdateItemsLayoutSpan()
	{
		if (PlatformView.Layout is UniformGridLayout listViewLayout &&
			Layout is GridItemsLayout gridItemsLayout)
		{
			listViewLayout.MaximumRowsOrColumns = gridItemsLayout.Span;

			// Recompute MinItemWidth/MinItemHeight for the new span so the
			// column count formula yields the correct result.
			if (_lastGridLayoutWidth > 0)
			{
				ApplyMinItemSizeForSpan(listViewLayout, gridItemsLayout, _lastGridLayoutWidth);
			}
			else if (PlatformView.ActualWidth > 0 || PlatformView.ActualHeight > 0)
			{
				double crossAxisSize = IsLayoutHorizontal ? PlatformView.ActualHeight : PlatformView.ActualWidth;
				ApplyMinItemSizeForSpan(listViewLayout, gridItemsLayout, crossAxisSize);
			}
		}
	}

	void UpdateItemsLayoutItemSpacing()
	{
		if (PlatformView.Layout is UniformGridLayout listViewLayout &&
			Layout is GridItemsLayout gridItemsLayout)
		{
			listViewLayout.MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing;
			listViewLayout.MinRowSpacing = gridItemsLayout.VerticalItemSpacing;

			// Spacing changes affect the MinItemWidth/Height formula:
			//   itemSize = (crossAxis - (span-1) * spacing) / span
			// Recompute so the column/row count remains correct.
			if (_lastGridLayoutWidth > 0)
			{
				ApplyMinItemSizeForSpan(listViewLayout, gridItemsLayout, _lastGridLayoutWidth);
			}
			else if (VirtualView is not null)
			{
				double crossAxisSize = IsLayoutHorizontal
					? VirtualView.HeightRequest
					: VirtualView.WidthRequest;

				if (crossAxisSize >= 0)
				{
					ApplyMinItemSizeForSpan(listViewLayout, gridItemsLayout, crossAxisSize);
				}
			}
		}
		else if (PlatformView.Layout is UI.Xaml.Controls.StackLayout stackLayout &&
			Layout is LinearItemsLayout linearItemsLayout)
		{
			stackLayout.Spacing = linearItemsLayout.ItemSpacing;
		}
	}

	double _lastSnapPointExtent;
	List<double>? _cachedSnapPointOffsets;
	SnapPointsType _cachedSnapPointsType;
	SnapPointsAlignment _cachedSnapPointsAlignment;

	void ScrollViewExtentChanged(Microsoft.UI.Xaml.Controls.ScrollView sender, object args)
	{
		// Only recalculate snap points when the extent actually changes
		// to avoid redundant work during intermediate layout passes.
		var currentExtent = IsLayoutHorizontal ? sender.ExtentWidth : sender.ExtentHeight;
		if (currentExtent != _lastSnapPointExtent)
		{
			_lastSnapPointExtent = currentExtent;
			UpdateSnapPoints();
		}
	}

	void UpdateSnapPoints()
	{
		if (PlatformView?.ScrollView?.ScrollPresenter is not WScrollPresenter scrollPresenter)
		{
			return;
		}

		if (Layout is not ItemsLayout itemsLayout)
		{
			return;
		}

		var snapPointsType = itemsLayout.SnapPointsType;

		if (snapPointsType == SnapPointsType.None)
		{
			scrollPresenter.HorizontalSnapPoints.Clear();
			scrollPresenter.VerticalSnapPoints.Clear();
			_cachedSnapPointOffsets = null;
			_cachedSnapPointsType = SnapPointsType.None;
			return;
		}

		// NOTE: MandatorySingle is treated identically to Mandatory.
		// WinUI's ScrollPresenter snap point object model does not natively support
		// "single item per gesture" limiting. Both Mandatory and MandatorySingle
		// will snap to the nearest snap point, but a fling gesture can skip past
		// multiple items. A true MandatorySingle implementation would require
		// intercepting scroll inertia to limit travel distance, which is not
		// supported by the current ScrollPresenter API.

		var itemCount = _collectionViewSource?.View?.Count ?? 0;
		if (itemCount == 0)
		{
			scrollPresenter.HorizontalSnapPoints.Clear();
			scrollPresenter.VerticalSnapPoints.Clear();
			_cachedSnapPointOffsets = null;
			return;
		}

		var snapAlignment = itemsLayout.SnapPointsAlignment;

		// Compute offsets from realized item containers.
		// Uses actual measured sizes to handle variable-size items,
		// complex templates, and async image loading.
		double runningOffset = 0;
		double spacing = 0;
		var newOffsets = new List<double>();

		if (Layout is LinearItemsLayout linearLayout)
		{
			spacing = linearLayout.ItemSpacing;
		}
		else if (Layout is GridItemsLayout gridLayout)
		{
			spacing = IsLayoutHorizontal ? gridLayout.HorizontalItemSpacing : gridLayout.VerticalItemSpacing;
		}

		foreach (var container in PlatformView.GetChildren<ItemContainer>())
		{
			if (container is null)
			{
				continue;
			}

			double itemExtent = IsLayoutHorizontal ? container.ActualWidth : container.ActualHeight;
			if (itemExtent <= 0)
			{
				continue;
			}

			newOffsets.Add(runningOffset);
			runningOffset += itemExtent + spacing;
		}

		if (newOffsets.Count == 0)
		{
			// Items not yet realized; snap points will be applied
			// when ExtentChanged fires after items are laid out.
			return;
		}

		// Skip rebuild if snap points haven't changed — avoids redundant
		// WinUI collection clears and allocations on every ExtentChanged.
		if (_cachedSnapPointOffsets is not null &&
			_cachedSnapPointsType == snapPointsType &&
			_cachedSnapPointsAlignment == snapAlignment &&
			_cachedSnapPointOffsets.Count == newOffsets.Count &&
			_cachedSnapPointOffsets.SequenceEqual(newOffsets))
		{
			return;
		}

		// Rebuild snap points — clear both axes to handle orientation changes
		var alignment = GetScrollSnapPointsAlignment(snapAlignment);

		scrollPresenter.HorizontalSnapPoints.Clear();
		scrollPresenter.VerticalSnapPoints.Clear();

		var snapPoints = IsLayoutHorizontal
			? scrollPresenter.HorizontalSnapPoints
			: scrollPresenter.VerticalSnapPoints;

		foreach (var offset in newOffsets)
		{
			snapPoints.Add(new Microsoft.UI.Xaml.Controls.Primitives.ScrollSnapPoint(
				offset, alignment));
		}

		_cachedSnapPointOffsets = newOffsets;
		_cachedSnapPointsType = snapPointsType;
		_cachedSnapPointsAlignment = snapAlignment;
	}

	static WScrollSnapPointsAlignment GetScrollSnapPointsAlignment(SnapPointsAlignment alignment)
	{
		return alignment switch
		{
			SnapPointsAlignment.Start => WScrollSnapPointsAlignment.Near,
			SnapPointsAlignment.Center => WScrollSnapPointsAlignment.Center,
			SnapPointsAlignment.End => WScrollSnapPointsAlignment.Far,
			_ => WScrollSnapPointsAlignment.Near,
		};
	}

	void UpdateEmptyView()
	{
		if (Element is null || PlatformView is null)
		{
			return;
		}

		var emptyView = Element.EmptyView;
		var emptyViewTemplate = Element.EmptyViewTemplate;

		// Clear empty view
		if (emptyView is null && emptyViewTemplate is null)
		{
			if (_emptyViewDisplayed)
			{
				if (_emptyView != null && PlatformView is IEmptyView ev)
					ev.EmptyViewVisibility = WVisibility.Collapsed;

				if (_mauiEmptyView != null)
					ItemsView.RemoveLogicalChild(_mauiEmptyView);

				_emptyViewDisplayed = false;
			}

			_emptyView = null;
			_mauiEmptyView = null;
			(PlatformView as IEmptyView)?.SetEmptyView(null, null);
			return;
		}

		if (emptyViewTemplate is DataTemplateSelector && emptyView is null)
		{
			_emptyView = null;
			_mauiEmptyView = null;
			(PlatformView as IEmptyView)?.SetEmptyView(null, null);
			return;
		}

		// Resolve empty view
		var oldMauiEmptyView = _mauiEmptyView;
		_emptyView = emptyViewTemplate != null
			? ItemsViewExtensions.RealizeEmptyViewTemplate(emptyView, emptyViewTemplate, MauiContext!, ref _mauiEmptyView)
			: emptyView switch
			{
				string text => new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = text
				},
				View view => ItemsViewExtensions.RealizeEmptyView(view, MauiContext!, ref _mauiEmptyView),
				_ => ItemsViewExtensions.RealizeEmptyViewTemplate(emptyView, null, MauiContext!, ref _mauiEmptyView)
			};

		// Remove old logical child before adding the new one to prevent leak
		if (oldMauiEmptyView is not null && _emptyViewDisplayed)
		{
			ItemsView.RemoveLogicalChild(oldMauiEmptyView);
			_emptyViewDisplayed = false;
		}

		(PlatformView as IEmptyView)?.SetEmptyView(_emptyView, _mauiEmptyView);
		UpdateEmptyViewVisibility();
	}

	void UpdateEmptyViewVisibility()
	{
		if (PlatformView is null || VirtualView is null)
		{
			return;
		}

		// Check both CollectionViewSource.View and the underlying _itemsSource
		// After a Reset action, CollectionViewSource.View.Count might not be immediately updated
		bool isEmpty = (_collectionViewSource?.View?.Count ?? 0) == 0 && (_itemsSource?.Count ?? 0) == 0;

		if (isEmpty)
		{
			if (_mauiEmptyView is not null && !_emptyViewDisplayed)
			{
				if (ItemsView.EmptyViewTemplate is null)
				{
					ItemsView.AddLogicalChild(_mauiEmptyView);
				}
			}

			if (_emptyView is not null && PlatformView is IEmptyView emptyView)
			{
				emptyView.EmptyViewVisibility = WVisibility.Visible;
			}

			_emptyViewDisplayed = true;
		}
		else
		{
			if (_emptyViewDisplayed)
			{
				if (_emptyView is not null && PlatformView is IEmptyView emptyView)
				{
					emptyView.EmptyViewVisibility = WVisibility.Collapsed;
				}

				ItemsView.RemoveLogicalChild(_mauiEmptyView);
			}

			_emptyViewDisplayed = false;
		}
	}

	void UpdateHeader()
	{
		if (Element is not StructuredItemsView structuredItemsView || PlatformView is null)
		{
			return;
		}

		var header = structuredItemsView.Header;
		var headerTemplate = structuredItemsView.HeaderTemplate;

		// Hide header only if both Header and HeaderTemplate are null
		if (header is null && headerTemplate is null)
		{
			if (_headerDisplayed)
			{
				if (PlatformView is MauiItemsView mauiItemsView)
				{
					mauiItemsView.HeaderVisibility = WVisibility.Collapsed;
				}

				if (_mauiHeader is not null)
				{
					ItemsView.RemoveLogicalChild(_mauiHeader);
				}
				_headerDisplayed = false;
			}
			return;
		}

		// Save old logical child reference before realization overwrites _mauiHeader via ref
		var oldMauiHeader = _mauiHeader;

		// If HeaderTemplate is set, use it regardless of header value
		if (headerTemplate is not null)
		{
			var bindingContext = header ?? (object)string.Empty;
			_header = ItemsViewExtensions.RealizeHeaderFooterTemplate(bindingContext, headerTemplate, MauiContext!, ref _mauiHeader);
		}
		else if (header is not null)
		{
			_header = header switch
			{
				string text => new TextBlock
				{
					Text = text,
					Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10),
					TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
				},
				View view => ItemsViewExtensions.RealizeHeaderFooterView(view, MauiContext!, ref _mauiHeader),
				_ => new TextBlock
				{
					Text = header.ToString(),
					Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10),
					TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
				}
			};
		}
		else
		{
			// This shouldn't happen due to null check above, but handle it safely
			return;
		}

		// When the header is a MAUI View (direct or from template), HorizontalOptions/VerticalOptions
		// are layout hints for the parent MAUI layout. Since the realized view is placed inside a WinUI
		// ContentControl (not a MAUI layout), we must manually map these options
		// to the platform view's HorizontalAlignment/VerticalAlignment.
		if (_mauiHeader is not null && _header is FrameworkElement headerElement)
		{
			headerElement.UpdateHorizontalOptions(_mauiHeader);
			headerElement.UpdateVerticalOptions(_mauiHeader);
		}

		if (PlatformView is MauiItemsView platformItemsView && _header is not null)
		{
			platformItemsView.SetHeader(_header, _mauiHeader);
			platformItemsView.HeaderVisibility = WVisibility.Visible;
		}

		// Remove old logical child before adding the new one to prevent leak
		if (oldMauiHeader is not null && _headerDisplayed)
		{
			ItemsView.RemoveLogicalChild(oldMauiHeader);
		}

		if (_mauiHeader is not null)
		{
			ItemsView.AddLogicalChild(_mauiHeader);
		}

		_headerDisplayed = true;
	}

	void UpdateFooter()
	{
		if (Element is not StructuredItemsView structuredItemsView || PlatformView is null)
		{
			return;
		}

		var footer = structuredItemsView.Footer;
		var footerTemplate = structuredItemsView.FooterTemplate;

		// Hide footer only if both Footer and FooterTemplate are null
		if (footer is null && footerTemplate is null)
		{
			if (_footerDisplayed)
			{
				if (PlatformView is MauiItemsView mauiItemsView)
				{
					mauiItemsView.FooterVisibility = WVisibility.Collapsed;
				}

				if (_mauiFooter is not null)
				{
					ItemsView.RemoveLogicalChild(_mauiFooter);
				}
				_footerDisplayed = false;
			}
			return;
		}

		// Save old logical child reference before realization overwrites _mauiFooter via ref
		var oldMauiFooter = _mauiFooter;

		// If FooterTemplate is set, use it regardless of footer value
		if (footerTemplate is not null)
		{
			var bindingContext = footer ?? (object)string.Empty;
			_footer = ItemsViewExtensions.RealizeHeaderFooterTemplate(bindingContext, footerTemplate, MauiContext!, ref _mauiFooter);
		}
		else if (footer is not null)
		{
			_footer = footer switch
			{
				string text => new TextBlock
				{
					Text = text,
					Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
					TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
				},
				View view => ItemsViewExtensions.RealizeHeaderFooterView(view, MauiContext!, ref _mauiFooter),
				_ => new TextBlock
				{
					Text = footer.ToString(),
					Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0),
					TextAlignment = Microsoft.UI.Xaml.TextAlignment.Center,
				}
			};
		}
		else
		{
			// This shouldn't happen due to null check above, but handle it safely
			return;
		}

		// When the footer is a MAUI View (direct or from template), HorizontalOptions/VerticalOptions
		// are layout hints for the parent MAUI layout. Since the realized view is placed inside a WinUI
		// ContentControl (not a MAUI layout), we must manually map these options
		// to the platform view's HorizontalAlignment/VerticalAlignment.
		if (_mauiFooter is not null && _footer is FrameworkElement footerElement)
		{
			footerElement.UpdateHorizontalOptions(_mauiFooter);
			footerElement.UpdateVerticalOptions(_mauiFooter);
		}

		if (PlatformView is MauiItemsView platformItemsView && _footer is not null)
		{
			platformItemsView.SetFooter(_footer, _mauiFooter);
			platformItemsView.FooterVisibility = WVisibility.Visible;
		}

		// Remove old logical child before adding the new one to prevent leak
		if (oldMauiFooter is not null && _footerDisplayed)
		{
			ItemsView.RemoveLogicalChild(oldMauiFooter);
		}

		if (_mauiFooter is not null)
		{
			ItemsView.AddLogicalChild(_mauiFooter);
		}

		_footerDisplayed = true;
	}

	void UpdateVerticalScrollBarVisibility()
	{
		var scrollBarVisibility = Element.VerticalScrollBarVisibility;
		var scrollView = PlatformView.ScrollView;

		if (scrollView is null)
			return;

		if (scrollBarVisibility != ScrollBarVisibility.Default)
		{
			// If the value is changing to anything other than the default, record the default
			if (_defaultVerticalScrollVisibility is null)
			{
				_defaultVerticalScrollVisibility = scrollView.VerticalScrollBarVisibility;
			}
		}

		if (_defaultVerticalScrollVisibility is null)
		{
			// If the default has never been recorded, then this has never been set to anything but the
			// default value; there's nothing to do.
			return;
		}

		switch (scrollBarVisibility)
		{
			case ScrollBarVisibility.Always:
				scrollView.VerticalScrollBarVisibility = ScrollingScrollBarVisibility.Visible;
				break;
			case ScrollBarVisibility.Never:
				scrollView.VerticalScrollBarVisibility = ScrollingScrollBarVisibility.Hidden;
				break;
			case ScrollBarVisibility.Default:
				scrollView.VerticalScrollBarVisibility = _defaultVerticalScrollVisibility.Value;
				break;
		}

	}

	void UpdateHorizontalScrollBarVisibility()
	{
		var scrollBarVisibility = Element.HorizontalScrollBarVisibility;
		var scrollView = PlatformView.ScrollView;

		if (scrollView is null)
			return;

		if (scrollBarVisibility != ScrollBarVisibility.Default)
		{
			// If the value is changing to anything other than the default, record the default
			if (_defaultHorizontalScrollVisibility is null)
			{
				_defaultHorizontalScrollVisibility = scrollView.HorizontalScrollBarVisibility;
			}
		}

		if (_defaultHorizontalScrollVisibility is null)
		{
			// If the default has never been recorded, then this has never been set to anything but the
			// default value; there's nothing to do.
			return;
		}

		switch (scrollBarVisibility)
		{
			case ScrollBarVisibility.Always:
				scrollView.HorizontalScrollBarVisibility = ScrollingScrollBarVisibility.Visible;
				break;
			case ScrollBarVisibility.Never:
				scrollView.HorizontalScrollBarVisibility = ScrollingScrollBarVisibility.Hidden;
				break;
			case ScrollBarVisibility.Default:
				scrollView.HorizontalScrollBarVisibility = _defaultHorizontalScrollVisibility.Value;
				break;
		}

	}

	void PointerScrollChanged(object sender, PointerRoutedEventArgs e)
	{
		if (PlatformView?.ScrollView is null)
			return;

		if (PlatformView.ScrollView.ComputedHorizontalScrollMode == ScrollingScrollMode.Enabled)
		{
			PlatformView.ScrollView.AddScrollVelocity(new(e.GetCurrentPoint(PlatformView.ScrollView).Properties.MouseWheelDelta, 0), null);
		}
	}

	void ScrollViewChanged(UI.Xaml.Controls.ScrollView sender, object args)
	{
		if (PlatformView?.ScrollView?.ScrollPresenter is null)
			return;

		HandleScroll(PlatformView.ScrollView.ScrollPresenter);
	}

	void HandleScroll(WScrollPresenter scrollViewer)
	{
		if (_isScrollingForItemsUpdate)
		{
			_isScrollingForItemsUpdate = false;
			return;
		}

		var itemsViewScrolledEventArgs = new ItemsViewScrolledEventArgs
		{
			HorizontalOffset = scrollViewer.HorizontalOffset,
			HorizontalDelta = scrollViewer.HorizontalOffset - _previousHorizontalOffset,
			VerticalOffset = scrollViewer.VerticalOffset,
			VerticalDelta = scrollViewer.VerticalOffset - _previousVerticalOffset,
		};

		_previousHorizontalOffset = scrollViewer.HorizontalOffset;
		_previousVerticalOffset = scrollViewer.VerticalOffset;

		bool advancing = true;
		switch (Layout)
		{
			case LinearItemsLayout linearItemsLayout:
				advancing = linearItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? itemsViewScrolledEventArgs.HorizontalDelta > 0
					: itemsViewScrolledEventArgs.VerticalDelta > 0;
				break;
			case GridItemsLayout gridItemsLayout:
				advancing = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
					? itemsViewScrolledEventArgs.HorizontalDelta > 0
					: itemsViewScrolledEventArgs.VerticalDelta > 0;
				break;
			default:
				break;
		}

		itemsViewScrolledEventArgs = ComputeVisibleIndexes(itemsViewScrolledEventArgs, advancing);

		Element.SendScrolled(itemsViewScrolledEventArgs);

		var remainingItemsThreshold = Element.RemainingItemsThreshold;
		if (_collectionViewSource != null && remainingItemsThreshold > -1)
		{
			var itemsRemaining = _collectionViewSource.View.Count - 1 - itemsViewScrolledEventArgs.LastVisibleItemIndex;

			if (itemsRemaining <= remainingItemsThreshold)
			{
				if (itemsViewScrolledEventArgs.LastVisibleItemIndex > _lastRemainingItemsThresholdIndex)
				{
					_lastRemainingItemsThresholdIndex = itemsViewScrolledEventArgs.LastVisibleItemIndex;
					Element.SendRemainingItemsThresholdReached();
				}
				// When scrolling backward within the threshold zone, keep the
				// high-water mark — don't reset, to avoid duplicate event fires.
				// The reset happens in the outer else (when leaving the zone entirely)
				// or in ItemsChanged (when new items are added).
			}
			else
			{
				// Reset when scrolling away from the threshold zone so the
				// event can re-fire when the user scrolls back.
				_lastRemainingItemsThresholdIndex = -1;
			}
		}
	}

	ItemsViewScrolledEventArgs ComputeVisibleIndexes(ItemsViewScrolledEventArgs args, bool advancing)
	{
		var (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex) = GetVisibleIndexes(advancing);

		args.FirstVisibleItemIndex = firstVisibleItemIndex;
		args.CenterItemIndex = centerItemIndex;
		args.LastVisibleItemIndex = lastVisibleItemIndex;

		return args;
	}

	(int firstVisibleItemIndex, int lastVisibleItemIndex, int centerItemIndex) GetVisibleIndexes(bool advancing)
	{
		int firstVisibleItemIndex = -1;
		int lastVisibleItemIndex = -1;

		if (PlatformView is null)
		{
			return (firstVisibleItemIndex, lastVisibleItemIndex, -1);
		}

		PlatformView.TryGetItemIndex(0, 0, out firstVisibleItemIndex);
		PlatformView.TryGetItemIndex(1, 1, out lastVisibleItemIndex);

		double center = (lastVisibleItemIndex + firstVisibleItemIndex) / 2.0;
		int centerItemIndex = advancing ? (int)Math.Ceiling(center) : (int)Math.Floor(center);

		return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
	}

	void ScrollToRequested(object? sender, ScrollToRequestEventArgs args)
	{
		// Use base.PlatformView to avoid InvalidOperationException
		if (base.PlatformView is not WItemsView platformView)
		{
			return;
		}

		int index;

		// Handle grouped scrolling by position
		if (args.Mode == ScrollToMode.Position &&
			ItemsView is GroupableItemsView groupableItemsView &&
			groupableItemsView.IsGrouped &&
			args.GroupIndex >= 0)
		{
			index = FindGroupedItemIndex(args.GroupIndex, args.Index);
		}
		else if (args.Mode == ScrollToMode.Element)
		{
			index = FindItemIndex(args.Item);
		}
		else
		{
			// Non-grouped position-based scroll
			index = args.Index;
		}

		// Validate index is within bounds
		var itemCount = _collectionViewSource?.View?.Count ?? 0;
		if (index < 0 || index >= itemCount)
		{
			return;
		}

		double offset = AlignToStart;
		switch (args.ScrollToPosition)
		{
			case ScrollToPosition.Start:
				offset = AlignToStart;
				break;
			case ScrollToPosition.Center:
				offset = AlignToCenter;
				break;
			case ScrollToPosition.End:
				offset = AlignToEnd;
				break;
		}

		platformView.StartBringItemIntoView(index, new BringIntoViewOptions()
		{
			AnimationDesired = args.IsAnimated,
			VerticalAlignmentRatio = offset,
			HorizontalAlignmentRatio = offset
		});
	}

	/// <summary>
	/// Finds the flat index in the flattened grouped collection for the specified group and item index.
	/// </summary>
	int FindGroupedItemIndex(int groupIndex, int itemIndex)
	{
		if (_collectionViewSource is null)
		{
			return -1;
		}

		if (ItemsView is not GroupableItemsView groupableItemsView)
		{
			return -1;
		}

		var itemsSource = groupableItemsView.ItemsSource;
		if (itemsSource is null)
		{
			return -1;
		}

		var hasGroupHeader = groupableItemsView.GroupHeaderTemplate is not null;
		var hasGroupFooter = groupableItemsView.GroupFooterTemplate is not null;

		int flatIndex = 0;
		int currentGroupIndex = 0;

		foreach (var group in itemsSource)
		{
			if (group is IList groupList)
			{
				if (currentGroupIndex == groupIndex)
				{
					// Found the target group
					if (hasGroupHeader)
					{
						flatIndex++; // Skip group header
					}

					// Check if itemIndex is within bounds of this group
					if (itemIndex < 0 || itemIndex >= groupList.Count)
					{
						return -1;
					}

					// Return the calculated flat index
					return flatIndex + itemIndex;
				}

				// Count items in this group to move to next group
				if (hasGroupHeader)
				{
					flatIndex++;
				}
				flatIndex += groupList.Count;
				if (hasGroupFooter)
				{
					flatIndex++;
				}
			}

			currentGroupIndex++;
		}

		// Group index not found
		return -1;
	}

	/// <summary>
	/// Finds the index of an item in the collection view by searching through the flattened list.
	/// Used for non-grouped ScrollToMode.Element requests.
	/// </summary>
	int FindItemIndex(object item)
	{
		if (_collectionViewSource is null)
		{
			return -1;
		}

		for (int index = 0; index < _collectionViewSource.View.Count; index++)
		{
			var viewItem = _collectionViewSource.View[index];

			// Check for ItemTemplateContext (non-grouped templated items)
			if (viewItem is ItemTemplateContext pair)
			{
				if (Equals(pair.Item, item))
				{
					return index;
				}
			}
			// Check for ItemTemplateContext2 (grouped templated items)
			else if (viewItem is ItemTemplateContext2 pair2)
			{
				// Skip headers and footers, only match actual items
				if (!pair2.IsHeader && !pair2.IsFooter && Equals(pair2.Item, item))
				{
					return index;
				}
			}
			// Check for non-templated items (direct equality)
			else if (Equals(viewItem, item))
			{
				return index;
			}
		}

		return -1;
	}
}