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
using WRect = Windows.Foundation.Rect;
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

	ScrollViewer? _scrollViewer;

	int _lastRemainingItemsThresholdIndex = -1;

	WeakNotifyPropertyChangedProxy? _layoutPropertyChangedProxy;
	PropertyChangedEventHandler? _layoutPropertyChanged;
	bool _isScrollingForItemsUpdate;
	SnapPointsType _snapPointsType;
	SnapPointsAlignment _snapPointsAlignment;
	bool _isSnapping; // guard against re-entrant snap from our own ChangeView
	int _pendingScrollToIndex = -1;
	RoutedEventHandler? _pendingLoadedHandler;
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
		// Only invalidate cache if:
		// 1. ItemSizingStrategy is MeasureFirstItem
		// 2. Control is already loaded (runtime template change, not initial load)
		if (handler is CollectionViewHandler2 cvHandler && itemsView is CollectionView cv &&
		cv.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem && itemsView.IsLoadedOnPlatform())
		{
			cvHandler.InvalidateFirstItemSize();
		}

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
		handler.UpdateEmptyViewVisibility();
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

		if (_scrollViewer is not null)
		{
			_scrollViewer.ViewChanged -= ScrollViewChanged;
			_scrollViewer.ViewChanging -= OnScrollViewerViewChanging;
			_scrollViewer = null;
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
		_mauiEmptyView?.Handler?.DisconnectHandler();
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

		base.DisconnectHandler(platformView);
	}

	CollectionViewSource CreateCollectionViewSource()
	{
		var itemsSource = Element.ItemsSource;
		var itemTemplate = Element.ItemTemplate;

		if (itemTemplate is not null && itemsSource is not null)
		{
			if (ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped
				&& IsItemsSourceGrouped(itemsSource))
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

		// When IsGrouped=true but no ItemTemplate, flatten the grouped source so that
		// the WinUI ItemsView (which doesn't support native grouping) can display the
		// individual items. Without this, the raw grouped collections are set as the source
		// but ItemsView cannot render them, resulting in an empty list.
		if (itemsSource is not null &&
			ItemsView is GroupableItemsView groupable && groupable.IsGrouped &&
			IsItemsSourceGrouped(itemsSource))
		{
			return new CollectionViewSource
			{
				Source = FlattenGroupedItemsSource(itemsSource),
				IsSourceGrouped = false
			};
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
			SelectMany(group => ((IEnumerable)group).Cast<object>()).
			ToList();
	}


	protected virtual void UpdateItemsSource()
	{
		if (PlatformView is null)
		{
			return;
		}

		// Save reference before cleanup nulls the field
		var oldCollectionViewSource = _collectionViewSource;

		// Safe cleanup: unsubscribe events, clean up collections and factory.
		CleanUpCollectionViewSource();

		// Create the new CollectionViewSource
		_collectionViewSource = CreateCollectionViewSource();
		_itemsSource = _collectionViewSource?.Source as IList;

		if (_collectionViewSource?.Source is INotifyCollectionChanged incc)
		{
			incc.CollectionChanged += ItemsChanged;
		}

		// Set up the new ItemTemplate
		if (VirtualView.ItemTemplate is not null)
		{
			_itemFactory = new ItemFactory(Element);
			PlatformView.ItemTemplate = _itemFactory;
		}
		else if (PlatformView.ItemTemplate is not null)
		{
			PlatformView.ItemTemplate = null;
		}

		PlatformView.ItemsSource = _collectionViewSource?.View;

		// Now safely null the old Source — PlatformView no longer references it,
		// so the CollectionChanged(Reset) it fires won't cause WinUI side effects.
		if (oldCollectionViewSource is not null)
		{
			oldCollectionViewSource.Source = null;
		}

		_scrollViewer?.ChangeView(0, 0, null, disableAnimation: true);
		_previousHorizontalOffset = 0;
		_previousVerticalOffset = 0;

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
			// CV2 uses ItemContainer + ElementWrapper, not ItemContentControl.
			// Check both types to handle CV1 and CV2 handlers correctly.
			foreach (var itemContainer in PlatformView.GetChildren<ItemContainer>())
			{
				if (itemContainer?.Child is ElementWrapper wrapper && wrapper.VirtualView is View mauiView)
				{
					VirtualView.RemoveLogicalChild(mauiView);
				}
			}

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

		if (!PlatformView.IsLoaded)
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
				if (PlatformView is null || !PlatformView.IsLoaded)
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
					if (PlatformView is null || !PlatformView.IsLoaded || _pendingScrollToIndex < 0)
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
		return itemsView;
	}

	protected void UpdateItemsLayout()
	{
		FindScrollViewer();

		// Unsubscribe from the old layout's property changes and subscribe to the new layout
		_layoutPropertyChangedProxy?.Unsubscribe();
		_layoutPropertyChangedProxy = null;

		if (Layout is not null)
		{
			_layoutPropertyChanged ??= LayoutPropertyChanged;
			_layoutPropertyChangedProxy = new WeakNotifyPropertyChangedProxy(Layout, _layoutPropertyChanged);
		}

		PlatformView.Layout = CreateItemsLayout();

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

			mauiItemsView.SetLayoutOrientation(IsLayoutHorizontal);
		}

		UpdateVerticalScrollBarVisibility();
		UpdateHorizontalScrollBarVisibility();
		UpdateEmptyView();
		UpdateSnapPoints();
	}

	UI.Xaml.Controls.Layout CreateItemsLayout()
	{
		switch (Layout)
		{
			case GridItemsLayout gridItemsLayout:
				return CreateGridView(gridItemsLayout);
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

	UniformGridLayout CreateGridView(GridItemsLayout gridItemsLayout)
	{
		// Use custom layout for grouped items to handle headers/footers spanning full width
		bool isGrouped = ItemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped;

		UniformGridLayout layout = isGrouped
			? new GroupableUniformGridLayout()
			: new UniformGridLayout();

		layout.Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
			? Orientation.Vertical : Orientation.Horizontal;
		layout.MaximumRowsOrColumns = gridItemsLayout.Span;
		layout.MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing;
		layout.MinRowSpacing = gridItemsLayout.VerticalItemSpacing;
		bool noTemplate = ItemsView.ItemTemplate is null;
		bool isMeasureAllItems = ItemsView is CollectionView cv && cv.ItemSizingStrategy == ItemSizingStrategy.MeasureAllItems;

		layout.ItemsStretch = (noTemplate && isMeasureAllItems)
			? UniformGridLayoutItemsStretch.Uniform
			: UniformGridLayoutItemsStretch.Fill;
		layout.ItemsJustification = UniformGridLayoutItemsJustification.Start;


		ApplyMinItemSizeForSpan(layout, gridItemsLayout);
		return layout;
	}

	/// <summary>
	/// Computes and applies MinItemWidth/MinItemHeight to work around a WinUI UniformGridLayout bug
	/// where two internal formulas disagree on items-per-line when ItemsStretch=Fill:
	///   - GetItemsPerLine uses: floor((available + spacing) / (minWidth + spacing))
	///   - CalculateExtraPixelsInLine uses: floor(available / (minWidth + spacing))
	/// The more restrictive formula sees fewer items and inflates the effective width, causing
	/// fewer columns than requested. By using floor(crossAxisSize / span) - spacing as MinItemWidth,
	/// both formulas agree on the correct number of columns.
	/// See: https://github.com/microsoft/microsoft-ui-xaml/blob/main/src/controls/dev/Repeater/UniformGridLayout.cpp
	/// </summary>
	void ApplyMinItemSizeForSpan(UniformGridLayout layout, GridItemsLayout gridItemsLayout)
	{
		// Only apply when an explicit cross-axis size is set on the CollectionView
		bool isHorizontal = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal;
		double crossAxisSize = isHorizontal
			? VirtualView.HeightRequest
			: VirtualView.WidthRequest;

		if (crossAxisSize <= 0 || double.IsNaN(crossAxisSize))
			return;

		int span = gridItemsLayout.Span;
		double spacing = isHorizontal
			? gridItemsLayout.VerticalItemSpacing
			: gridItemsLayout.HorizontalItemSpacing;

		// Formula: floor(crossAxisSize / span) - spacing
		// This ensures both WinUI internal formulas agree on the correct number of columns.
		double minItemSize = Math.Floor(crossAxisSize / span) - spacing;
		if (minItemSize <= 0)
			return;

		if (isHorizontal)
			layout.MinItemHeight = minItemSize;
		else
			layout.MinItemWidth = minItemSize;
	}

	void FindScrollViewer()
	{
		if (PlatformView is null)
		{
			return;
		}

		var scrollViewer = PlatformView.GetFirstDescendant<ScrollViewer>();
		if (scrollViewer is not null)
		{
			OnScrollViewerFound(scrollViewer);
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

	void OnScrollViewerFound(ScrollViewer scrollViewer)
	{
		if (_scrollViewer == scrollViewer)
		{
			return;
		}

		if (_scrollViewer is not null)
		{
			_scrollViewer.ViewChanged -= ScrollViewChanged;
			_scrollViewer.ViewChanging -= OnScrollViewerViewChanging;
		}

		_scrollViewer = scrollViewer;
		_scrollViewer.ViewChanged += ScrollViewChanged;
		_scrollViewer.ViewChanging += OnScrollViewerViewChanging;

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
	}

	void UpdateItemsLayoutSpan()
	{
		if (PlatformView.Layout is UniformGridLayout listViewLayout &&
			Layout is GridItemsLayout gridItemsLayout)
		{
			listViewLayout.MaximumRowsOrColumns = gridItemsLayout.Span;
			ApplyMinItemSizeForSpan(listViewLayout, gridItemsLayout);
		}
	}

	void UpdateItemsLayoutItemSpacing()
	{
		if (PlatformView.Layout is UniformGridLayout listViewLayout &&
			Layout is GridItemsLayout gridItemsLayout)
		{
			listViewLayout.MinColumnSpacing = gridItemsLayout.HorizontalItemSpacing;
			listViewLayout.MinRowSpacing = gridItemsLayout.VerticalItemSpacing;
			ApplyMinItemSizeForSpan(listViewLayout, gridItemsLayout);
		}
		else if (PlatformView.Layout is UI.Xaml.Controls.StackLayout stackLayout &&
			Layout is LinearItemsLayout linearItemsLayout)
		{
			stackLayout.Spacing = linearItemsLayout.ItemSpacing;
		}
	}

	void UpdateSnapPoints()
	{
		if (_scrollViewer is null || Layout is not ItemsLayout itemsLayout)
		{
			return;
		}

		_snapPointsType = itemsLayout.SnapPointsType;
		_snapPointsAlignment = itemsLayout.SnapPointsAlignment;

		// Do NOT set native SnapPointsType on the ScrollViewer.
		// The StackPanel's built-in IScrollSnapPointsInfo provides snap points
		// at its direct children (Header, ItemsRepeater, EmptyView, Footer),
		// not at individual items. Instead, we handle snapping manually in
		// OnScrollViewerViewChanging, similar to how iOS/Mac uses TargetContentOffset.
		_scrollViewer.HorizontalSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType.None;
		_scrollViewer.VerticalSnapPointsType = Microsoft.UI.Xaml.Controls.SnapPointsType.None;
	}

	/// <summary>
	/// Handles the ScrollViewer's ViewChanging event, which fires DURING deceleration
	/// before the scroll settles. This is the Windows equivalent of iOS's
	/// <c>TargetContentOffset(proposedContentOffset, scrollingVelocity)</c>.
	///
	/// We compute the nearest item to the proposed scroll destination and redirect
	/// the scroll to snap to that item's boundary using ChangeView.
	/// </summary>
	void OnScrollViewerViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
	{
		if (_scrollViewer is null || PlatformView is null ||
			_snapPointsType == SnapPointsType.None || _isSnapping)
		{
			return;
		}

		// e.NextView is where the ScrollViewer is headed.
		// e.FinalView is where it will end up if we don't intervene.
		// We use FinalView as the "proposed content offset" (like Mac's proposedContentOffset).
		var finalView = e.FinalView;
		bool isHorizontal = IsLayoutHorizontal;
		double proposedOffset = isHorizontal ? finalView.HorizontalOffset : finalView.VerticalOffset;
		double currentOffset = isHorizontal ? _scrollViewer.HorizontalOffset : _scrollViewer.VerticalOffset;
		double viewportSize = isHorizontal ? _scrollViewer.ViewportWidth : _scrollViewer.ViewportHeight;
		double maxOffset = isHorizontal ? _scrollViewer.ScrollableWidth : _scrollViewer.ScrollableHeight;

		// Determine scroll direction (like Mac's scrollingVelocity)
		double scrollDelta = proposedOffset - currentOffset;

		// Find the best snap target among visible item containers
		// This mirrors Mac's SnapHelpers.FindBestSnapCandidate + AdjustContentOffset
		double? snapTarget = null;

		if (_snapPointsType == SnapPointsType.MandatorySingle)
		{
			// MandatorySingle: like Mac's ScrollSingle — advance exactly one item
			// from the CURRENT position, based on scroll direction
			snapTarget = FindSingleSnapTarget(isHorizontal, currentOffset, viewportSize, maxOffset, scrollDelta);
		}
		else
		{
			// Mandatory: like Mac's standard TargetContentOffset —
			// find the nearest item to the PROPOSED destination
			snapTarget = FindNearestSnapTarget(isHorizontal, proposedOffset, viewportSize, maxOffset);
		}

		if (snapTarget.HasValue)
		{
			double distance = Math.Abs(snapTarget.Value - proposedOffset);
			if (distance > 1.0) // Only intervene if snap target differs by more than 1px
			{
				_isSnapping = true;
				if (isHorizontal)
				{
					_scrollViewer.ChangeView(snapTarget.Value, null, null, disableAnimation: false);
				}
				else
				{
					_scrollViewer.ChangeView(null, snapTarget.Value, null, disableAnimation: false);
				}
			}
		}
	}

	/// <summary>
	/// Finds the item nearest to the proposed offset and returns the scroll offset
	/// that would align it per the current SnapPointsAlignment.
	/// Equivalent to Mac's <c>SnapHelpers.FindBestSnapCandidate + AdjustContentOffset</c>.
	/// </summary>
	double? FindNearestSnapTarget(bool isHorizontal, double proposedOffset, double viewportSize, double maxOffset)
	{
		double? bestTarget = null;
		double bestDistance = double.MaxValue;

		foreach (var container in PlatformView.GetChildren<ItemContainer>())
		{
			if (container is null || container.Visibility != WVisibility.Visible)
				continue;

			var target = ComputeSnapTarget(container, isHorizontal, proposedOffset, viewportSize, maxOffset);
			if (target is null)
				continue;

			double distance = Math.Abs(target.Value - proposedOffset);
			if (distance < bestDistance)
			{
				bestDistance = distance;
				bestTarget = target;
			}
		}

		return bestTarget;
	}

	/// <summary>
	/// Finds the next item in the scroll direction from the current position.
	/// Equivalent to Mac's <c>ScrollSingle</c> — only advances one item per gesture.
	/// </summary>
	double? FindSingleSnapTarget(bool isHorizontal, double currentOffset, double viewportSize, double maxOffset, double scrollDelta)
	{
		// First, find the item currently aligned to the snap position
		double? currentAligned = FindNearestSnapTarget(isHorizontal, currentOffset, viewportSize, maxOffset);

		if (currentAligned is null)
			return null;

		if (Math.Abs(scrollDelta) < 1.0)
			return currentAligned; // No meaningful scroll, stay put

		// Collect all snap targets sorted by offset
		var allTargets = new List<double>();
		foreach (var container in PlatformView.GetChildren<ItemContainer>())
		{
			if (container is null || container.Visibility != WVisibility.Visible)
				continue;

			var target = ComputeSnapTarget(container, isHorizontal, currentOffset, viewportSize, maxOffset);
			if (target.HasValue)
				allTargets.Add(target.Value);
		}

		allTargets.Sort();

		if (allTargets.Count == 0)
			return null;

		// Find current index in sorted targets
		int currentIndex = 0;
		double minDist = double.MaxValue;
		for (int i = 0; i < allTargets.Count; i++)
		{
			double d = Math.Abs(allTargets[i] - currentAligned.Value);
			if (d < minDist)
			{
				minDist = d;
				currentIndex = i;
			}
		}

		// Advance exactly one step based on scroll direction (like Mac's FindNextItem)
		int span = 1;
		if (Layout is GridItemsLayout gridLayout)
			span = gridLayout.Span;

		int nextIndex;
		if (scrollDelta > 0)
			nextIndex = Math.Min(currentIndex + span, allTargets.Count - 1);
		else
			nextIndex = Math.Max(currentIndex - span, 0);

		return allTargets[nextIndex];
	}

	/// <summary>
	/// Computes the scroll offset that would align the given container
	/// per the current SnapPointsAlignment. Returns null if the container
	/// position cannot be determined.
	///
	/// This mirrors Mac's <c>SnapHelpers.GetViewportOffset</c> logic:
	/// - Start: item's leading edge aligns with viewport's leading edge
	/// - Center: item's center aligns with viewport's center
	/// - End: item's trailing edge aligns with viewport's trailing edge
	/// </summary>
	double? ComputeSnapTarget(ItemContainer container, bool isHorizontal, double referenceOffset, double viewportSize, double maxOffset)
	{
		// TransformToVisual requires the element to be in the visual tree and have a valid layout.
		// Without these checks, TransformToVisual throws — guard instead of catching.
		if (!container.IsLoaded || _scrollViewer is null)
			return null;

		double itemSize = isHorizontal ? container.ActualWidth : container.ActualHeight;
		if (itemSize <= 0)
			return null;

		var transform = container.TransformToVisual(_scrollViewer);
		var point = transform.TransformPoint(new global::Windows.Foundation.Point(0, 0));

		// Container position relative to viewport
		double itemStart = isHorizontal ? point.X : point.Y;

		// Convert viewport-relative position to absolute content offset,
		// then compute the target scroll offset per alignment.
		// currentScrollOffset + itemStart = absolute position of item's leading edge.
		double currentScrollOffset = isHorizontal
			? _scrollViewer.HorizontalOffset
			: _scrollViewer.VerticalOffset;

		double absoluteItemStart = currentScrollOffset + itemStart;

		double snapTarget = _snapPointsAlignment switch
		{
			// Mac: viewport.Left - itemFrame.Left (delta), applied to proposed offset
			SnapPointsAlignment.Start => absoluteItemStart,
			// Mac: center(viewport) - center(itemFrame)
			SnapPointsAlignment.Center => absoluteItemStart + itemSize / 2 - viewportSize / 2,
			// Mac: viewport.Right - itemFrame.Right
			SnapPointsAlignment.End => absoluteItemStart + itemSize - viewportSize,
			_ => absoluteItemStart,
		};

		// Clamp to valid scroll range
		return Math.Max(0, Math.Min(snapTarget, maxOffset));
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
			// Always set EmptyViewVisibility to Collapsed when items exist,
			// regardless of _emptyViewDisplayed state. This handles the case
			// where the empty view was made visible on the platform but
			// _emptyViewDisplayed got out of sync (e.g., during Collapsed→Visible
			// transitions where the template wasn't yet applied when the flag was set).
			if (_emptyView is not null && PlatformView is IEmptyView emptyView)
			{
				emptyView.EmptyViewVisibility = WVisibility.Collapsed;
			}

			if (_emptyViewDisplayed)
			{
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

		ItemsViewExtensions.ApplyMauiLayoutProperties(_mauiHeader, _header);

		if (PlatformView is MauiItemsView platformItemsView && _header is not null)
		{
			platformItemsView.SetHeader(_header);
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

		ItemsViewExtensions.ApplyMauiLayoutProperties(_mauiFooter, _footer);

		if (PlatformView is MauiItemsView platformItemsView && _footer is not null)
		{
			platformItemsView.SetFooter(_footer);
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
		if (_scrollViewer is null)
			return;

		// For horizontal layout, vertical is the cross-axis — must stay Disabled
		// so ScrollContentPresenter constrains content height to viewport
		if (IsLayoutHorizontal)
		{
			_scrollViewer.VerticalScrollBarVisibility = WASDKScrollBarVisibility.Disabled;
			return;
		}

		// Vertical is the scroll axis — respect the user's MAUI setting
		_scrollViewer.VerticalScrollBarVisibility = Element.VerticalScrollBarVisibility switch
		{
			ScrollBarVisibility.Always => WASDKScrollBarVisibility.Visible,
			ScrollBarVisibility.Never => WASDKScrollBarVisibility.Hidden,
			_ => WASDKScrollBarVisibility.Auto,
		};
	}

	void UpdateHorizontalScrollBarVisibility()
	{
		if (_scrollViewer is null)
			return;

		// For vertical layout, horizontal is the cross-axis — must stay Disabled
		// so ScrollContentPresenter constrains content width to viewport
		if (!IsLayoutHorizontal)
		{
			_scrollViewer.HorizontalScrollBarVisibility = WASDKScrollBarVisibility.Disabled;
			return;
		}

		// Horizontal is the scroll axis — respect the user's MAUI setting
		_scrollViewer.HorizontalScrollBarVisibility = Element.HorizontalScrollBarVisibility switch
		{
			ScrollBarVisibility.Always => WASDKScrollBarVisibility.Visible,
			ScrollBarVisibility.Never => WASDKScrollBarVisibility.Hidden,
			_ => WASDKScrollBarVisibility.Auto,
		};
	}

	void ScrollViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
	{
		if (_scrollViewer is null)
			return;

		// Reset the snapping guard when the scroll animation (including our ChangeView)
		// fully settles. This prevents re-entrant snap during our own snap animation.
		if (!e.IsIntermediate)
			_isSnapping = false;

		HandleScroll(_scrollViewer);
	}

	void HandleScroll(ScrollViewer scrollViewer)
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

		if (PlatformView is null || _scrollViewer is null)
		{
			return (firstVisibleItemIndex, lastVisibleItemIndex, -1);
		}

		// MauiItemsView uses a custom ControlTemplate with a ScrollViewer wrapping
		// a StackPanel > ItemsRepeater (not the default ItemsView template with ScrollView).
		// Snap points are handled manually in OnScrollViewerViewChanging (like iOS TargetContentOffset).
		// WinUI's TryGetItemIndex relies on its internal ScrollView part (PART_ScrollView),
		// which doesn't exist in our template — so it always returns -1.
		// Instead, walk the realized ItemContainer children and check which ones
		// are visible within the ScrollViewer viewport, similar to CV1's fallback approach.
		bool isHorizontal = IsLayoutHorizontal;
		int index = 0;

		foreach (var container in PlatformView.GetChildren<ItemContainer>())
		{
			if (container is null)
			{
				index++;
				continue;
			}

			if (IsElementVisibleInScrollViewer(container, _scrollViewer, isHorizontal))
			{
				if (firstVisibleItemIndex == -1)
				{
					firstVisibleItemIndex = index;
				}

				lastVisibleItemIndex = index;
			}

			index++;
		}

		double center = (lastVisibleItemIndex + firstVisibleItemIndex) / 2.0;
		int centerItemIndex = advancing ? (int)Math.Ceiling(center) : (int)Math.Floor(center);

		return (firstVisibleItemIndex, lastVisibleItemIndex, centerItemIndex);
	}

	/// <summary>
	/// Checks whether a UI element is visible within the scroll viewer's viewport.
	/// Uses coordinate transformation to compare element bounds against the viewport.
	/// </summary>
	static bool IsElementVisibleInScrollViewer(FrameworkElement element, ScrollViewer scrollViewer, bool isHorizontal)
	{
		if (element.Visibility != WVisibility.Visible)
		{
			return false;
		}

		try
		{
			var transform = element.TransformToVisual(scrollViewer);
			var elementBounds = transform.TransformBounds(
				new WRect(0, 0, element.ActualWidth, element.ActualHeight));
			var viewportBounds = new WRect(
				0, 0, scrollViewer.ActualWidth, scrollViewer.ActualHeight);

			if (isHorizontal)
			{
				return elementBounds.Left < viewportBounds.Right && elementBounds.Right > viewportBounds.Left;
			}
			else
			{
				return elementBounds.Top < viewportBounds.Bottom && elementBounds.Bottom > viewportBounds.Top;
			}
		}
		catch
		{
			// TransformToVisual can throw if the element is not in the visual tree
			return false;
		}
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
		else if (args.Mode == ScrollToMode.Element && args.Group is not null)
		{
			index = FindGroupedItemByElement(args.Item, args.Group);
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

		if (platformView.IsLoaded)
		{
			PerformScrollTo(platformView, index, offset, args.IsAnimated);
		}
		else
		{
			platformView.Loaded -= OnPlatformViewLoaded;

			void OnPlatformViewLoaded(object sender, RoutedEventArgs e)
			{
				platformView.Loaded -= OnPlatformViewLoaded;
				PerformScrollTo(platformView, index, offset, args.IsAnimated);
			}
			platformView.Loaded += OnPlatformViewLoaded;
		}

	}

	void PerformScrollTo(WItemsView platformView, int index, double offset, bool animated)
	{
		platformView.StartBringItemIntoView(index, new BringIntoViewOptions()
		{
			AnimationDesired = animated,
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

	/// <summary>
	/// Finds the flat index in the flattened grouped collection for a ScrollToMode.Element request
	/// where a group is specified. If item is null, returns the index of the group header.
	/// If item is non-null, returns the index of that item within the specified group.
	/// </summary>
	int FindGroupedItemByElement(object? item, object group)
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

		// Find the target group and its items by matching the group object
		IList? targetGroupItems = null;
		int flatIndexOfGroup = 0;
		int currentFlatIndex = 0;

		foreach (var g in itemsSource)
		{
			if (g is not IList groupList)
			{
				continue;
			}

			if (Equals(g, group))
			{
				targetGroupItems = groupList;
				flatIndexOfGroup = currentFlatIndex;
				break;
			}

			// Advance past this group's entries in the flat list
			if (hasGroupHeader)
			{
				currentFlatIndex++;
			}

			currentFlatIndex += groupList.Count;

			if (hasGroupFooter)
			{
				currentFlatIndex++;
			}
		}

		if (targetGroupItems is null)
		{
			return -1;
		}

		// If item is null, scroll to the group header (if it exists)
		if (item is null)
		{
			if (hasGroupHeader)
			{
				return flatIndexOfGroup;
			}

			// No header template — scroll to the first item in the group instead
			if (targetGroupItems.Count > 0)
			{
				return flatIndexOfGroup;
			}

			return -1;
		}

		// Find the item within the target group
		int itemStartIndex = flatIndexOfGroup;
		if (hasGroupHeader)
		{
			itemStartIndex++;
		}

		for (int i = 0; i < targetGroupItems.Count; i++)
		{
			if (Equals(targetGroupItems[i], item))
			{
				return itemStartIndex + i;
			}
		}

		return -1;
	}
}
