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
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public abstract class ItemsViewHandler2<TItemsView> : ViewHandler<TItemsView, WItemsView> where TItemsView : ItemsView
	{
		CollectionViewSource? _collectionViewSource;
		IList? _itemsSource;

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
			[Controls.StructuredItemsView.ItemsLayoutProperty.PropertyName] = MapItemsLayout,
			[Controls.StructuredItemsView.HeaderProperty.PropertyName] = MapHeader,
			[Controls.StructuredItemsView.HeaderTemplateProperty.PropertyName] = MapHeaderTemplate,
			[Controls.StructuredItemsView.FooterProperty.PropertyName] = MapFooter,
			[Controls.StructuredItemsView.FooterTemplateProperty.PropertyName] = MapFooterTemplate,
		};



		private bool _scrollUpdatePending;

		public static void MapItemsSource(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemsSource();
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

			VirtualView.ScrollToRequested += ScrollToRequested;
			FindScrollViewer();
		}


		protected override void DisconnectHandler(WItemsView platformView)
		{
			// Unsubscribe from LayoutUpdated before disconnecting to prevent exceptions
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
							, IsSourceGrouped = false
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

			PlatformView.ItemsSource = _collectionViewSource?.View;

			if (VirtualView.ItemTemplate is not null)
			{
				PlatformView.ItemTemplate = new ItemFactory(Element);
			}
			else if (PlatformView.ItemTemplate is not null)
			{
				PlatformView.ItemTemplate = null;
			}

			UpdateEmptyViewVisibility();
		}

		void CleanUpCollectionViewSource()
		{
			if (_collectionViewSource is not null)
			{
				if (_collectionViewSource.Source is ObservableItemTemplateCollection2 observableItemTemplateCollection)
				{
					observableItemTemplateCollection.CleanUp();
				}

				if (_collectionViewSource.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= ItemsChanged;
				}

				_collectionViewSource.Source = null;
				_collectionViewSource = null;
			}

			// Remove all children inside the ItemsSource
			if (VirtualView is not null)
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

			if(e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Reset )
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

			if (_itemsSource is null || _itemsSource.Count == 0 || _collectionViewSource?.View?.Count <= 1)
			{
				return;
			}

			if (PlatformView.Layout is MauiGridLayout)
			{
				PlatformView.UpdateLayout();
			}


			if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepItemsInView)
			{
				_isScrollingForItemsUpdate = true;
				// Keeps the first item in the list displayed when new items are added.
				PlatformView.StartBringItemIntoView(0, new BringIntoViewOptions() { AnimationDesired = false });
			}
			else if (VirtualView.ItemsUpdatingScrollMode == ItemsUpdatingScrollMode.KeepLastItemInView)
			{
				// Adjusts the scroll offset to keep the last item in the list displayed when new items are added.
				PlatformView.StartBringItemIntoView(_itemsSource.Count - 1, new BringIntoViewOptions() { AnimationDesired = false });
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

			UpdateItemsSource();

			PlatformView.Layout = CreateItemsLayout();

			bool isHorizontal = IsLayoutHorizontal;

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
		}

		UI.Xaml.Controls.Layout CreateItemsLayout()
		{
			switch (Layout)
			{
				case GridItemsLayout gridItemsLayout:
					// Use MauiGridLayout for all grid layouts
					// It supports full-span headers/footers for grouped collections
					// and works correctly for non-grouped collections too
					return CreateMauiGridLayout(gridItemsLayout);
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

		static MauiGridLayout CreateMauiGridLayout(GridItemsLayout gridItemsLayout)
		{
			return new MauiGridLayout()
			{
				Orientation = gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal
						? Orientation.Vertical : Orientation.Horizontal,
				Span = gridItemsLayout.Span,
				HorizontalSpacing = gridItemsLayout.HorizontalItemSpacing,
				VerticalSpacing = gridItemsLayout.VerticalItemSpacing,
			};
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

			void ListViewLoaded(object sender, RoutedEventArgs e)
			{
				var lv = (WItemsView)sender;
				lv.Loaded -= ListViewLoaded;
				FindScrollViewer();
			}

			PlatformView.Loaded += ListViewLoaded;
		}

		void OnScrollViewerFound()
		{
			if (PlatformView is null || PlatformView.ScrollView is null)
			{
				return;
			}

			PlatformView.ScrollView.ViewChanged -= ScrollViewChanged;
			PlatformView.ScrollView.PointerWheelChanged -= PointerScrollChanged;

			PlatformView.ScrollView.ViewChanged += ScrollViewChanged;
			PlatformView.ScrollView.PointerWheelChanged += PointerScrollChanged;

			UpdateVerticalScrollBarVisibility();
			UpdateHorizontalScrollBarVisibility();
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
		}

		void UpdateItemsLayoutSpan()
		{
			if (PlatformView.Layout is MauiGridLayout mauiGridLayout &&
				Layout is GridItemsLayout gridItemsLayout)
			{
				mauiGridLayout.Span = gridItemsLayout.Span;
			}
		}

		void UpdateItemsLayoutItemSpacing()
		{
			if (PlatformView.Layout is MauiGridLayout mauiGridLayout &&
				Layout is GridItemsLayout gridItemsLayout)
			{
				mauiGridLayout.HorizontalSpacing = gridItemsLayout.HorizontalItemSpacing;
				mauiGridLayout.VerticalSpacing = gridItemsLayout.VerticalItemSpacing;
			}
			else if (PlatformView.Layout is UI.Xaml.Controls.StackLayout stackLayout &&
				Layout is LinearItemsLayout linearItemsLayout)
			{
				stackLayout.Spacing = linearItemsLayout.ItemSpacing;
			}
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

			// Resolve empty view
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
				if (_mauiEmptyView is not null)
				{
					if (_emptyViewDisplayed)
					{
						ItemsView.RemoveLogicalChild(_mauiEmptyView);
					}

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
						Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
					},
					View view => ItemsViewExtensions.RealizeHeaderFooterView(view, MauiContext!, ref _mauiHeader),
					_ => new TextBlock
					{
						Text = header.ToString(),
						Margin = new Microsoft.UI.Xaml.Thickness(0, 0, 0, 10)
					}
				};
			}
			else
			{
				// This shouldn't happen due to null check above, but handle it safely
				return;
			}

			if (PlatformView is MauiItemsView platformItemsView && _header is not null)
			{
				platformItemsView.SetHeader(_header, _mauiHeader);
				platformItemsView.HeaderVisibility = WVisibility.Visible;
			}

			// Add logical child for View
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
						Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
					},
					View view => ItemsViewExtensions.RealizeHeaderFooterView(view, MauiContext!, ref _mauiFooter),
					_ => new TextBlock
					{
						Text = footer.ToString(),
						Margin = new Microsoft.UI.Xaml.Thickness(0, 10, 0, 0)
					}
				};
			}
			else
			{
				// This shouldn't happen due to null check above, but handle it safely
				return;
			}

			if (PlatformView is MauiItemsView platformItemsView && _footer is not null)
			{
				platformItemsView.SetFooter(_footer, _mauiFooter);
				platformItemsView.FooterVisibility = WVisibility.Visible;
			}

			// Add logical child for View
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
					scrollView.VerticalScrollBarVisibility =_defaultVerticalScrollVisibility.Value;
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
			if(_isScrollingForItemsUpdate)
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
					advancing = itemsViewScrolledEventArgs.HorizontalDelta > 0;
					break;
				case GridItemsLayout gridItemsLayout:
					advancing = itemsViewScrolledEventArgs.VerticalDelta > 0;
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
					
				if(itemsRemaining<= remainingItemsThreshold)
				{
					if (itemsViewScrolledEventArgs.LastVisibleItemIndex > _lastRemainingItemsThresholdIndex) 
					{
						_lastRemainingItemsThresholdIndex = itemsViewScrolledEventArgs.LastVisibleItemIndex;
					Element.SendRemainingItemsThresholdReached();
					}
				
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
			var index = args.Index;
			if (args.Mode == ScrollToMode.Element)
			{
				index = FindItemIndex(args.Item);
			}

			// Validate index is within bounds
			var itemCount = _collectionViewSource?.View?.Count ?? 0;
			if (index < 0 || index >= itemCount)
			{
				return;
			}

			float offset = 0.0f;
			switch (args.ScrollToPosition)
			{
				case ScrollToPosition.Start:
					offset = 0.0f;
					break;
				case ScrollToPosition.Center:
					offset = 0.5f;
					break;
				case ScrollToPosition.End:
					offset = 1.0f;
					break;
			}

			PlatformView.StartBringItemIntoView(index, new BringIntoViewOptions()
			{
				AnimationDesired = args.IsAnimated,
				VerticalAlignmentRatio = offset,
				HorizontalAlignmentRatio = offset
			});
		}

		int FindItemIndex(object item)
		{
			if (_collectionViewSource is null)
			{
				return -1;
			}

			for (int index = 0; index < _collectionViewSource.View.Count; index++)
			{
				if (_collectionViewSource.View[index] is ItemTemplateContext pair)
				{
					if (pair.Item == item)
					{
						return index;
					}
				}
			}

			return -1;
		}
	}
}
