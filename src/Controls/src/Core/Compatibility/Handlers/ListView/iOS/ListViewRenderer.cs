#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Foundation;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ListView;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public class ListViewRenderer : ViewRenderer<ListView, UITableView>
	{
		public static PropertyMapper<ListView, ListViewRenderer> Mapper =
				new PropertyMapper<ListView, ListViewRenderer>(VisualElementRendererMapper);

		public static CommandMapper<ListView, ListViewRenderer> CommandMapper =
			new CommandMapper<ListView, ListViewRenderer>(VisualElementRendererCommandMapper);

		const int DefaultRowHeight = 44;

		UIView _backgroundUIView;
		ListViewDataSource _dataSource;
		IPlatformViewHandler _headerRenderer;
		IPlatformViewHandler _footerRenderer;

		RectangleF _previousFrame;
		ScrollToRequestedEventArgs _requestedScroll;

		FormsUITableViewController _tableViewController;
		ListView ListView => Element;
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		public override UIViewController ViewController => _tableViewController;
		//bool _disposed;
		bool _usingLargeTitles;

		bool? _defaultHorizontalScrollVisibility;
		bool? _defaultVerticalScrollVisibility;

		protected UITableViewRowAnimation InsertRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation DeleteRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadSectionsAnimation
		{
			get { return _dataSource.ReloadSectionsAnimation; }
			set { _dataSource.ReloadSectionsAnimation = value; }
		}

		public ListViewRenderer() : base(Mapper, CommandMapper)
		{
			AutoPackage = false;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			double height = Bounds.Height;
			double width = Bounds.Width;
			if (_headerRenderer != null)
			{
				UpdateHeaderMeasure();
			}

			if (_footerRenderer != null)
			{
				UpdateFooterMeasure();
			}

			if (_requestedScroll != null && Superview != null)
			{
				var request = _requestedScroll;
				_requestedScroll = null;
				OnScrollToRequested(this, request);
			}

			if (_previousFrame != Frame)
				_previousFrame = Frame;
		}

		protected override void SetBackground(Brush brush)
		{
			if (Control == null)
				return;

			BrushExtensions.RemoveBackgroundLayer(_backgroundUIView);

			if (!Brush.IsNullOrEmpty(brush))
			{
				if (_backgroundUIView == null)
				{
					_backgroundUIView = new UIView();
					Control.BackgroundView = _backgroundUIView;
				}

				if (brush is SolidColorBrush solidColorBrush)
				{
					var backgroundColor = solidColorBrush.Color;

					if (backgroundColor == null)
						_backgroundUIView.BackgroundColor = UIColor.White;
					else
						_backgroundUIView.BackgroundColor = backgroundColor.ToPlatform();
				}
				else
				{
					var backgroundLayer = _backgroundUIView.GetBackgroundLayer(Element.Background);

					if (backgroundLayer != null)
					{
						_backgroundUIView.BackgroundColor = UIColor.Clear;
						BrushExtensions.InsertBackgroundLayer(_backgroundUIView, backgroundLayer, 0);
					}
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (Element != null)
				{
					var templatedItems = TemplatedItemsView.TemplatedItems;
					templatedItems.CollectionChanged -= OnCollectionChanged;
					templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				}

				if (_dataSource != null)
				{
					_dataSource.Dispose();
					_dataSource = null;
				}

				if (_tableViewController != null)
				{
					_tableViewController.Dispose();
					_tableViewController = null;
				}

				if (_headerRenderer != null)
				{
					_headerRenderer.VirtualView?.DisposeModalAndChildHandlers();
					_headerRenderer = null;
				}
				if (_footerRenderer != null)
				{
					_footerRenderer.VirtualView?.DisposeModalAndChildHandlers();
					_footerRenderer = null;
				}

				if (_backgroundUIView != null)
				{
					_backgroundUIView.Dispose();
					_backgroundUIView = null;
				}

				var headerView = ListView?.HeaderElement as VisualElement;
				if (headerView != null)
					headerView.MeasureInvalidated -= OnHeaderMeasureInvalidated;
				Control?.TableHeaderView?.Dispose();

				var footerView = ListView?.FooterElement as VisualElement;
				if (footerView != null)
					footerView.MeasureInvalidated -= OnFooterMeasureInvalidated;
				Control?.TableFooterView?.Dispose();
			}

			_disposed = true;

			base.Dispose(disposing);
		}
		bool _disposed = false;
		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			_requestedScroll = null;

			if (e.OldElement != null)
			{
				var listView = e.OldElement;
				var headerView = (VisualElement)listView.HeaderElement;
				if (headerView != null)
					headerView.MeasureInvalidated -= OnHeaderMeasureInvalidated;

				var footerView = (VisualElement)listView.FooterElement;
				if (footerView != null)
					footerView.MeasureInvalidated -= OnFooterMeasureInvalidated;

				listView.ScrollToRequested -= OnScrollToRequested;
				var templatedItems = ((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems;

				templatedItems.CollectionChanged -= OnCollectionChanged;
				templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
						|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
					)
					{
						var parentNav = e.NewElement.FindParentOfType<NavigationPage>();
						_usingLargeTitles = (parentNav != null && parentNav.OnThisPlatform().PrefersLargeTitles());
					}
					_tableViewController = new FormsUITableViewController(e.NewElement, _usingLargeTitles);
					SetNativeControl(_tableViewController.TableView);

					if (OperatingSystem.IsIOSVersionAtLeast(15) || OperatingSystem.IsMacCatalystVersionAtLeast(15)
#if TVOS
						|| OperatingSystem.IsTvOSVersionAtLeast(15)
#endif
					)
						_tableViewController.TableView.SectionHeaderTopPadding = new nfloat(0);

					_backgroundUIView = _tableViewController.TableView.BackgroundView;
				}

				var listView = e.NewElement;

				listView.ScrollToRequested += OnScrollToRequested;
				var templatedItems = ((ITemplatedItemsView<Cell>)e.NewElement).TemplatedItems;

				templatedItems.CollectionChanged += OnCollectionChanged;
				templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;

				UpdateRowHeight();

				Control.Source = _dataSource = e.NewElement.HasUnevenRows ? new UnevenListViewDataSource(e.NewElement, _tableViewController) : new ListViewDataSource(e.NewElement, _tableViewController);

				UpdateHeader();
				UpdateFooter();
				UpdatePullToRefreshEnabled();
				UpdateSpinnerColor();
				UpdateIsRefreshing();
				UpdateSeparatorColor();
				UpdateSeparatorVisibility();
				UpdateSelectionMode();
				UpdateVerticalScrollBarVisibility();
				UpdateHorizontalScrollBarVisibility();

				var selected = e.NewElement.SelectedItem;
				if (selected != null)
					_dataSource.OnItemSelected(null, new SelectedItemChangedEventArgs(selected, templatedItems.GetGlobalIndexOfItem(selected)));
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Microsoft.Maui.Controls.ListView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.IsGroupingEnabledProperty.PropertyName)
				_dataSource.UpdateGrouping();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.HasUnevenRowsProperty.PropertyName)
			{
				Control.Source = _dataSource = Element.HasUnevenRows ? new UnevenListViewDataSource(_dataSource) : new ListViewDataSource(_dataSource);
				ReloadData();
			}
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.IsPullToRefreshEnabledProperty.PropertyName)
				UpdatePullToRefreshEnabled();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.SeparatorColorProperty.PropertyName)
				UpdateSeparatorColor();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.SeparatorVisibilityProperty.PropertyName)
				UpdateSeparatorVisibility();
			else if (e.PropertyName == "HeaderElement")
				UpdateHeader();
			else if (e.PropertyName == "FooterElement")
				UpdateFooter();
			else if (e.PropertyName == "RefreshAllowed")
				UpdatePullToRefreshEnabled();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.SelectionModeProperty.PropertyName)
				UpdateSelectionMode();
			else if (e.PropertyName == Microsoft.Maui.Controls.ListView.RefreshControlColorProperty.PropertyName)
				UpdateSpinnerColor();
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
		}

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
#pragma warning disable CA1422 // Validate platform compatibility
			base.TraitCollectionDidChange(previousTraitCollection);
#pragma warning restore CA1422 // Validate platform compatibility
			// Make sure the cells adhere to changes UI theme
			if (OperatingSystem.IsIOSVersionAtLeast(13) && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				ReloadData();
		}

		NSIndexPath[] GetPaths(int section, int index, int count)
		{
			var paths = new NSIndexPath[count];
			for (var i = 0; i < paths.Length; i++)
				paths[i] = NSIndexPath.FromRowSection(index + i, section);

			return paths;
		}

		UITableViewScrollPosition GetScrollPosition(ScrollToPosition position)
		{
			switch (position)
			{
				case ScrollToPosition.Center:
					return UITableViewScrollPosition.Middle;
				case ScrollToPosition.End:
					return UITableViewScrollPosition.Bottom;
				case ScrollToPosition.Start:
					return UITableViewScrollPosition.Top;
				case ScrollToPosition.MakeVisible:
				default:
					return UITableViewScrollPosition.None;
			}
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateItems(e, 0, true);
		}

		void UpdateFooterMeasure()
		{
			Control.TableFooterView = null;
			if (Bounds.Width * Bounds.Height == 0)
				return;

			var size = _footerRenderer.VirtualView.Measure(Bounds.Width, double.PositiveInfinity);
			var platformFrame = new RectangleF(0, 0, size.Width, size.Height);
			_footerRenderer.PlatformView.Frame = platformFrame;
			_footerRenderer.VirtualView.Arrange(platformFrame.ToRectangle());
			Control.TableFooterView = _footerRenderer.PlatformView;

			BeginInvokeOnMainThread(() =>
			{
				if (_footerRenderer != null)
					Control.TableFooterView = _footerRenderer.PlatformView;
			});
		}

		void UpdateHeaderMeasure()
		{
			Control.TableHeaderView = null;

			if (Bounds.Width * Bounds.Height == 0)
				return;

			var size = _headerRenderer.VirtualView.Measure(Bounds.Width, double.PositiveInfinity);
			var platformFrame = new RectangleF(0, 0, size.Width, size.Height);
			_headerRenderer.PlatformView.Frame = platformFrame;
			_headerRenderer.VirtualView.Arrange(platformFrame.ToRectangle());
			Control.TableHeaderView = _headerRenderer.PlatformView;

			// Time for another story with Jason. Gather round children because the following Math.Ceiling will look like it's completely useless.
			// You will remove it and test and find everything is fiiiiiine, but it is not fine, no it is far from fine. See iOS, or at least iOS 8
			// has an issue where-by if the TableHeaderView happens to NOT be an integer height, it will add padding to the space between the content
			// of the UITableView and the TableHeaderView to the tune of the difference between Math.Ceiling (height) - height. Now this seems fine
			// and when you test it will be, EXCEPT that it does this every time you toggle the visibility of the UITableView causing the spacing to
			// grow a little each time, which you weren't testing at all were you? So there you have it, the stupid reason we integer align here.
			//
			// The same technically applies to the footer, though that could hardly matter less. We just do it for fun.
			BeginInvokeOnMainThread(() =>
			{
				if (_headerRenderer != null)
					Control.TableHeaderView = _headerRenderer.PlatformView;
			});
		}

		void OnFooterMeasureInvalidated(object sender, EventArgs eventArgs)
		{
			UpdateFooterMeasure();
		}

		void OnGroupedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var til = (TemplatedItemsList<ItemsView<Cell>, Cell>)sender;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			var groupIndex = templatedItems.IndexOf(til.HeaderContent);
			UpdateItems(e, groupIndex, false);
		}

		void OnHeaderMeasureInvalidated(object sender, EventArgs eventArgs)
		{
			UpdateHeaderMeasure();
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			if (Superview == null)
			{
				_requestedScroll = e;
				return;
			}

			var position = GetScrollPosition(e.Position);
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (Element.IsGroupingEnabled)
			{
				var result = templatedItems.GetGroupAndIndexOfItem(scrollArgs.Group, scrollArgs.Item);
				if (result.Item1 != -1 && result.Item2 != -1)
					Control.ScrollToRow(NSIndexPath.FromRowSection(result.Item2, result.Item1), position, e.ShouldAnimate);
			}
			else
			{
				var index = templatedItems.GetGlobalIndexOfItem(scrollArgs.Item);
				if (index != -1)
				{
					Control.Layer.RemoveAllAnimations();
					//iOS11 hack
					if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11))
						this.BeginInvokeOnMainThread(() =>
						{
							if (Control != null /*&& !_disposed*/)
								Control.ScrollToRow(NSIndexPath.FromRowSection(index, 0), position, e.ShouldAnimate);
						});
					else
						Control.ScrollToRow(NSIndexPath.FromRowSection(index, 0), position, e.ShouldAnimate);
				}
			}
		}

		void UpdateFooter()
		{
			var footer = ListView.FooterElement;
			var footerView = (View)footer;

			if (footerView != null)
			{
				if (_footerRenderer != null)
				{
					((VisualElement)_footerRenderer.VirtualView).MeasureInvalidated -= OnFooterMeasureInvalidated;
					var reflectableType = _footerRenderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _footerRenderer.GetType();
					if (footer != null && rendererType == Microsoft.Maui.Controls.Internals.Registrar.Registered.GetHandlerTypeForObject(footer))
					{
						_footerRenderer.SetVirtualView(footerView);
						return;
					}
					Control.TableFooterView = null;

					((Element)_footerRenderer.VirtualView)?.DisposeModalAndChildHandlers();
					_footerRenderer.DisconnectHandler();
				}

				_footerRenderer = footerView.ToHandler(MauiContext);

				footerView.MeasureInvalidated += OnFooterMeasureInvalidated;
				UpdateFooterMeasure();
			}
			else if (_footerRenderer != null)
			{
				Control.TableFooterView = null;
				((VisualElement)_footerRenderer.VirtualView).MeasureInvalidated -= OnFooterMeasureInvalidated;

				_footerRenderer.VirtualView?.DisposeModalAndChildHandlers();
				_footerRenderer.DisconnectHandler();
				_footerRenderer = null;
			}
		}

		void UpdateHeader()
		{
			var header = ListView.HeaderElement;
			var headerView = (View)header;

			if (headerView != null)
			{
				if (_headerRenderer != null)
				{
					((VisualElement)_headerRenderer.VirtualView).MeasureInvalidated -= OnHeaderMeasureInvalidated;
					var reflectableType = _headerRenderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _headerRenderer.GetType();
					if (header != null && rendererType == Microsoft.Maui.Controls.Internals.Registrar.Registered.GetHandlerTypeForObject(header))
					{
						_headerRenderer.SetVirtualView(headerView);
						return;
					}
					Control.TableHeaderView = null;

					_headerRenderer.VirtualView?.DisposeModalAndChildHandlers();
					_headerRenderer?.DisconnectHandler();
				}

				_headerRenderer = headerView.ToHandler(MauiContext);

				headerView.MeasureInvalidated += OnHeaderMeasureInvalidated;
				UpdateHeaderMeasure();
			}
			else if (_headerRenderer != null)
			{
				Control.TableHeaderView = null;
				((VisualElement)_headerRenderer.VirtualView).MeasureInvalidated -= OnHeaderMeasureInvalidated;

				_headerRenderer.VirtualView?.DisposeModalAndChildHandlers();
				_headerRenderer = null;
			}
		}

		void UpdateIsRefreshing()
		{
			var refreshing = Element.IsRefreshing;
			if (_tableViewController != null)
				_tableViewController.UpdateIsRefreshing(refreshing);
		}

		void UpdateItems(NotifyCollectionChangedEventArgs e, int section, bool resetWhenGrouped)
		{
			var exArgs = e as NotifyCollectionChangedEventArgsEx;
			if (exArgs != null)
				_dataSource.Counts[section] = exArgs.Count;

			// This means the UITableView hasn't rendered any cells yet
			// so there's no need to synchronize the rows on the UITableView
			if (Control.IndexPathsForVisibleRows == null && e.Action != NotifyCollectionChangedAction.Reset)
				return;

			var groupReset = resetWhenGrouped && Element.IsGroupingEnabled;

			// We can't do this check on grouped lists because the index doesn't match the number of rows in a section.
			// Likewise, we can't do this check on lists using RecycleElement because the number of rows in a section will remain constant because they are reused.
			if (!groupReset && Element.CachingStrategy == ListViewCachingStrategy.RetainElement)
			{
				var lastIndex = Control.NumberOfRowsInSection(section);
				if (e.NewStartingIndex > lastIndex || e.OldStartingIndex > lastIndex)
					throw new ArgumentException(
						$"Index '{Math.Max(e.NewStartingIndex, e.OldStartingIndex)}' is greater than the number of rows '{lastIndex}'.");
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					InsertRows(e.NewStartingIndex, e.NewItems.Count, section);

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					DeleteRows(e.OldStartingIndex, e.OldItems.Count, section);

					if (TemplatedItemsView.TemplatedItems.Count == 0)
						InvalidateCellCache();

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					MoveRows(e.NewStartingIndex, e.OldStartingIndex, e.OldItems.Count, section);

					if (e.OldStartingIndex == 0)
						InvalidateCellCache();

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					ReloadRows(e.OldStartingIndex, e.OldItems.Count, section);

					if (e.OldStartingIndex == 0)
						InvalidateCellCache();

					break;

				case NotifyCollectionChangedAction.Reset:
					InvalidateCellCache();
					ReloadData();
					return;
			}
		}

		void InsertRows(int newStartingIndex, int newItemsCount, int section)
		{
			var action = new Action(() =>
			{
				Control.BeginUpdates();
				Control.InsertRows(GetPaths(section, newStartingIndex, newItemsCount), InsertRowsAnimation);
				Control.EndUpdates();
			});

			if (Element.OnThisPlatform().RowAnimationsEnabled())
				action.Invoke();
			else
				PerformWithoutAnimation(() => { action.Invoke(); });
		}

		void DeleteRows(int oldStartingIndex, int oldItemsCount, int section)
		{
			var action = new Action(() =>
			{
				Control.BeginUpdates();
				Control.DeleteRows(GetPaths(section, oldStartingIndex, oldItemsCount), DeleteRowsAnimation);
				Control.EndUpdates();
			});

			if (Element.OnThisPlatform().RowAnimationsEnabled())
				action.Invoke();
			else
				PerformWithoutAnimation(() => { action.Invoke(); });
		}

		void MoveRows(int newStartingIndex, int oldStartingIndex, int oldItemsCount, int section)
		{
			var action = new Action(() =>
			{
				Control.BeginUpdates();
				for (var i = 0; i < oldItemsCount; i++)
				{
					var oldIndex = oldStartingIndex;
					var newIndex = newStartingIndex;

					if (newStartingIndex < oldStartingIndex)
					{
						oldIndex += i;
						newIndex += i;
					}

					Control.MoveRow(NSIndexPath.FromRowSection(oldIndex, section), NSIndexPath.FromRowSection(newIndex, section));
				}
				Control.EndUpdates();
			});

			if (Element.OnThisPlatform().RowAnimationsEnabled())
				action.Invoke();
			else
				PerformWithoutAnimation(() => { action.Invoke(); });
		}

		void ReloadRows(int oldStartingIndex, int oldItemsCount, int section)
		{
			var action = new Action(() =>
			{
				Control.BeginUpdates();
				Control.ReloadRows(GetPaths(section, oldStartingIndex, oldItemsCount), ReloadRowsAnimation);
				Control.EndUpdates();
			});

			if (Element.OnThisPlatform().RowAnimationsEnabled())
				action.Invoke();
			else
				PerformWithoutAnimation(() => { action.Invoke(); });
		}

		void ReloadData()
		{
			if (Element.OnThisPlatform().RowAnimationsEnabled())
				Control.ReloadData();
			else
				PerformWithoutAnimation(() => { Control.ReloadData(); });
		}

		void InvalidateCellCache()
		{
			_dataSource.InvalidatePrototypicalCellCache();
		}

		void UpdatePullToRefreshEnabled()
		{
			if (_tableViewController != null)
			{
				var isPullToRequestEnabled = Element.IsPullToRefreshEnabled && ListView.RefreshAllowed;
				_tableViewController.UpdatePullToRefreshEnabled(isPullToRequestEnabled);
			}
		}

		void UpdateRowHeight()
		{
			var rowHeight = Element.RowHeight;

			if (Element.HasUnevenRows && rowHeight == -1)
				Control.RowHeight = UITableView.AutomaticDimension;
			else
				Control.RowHeight = rowHeight <= 0 ? DefaultRowHeight : rowHeight;
		}

		void UpdateSeparatorColor()
		{
			var color = Element.SeparatorColor;
			// ...and Steve said to the unbelievers the separator shall be gray, and gray it was. The unbelievers looked on, and saw that it was good, and
			// they went forth and documented the default color. The holy scripture still reflects this default.
			// Defined here: https://developer.apple.com/library/ios/documentation/UIKit/Reference/UITableView_Class/#//apple_ref/occ/instp/UITableView/separatorColor
			Control.SeparatorColor = color?.ToPlatform() ?? Microsoft.Maui.Platform.ColorExtensions.SeparatorColor;
		}

		void UpdateSeparatorVisibility()
		{
			var visibility = Element.SeparatorVisibility;
			switch (visibility)
			{
				case SeparatorVisibility.Default:
					Control.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
					break;
				case SeparatorVisibility.None:
					Control.SeparatorStyle = UITableViewCellSeparatorStyle.None;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		void UpdateSelectionMode()
		{
			if (Element.SelectionMode == ListViewSelectionMode.None)
			{
				Element.SelectedItem = null;
				var selectedIndexPath = Control.IndexPathForSelectedRow;
				if (selectedIndexPath != null)
					Control.DeselectRow(selectedIndexPath, false);
			}
		}

		void UpdateSpinnerColor()
		{
			var color = Element.RefreshControlColor;

			if (_tableViewController != null)
				_tableViewController.UpdateRefreshControlColor(color == null ? null : color.ToPlatform());
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = Control.ShowsVerticalScrollIndicator;

			switch (Element.VerticalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					Control.ShowsVerticalScrollIndicator = true;
					break;
				case (ScrollBarVisibility.Never):
					Control.ShowsVerticalScrollIndicator = false;
					break;
				case (ScrollBarVisibility.Default):
					Control.ShowsVerticalScrollIndicator = (bool)_defaultVerticalScrollVisibility;
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = Control.ShowsHorizontalScrollIndicator;

			switch (Element.HorizontalScrollBarVisibility)
			{
				case (ScrollBarVisibility.Always):
					Control.ShowsHorizontalScrollIndicator = true;
					break;
				case (ScrollBarVisibility.Never):
					Control.ShowsHorizontalScrollIndicator = false;
					break;
				case (ScrollBarVisibility.Default):
					Control.ShowsHorizontalScrollIndicator = (bool)_defaultHorizontalScrollVisibility;
					break;
			}
		}

		internal sealed class UnevenListViewDataSource : ListViewDataSource
		{
			IPlatformViewHandler _prototype;
			bool _disposed;
			Dictionary<object, Cell> _prototypicalCellByTypeOrDataTemplate = new Dictionary<object, Cell>();

			public UnevenListViewDataSource(ListView list, FormsUITableViewController uiTableViewController) : base(list, uiTableViewController)
			{
			}

			public UnevenListViewDataSource(ListViewDataSource source) : base(source)
			{
			}

			nfloat GetEstimatedRowHeight(UITableView table)
			{
				if (List.RowHeight != -1)
				{
					// Not even sure we need this case; A list with HasUnevenRows and a RowHeight doesn't make a ton of sense
					// Anyway, no need for an estimate, because the heights we'll use are known
					return 0;
				}

				var templatedItems = TemplatedItemsView.TemplatedItems;

				if (templatedItems.Count == 0)
				{
					// No cells to provide an estimate, use the default row height constant
					return DefaultRowHeight;
				}

				// We're going to base our estimate off of the first cell
				var isGroupingEnabled = List.IsGroupingEnabled;

				if (isGroupingEnabled)
					templatedItems = templatedItems.GetGroup(0);

				object item = null;
				if (templatedItems == null || templatedItems.ListProxy.TryGetValue(0, out item) == false)
					return DefaultRowHeight;

				var firstCell = templatedItems.ActivateContent(0, item);

				// Let's skip this optimization for grouped lists. It will likely cause more trouble than it's worth.
				if (firstCell?.Height > 0 && !isGroupingEnabled)
				{
					// Seems like we've got cells which already specify their height; since the heights are known,
					// we don't need to use estimatedRowHeight at all; zero will disable it and use the known heights.
					// However, not setting the EstimatedRowHeight will drastically degrade performance with large lists.
					// In this case, we will cache the specified cell heights asynchronously, which will be returned one time on
					// table load by EstimatedHeight.

					return 0;
				}

				return CalculateHeightForCell(table, firstCell);
			}

			internal override void InvalidatingPrototypicalCellCache()
			{
				ClearPrototype();
				_prototypicalCellByTypeOrDataTemplate.Clear();
			}

			protected override void UpdateEstimatedRowHeight(UITableView tableView)
			{
				var estimatedRowHeight = GetEstimatedRowHeight(tableView);
				//if we are providing 0 we are disabling EstimatedRowHeight,
				//this works fine on newer versions, but iOS10 it will cause a crash so we leave the default value
				if (estimatedRowHeight > 0 || (estimatedRowHeight == 0 && (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11))))
					tableView.EstimatedRowHeight = estimatedRowHeight;
			}

			internal Cell GetPrototypicalCell(NSIndexPath indexPath)
			{
				var itemTypeOrDataTemplate = default(object);

				var cachingStrategy = List.CachingStrategy;
				if (cachingStrategy == ListViewCachingStrategy.RecycleElement)
					itemTypeOrDataTemplate = GetDataTemplateForPath(indexPath);

				else if (cachingStrategy == ListViewCachingStrategy.RecycleElementAndDataTemplate)
					itemTypeOrDataTemplate = GetItemTypeForPath(indexPath);

				else // ListViewCachingStrategy.RetainElement
					return GetCellForPath(indexPath);

				if (itemTypeOrDataTemplate == null)
					itemTypeOrDataTemplate = typeof(TextCell);

				if (!_prototypicalCellByTypeOrDataTemplate.TryGetValue(itemTypeOrDataTemplate, out Cell protoCell))
				{
					// cache prototypical cell by item type; Items of the same Type share
					// the same DataTemplate (this is enforced by RecycleElementAndDataTemplate)
					protoCell = GetCellForPath(indexPath);
					_prototypicalCellByTypeOrDataTemplate[itemTypeOrDataTemplate] = protoCell;
				}

				var templatedItems = GetTemplatedItemsListForPath(indexPath);
				return templatedItems.UpdateContent(protoCell, indexPath.Row);
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{
				// iOS may ask for a row we have just deleted and hence cannot rebind in order to measure height.
				if (!IsValidIndexPath(indexPath))
					return DefaultRowHeight;

				var cell = GetPrototypicalCell(indexPath);

				if (List.RowHeight == -1 && cell.Height == -1 && cell is ViewCell)
					return UITableView.AutomaticDimension;

				var renderHeight = cell.RenderHeight;
				return renderHeight > 0 ? (nfloat)renderHeight : DefaultRowHeight;
			}

			internal nfloat CalculateHeightForCell(UITableView tableView, Cell cell)
			{
				var viewCell = cell as ViewCell;
				if (viewCell != null && viewCell.View != null)
				{
					var target = viewCell.View;
					if (_prototype == null)
						_prototype = target.ToHandler(cell.FindMauiContext());
					else
						_prototype.SetVirtualView(target);

					var req = target.Measure(tableView.Frame.Width, double.PositiveInfinity);
					target.Handler?.DisconnectHandler();

					foreach (Element descendant in target.Descendants())
					{

						// Clear renderer from descendent; this will not happen in Dispose as normal because we need to
						// unhook the Element from the renderer before disposing it.
						descendant.Handler?.DisconnectHandler();
					}

					// Let the EstimatedHeight method know to use this value.
					// Much more efficient than checking the value each time.
					//_useEstimatedRowHeight = true;
					var height = (nfloat)req.Request.Height;
					return height > 1 ? height : DefaultRowHeight;
				}

				var renderHeight = cell.RenderHeight;
				return renderHeight > 0 ? (nfloat)renderHeight : DefaultRowHeight;
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				_disposed = true;

				if (disposing)
				{
					ClearPrototype();
				}

				base.Dispose(disposing);
			}

			void ClearPrototype()
			{
				if (_prototype != null)
				{
					var element = _prototype.VirtualView;
					element?.Handler?.DisconnectHandler();
					//_prototype?.Dispose();
					//_prototype = null;
				}
			}
		}

		internal class ListViewDataSource : UITableViewSource
		{
			const int DefaultItemTemplateId = 1;
			static int s_dataTemplateIncrementer = 2; // lets start at not 0 because
			readonly nfloat _defaultSectionHeight;
			Dictionary<DataTemplate, int> _templateToId = new Dictionary<DataTemplate, int>();
			UITableView _uiTableView;
			FormsUITableViewController _uiTableViewController;
			protected ListView List;
			protected ITemplatedItemsView<Cell> TemplatedItemsView => List;
			bool _isDragging;
			bool _setupSelection;
			bool _selectionFromNative;
			bool _disposed;
			bool _wasEmpty;
			bool _estimatedRowHeight;

			public UITableViewRowAnimation ReloadSectionsAnimation { get; set; } = UITableViewRowAnimation.Automatic;

			public ListViewDataSource(ListViewDataSource source)
			{
				_uiTableViewController = source._uiTableViewController;
				List = source.List;
				_uiTableView = source._uiTableView;
				_defaultSectionHeight = source._defaultSectionHeight;
				_selectionFromNative = source._selectionFromNative;

				Counts = new Dictionary<int, int>();
			}

			public ListViewDataSource(ListView list, FormsUITableViewController uiTableViewController)
			{
				_uiTableViewController = uiTableViewController;
				_uiTableView = uiTableViewController.TableView;
				_defaultSectionHeight = DefaultRowHeight;
				List = list;
				List.ItemSelected += OnItemSelected;
				UpdateShortNameListener();

				Counts = new Dictionary<int, int>();
			}

			public Dictionary<int, int> Counts { get; set; }

			UIColor DefaultBackgroundColor => UIColor.Clear;

			internal void InvalidatePrototypicalCellCache()
			{
				_estimatedRowHeight = false;
				InvalidatingPrototypicalCellCache();
			}

			internal virtual void InvalidatingPrototypicalCellCache()
			{
			}

			public override void DraggingEnded(UIScrollView scrollView, bool willDecelerate)
			{
				_isDragging = false;
				_uiTableViewController.UpdateShowHideRefresh(false);
			}

			public override void DraggingStarted(UIScrollView scrollView)
			{
				_isDragging = true;
			}

			void SetupSelection(UITableViewCell platformCell, UITableView tableView)
			{
				if (!(platformCell is ContextActionsCell))
					return;

				if (_setupSelection)
					return;

				ContextActionsCell.SetupSelection(tableView);

				_setupSelection = true;
			}

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				Cell cell;
				UITableViewCell platformCell;

				Performance.Start(out string reference);

				var cachingStrategy = List.CachingStrategy;
				if (cachingStrategy == ListViewCachingStrategy.RetainElement)
				{
					cell = GetCellForPath(indexPath);
					platformCell = CellTableViewCell.GetPlatformCell(tableView, cell);
				}
				else if ((cachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
				{
					var id = TemplateIdForPath(indexPath);
					platformCell = tableView.DequeueReusableCell(ContextActionsCell.Key + id);
					if (platformCell == null)
					{
						cell = GetCellForPath(indexPath);

						platformCell = CellTableViewCell.GetPlatformCell(tableView, cell, true, id.ToString());
					}
					else
					{
						var templatedList = TemplatedItemsView.TemplatedItems.GetGroup(indexPath.Section);

						cell = (Cell)((INativeElementView)platformCell).Element;
						cell.SendDisappearing();

						templatedList.UpdateContent(cell, indexPath.Row);
						cell.SendAppearing();
					}
				}
				else
					throw new NotSupportedException();

				SetupSelection(platformCell, tableView);

				if (List.IsSet(Specifics.SeparatorStyleProperty))
				{
					if (List.OnThisPlatform().GetSeparatorStyle() == SeparatorStyle.FullWidth)
					{
						platformCell.SeparatorInset = UIEdgeInsets.Zero;
						platformCell.LayoutMargins = UIEdgeInsets.Zero;
						platformCell.PreservesSuperviewLayoutMargins = false;
					}
				}
				var bgColor = tableView.IndexPathForSelectedRow != null && tableView.IndexPathForSelectedRow.Equals(indexPath) ? UIColor.Clear : DefaultBackgroundColor;
				SetCellBackgroundColor(platformCell, bgColor);
				PreserveActivityIndicatorState(cell);
				Performance.Stop(reference);
				return platformCell;
			}

			public override nfloat GetHeightForHeader(UITableView tableView, nint section)
			{
				if (List.IsGroupingEnabled)
				{
					var cell = TemplatedItemsView.TemplatedItems[(int)section];
					nfloat height = (float)cell.RenderHeight;
					if (height == -1)
						height = _defaultSectionHeight;

					return height;
				}

				return 0;
			}

			public override UIView GetViewForHeader(UITableView tableView, nint section)
			{
				if (!List.IsGroupingEnabled)
					return null;

				var cell = TemplatedItemsView.TemplatedItems[(int)section];
				if (cell.HasContextActions)
					throw new NotSupportedException("Header cells do not support context actions");

				const string reuseIdentifier = "HeaderWrapper";
				var header = (HeaderWrapperView)tableView.DequeueReusableHeaderFooterView(reuseIdentifier) ?? new HeaderWrapperView(reuseIdentifier);
				header.Cell = cell;

				cell.TableView = tableView;
				cell.ReusableCell = null;

				var handler = cell.ToHandler(cell.FindMauiContext());
				var renderer = (handler as CellRenderer) ?? (handler.PlatformView as CellRenderer);
				header.SetTableViewCell(renderer.PlatformView);

				return header;
			}

			public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
			{
				if (!List.IsGroupingEnabled)
					return;

				if (headerView is HeaderWrapperView wrapper)
				{
					wrapper.Cell?.SendDisappearing();
					wrapper.Cell = null;
				}
			}

			public override nint NumberOfSections(UITableView tableView)
			{
				if (List.IsGroupingEnabled)
					return TemplatedItemsView.TemplatedItems.Count;

				return 1;
			}

			public void OnItemSelected(object sender, SelectedItemChangedEventArgs eventArg)
			{
				if (_selectionFromNative)
				{
					_selectionFromNative = false;
					return;
				}

				if (List == null)
					return;

				var location = TemplatedItemsView.TemplatedItems.GetGroupAndIndexOfItem(eventArg.SelectedItem);
				if (location.Item1 == -1 || location.Item2 == -1)
				{
					var selectedIndexPath = _uiTableView.IndexPathForSelectedRow;

					var animate = true;

					if (selectedIndexPath != null)
					{
						var cell = _uiTableView.CellAt(selectedIndexPath) as ContextActionsCell;
						if (cell != null)
						{
							cell.PrepareForDeselect();
							if (cell.IsOpen)
								animate = false;
						}
					}

					if (selectedIndexPath != null)
						_uiTableView.DeselectRow(selectedIndexPath, animate);
					return;
				}

				_uiTableView.SelectRow(NSIndexPath.FromRowSection(location.Item2, location.Item1), true, UITableViewScrollPosition.Middle);
			}

			public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.CellAt(indexPath);
				if (cell == null)
					return;

				SetCellBackgroundColor(cell, DefaultBackgroundColor);
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.CellAt(indexPath);

				if (cell == null)
					return;

				Cell formsCell = null;
				if ((List.CachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
					formsCell = (Cell)((INativeElementView)cell).Element;

				SetCellBackgroundColor(cell, UIColor.Clear);

				if (List.SelectionMode == ListViewSelectionMode.None)
					tableView.DeselectRow(indexPath, false);

				_selectionFromNative = true;

				tableView.EndEditing(true);
				List.NotifyRowTapped(indexPath.Section, indexPath.Row, formsCell);
			}

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				int countOverride;
				if (Counts.TryGetValue((int)section, out countOverride))
				{
					Counts.Remove((int)section);
					if (_wasEmpty && countOverride > 0)
					{
						// We've moved from no items to having at least one item; it's likely that the layout needs to update
						// its cell size/estimate
						_estimatedRowHeight = false;
					}
					_wasEmpty = countOverride == 0;
					return countOverride;
				}

				var templatedItems = TemplatedItemsView.TemplatedItems;
				if (List.IsGroupingEnabled)
				{
					var group = (IList)((IList)templatedItems)[(int)section];
					return group.Count;
				}

				return templatedItems.Count;
			}

			public override void Scrolled(UIScrollView scrollView)
			{
				var args = new ScrolledEventArgs(scrollView.ContentOffset.X, scrollView.ContentOffset.Y);
				List?.SendScrolled(args);

				if (_isDragging && scrollView.ContentOffset.Y < 0)
				{
					// If the refresh spinner is currently displayed and pull-to-refresh is not enabled,
					// calling UpdateShowHideRefresh will remove the spinner and cause the ScrollView to shift;
					// this will fire off this Scrolled override again and we'll be in an infinite loop (which iOS
					// will promptly kill, and the app will close)
					// So we temporarily flip _isDragging to false in order to prevent the loop.
					_isDragging = false;
					_uiTableViewController.UpdateShowHideRefresh(true);
					_isDragging = true;
				}

				if (_isDragging && scrollView.ContentOffset.Y < -10f && _uiTableViewController._usingLargeTitles && DeviceDisplay.MainDisplayInfo.Orientation.IsPortrait())
				{
					_uiTableViewController.ForceRefreshing();
				}
			}

			public override string[] SectionIndexTitles(UITableView tableView)
			{
				var templatedItems = TemplatedItemsView.TemplatedItems;
				if (templatedItems.ShortNames == null)
					return null;

				return templatedItems.ShortNames.ToArray();
			}

			public void Cleanup()
			{
				_selectionFromNative = false;
				_isDragging = false;
			}

			public void UpdateGrouping()
			{
				UpdateShortNameListener();

				if (List.OnThisPlatform().RowAnimationsEnabled())
					_uiTableView.ReloadData();
				else
					PerformWithoutAnimation(() => { _uiTableView.ReloadData(); });
			}

			public void DetermineEstimatedRowHeight()
			{
				if (_estimatedRowHeight)
					return;

				UpdateEstimatedRowHeight(_uiTableView);

				_estimatedRowHeight = true;
			}

			protected bool IsValidIndexPath(NSIndexPath indexPath)
			{
				var templatedItems = TemplatedItemsView.TemplatedItems;
				if (List.IsGroupingEnabled)
				{
					var section = indexPath.Section;
					if (section < 0 || section >= templatedItems.Count)
						return false;

					templatedItems = (ITemplatedItemsList<Cell>)((IList)templatedItems)[indexPath.Section];
				}

				return templatedItems.ListProxy.TryGetValue(indexPath.Row, out var _);
			}

			protected ITemplatedItemsList<Cell> GetTemplatedItemsListForPath(NSIndexPath indexPath)
			{
				var templatedItems = TemplatedItemsView.TemplatedItems;
				if (List.IsGroupingEnabled)
					templatedItems = (ITemplatedItemsList<Cell>)((IList)templatedItems)[indexPath.Section];

				return templatedItems;
			}

			protected DataTemplate GetDataTemplateForPath(NSIndexPath indexPath)
			{
				var templatedList = GetTemplatedItemsListForPath(indexPath);
				var item = templatedList.ListProxy[indexPath.Row];
				return templatedList.SelectDataTemplate(item);
			}

			protected Type GetItemTypeForPath(NSIndexPath indexPath)
			{
				var templatedList = GetTemplatedItemsListForPath(indexPath);
				var item = templatedList.ListProxy[indexPath.Row];
				return item.GetType();
			}

			protected Cell GetCellForPath(NSIndexPath indexPath)
			{
				var templatedItems = GetTemplatedItemsListForPath(indexPath);
				var cell = templatedItems[indexPath.Row];
				return cell;
			}

			void OnShortNamesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (List.OnThisPlatform().RowAnimationsEnabled())
					_uiTableView.ReloadSectionIndexTitles();
				else
					PerformWithoutAnimation(() => { _uiTableView.ReloadSectionIndexTitles(); });
			}

			static void SetCellBackgroundColor(UITableViewCell cell, UIColor color)
			{
				var contextCell = cell as ContextActionsCell;
				cell.BackgroundColor = color;
				if (contextCell != null)
					contextCell.ContentCell.BackgroundColor = color;
			}

			int TemplateIdForPath(NSIndexPath indexPath)
			{
				var itemTemplate = List.ItemTemplate;
				var selector = itemTemplate as DataTemplateSelector;
				if (selector == null)
					return DefaultItemTemplateId;

				var templatedList = GetTemplatedItemsListForPath(indexPath);
				var item = templatedList.ListProxy[indexPath.Row];

				itemTemplate = selector.SelectTemplate(item, List);
				int key;
				if (!_templateToId.TryGetValue(itemTemplate, out key))
				{
					s_dataTemplateIncrementer++;
					key = s_dataTemplateIncrementer;
					_templateToId[itemTemplate] = key;
				}
				return key;
			}

			void UpdateShortNameListener()
			{
				WatchShortNameCollection(List.IsGroupingEnabled);
			}

			void WatchShortNameCollection(bool watch)
			{
				var templatedList = TemplatedItemsView.TemplatedItems;

				if (templatedList.ShortNames == null)
				{
					return;
				}

				var incc = (INotifyCollectionChanged)templatedList.ShortNames;

				if (watch)
				{
					incc.CollectionChanged += OnShortNamesCollectionChanged;
				}
				else
				{
					incc.CollectionChanged -= OnShortNamesCollectionChanged;
				}
			}

			protected virtual void UpdateEstimatedRowHeight(UITableView tableView)
			{
				// We need to set a default estimated row height,
				// because re-setting it later(when we have items on the TIL)
				// will cause the UITableView to reload, and throw an Exception
				tableView.EstimatedRowHeight = DefaultRowHeight;

				// if even rows OR uneven rows but user specified a row height anyway...
				if (!List.HasUnevenRows || List.RowHeight != -1)
					tableView.EstimatedRowHeight = 0;
			}

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (disposing)
				{
					if (List != null)
					{
						List.ItemSelected -= OnItemSelected;
						WatchShortNameCollection(false);
						List = null;
					}

					_templateToId = null;
					_uiTableView = null;
					_uiTableViewController = null;
				}

				_disposed = true;

				base.Dispose(disposing);
			}

			void PreserveActivityIndicatorState(Element element)
			{
				if (element == null)
					return;

				var activityIndicator = element as ActivityIndicator;
				if (activityIndicator != null)
				{
					// TODO MAUI
					//var renderer = Platform.GetRenderer(activityIndicator) as ActivityIndicatorRenderer;
					//renderer?.PreserveState();
				}
				else
				{
					foreach (Element childElement in (element as IElementController).LogicalChildren)
						PreserveActivityIndicatorState(childElement);
				}
			}
		}
	}

	class HeaderWrapperView : UITableViewHeaderFooterView
	{
		public HeaderWrapperView(string reuseIdentifier) : base((NSString)reuseIdentifier)
		{
		}

		UITableViewCell _tableViewCell;

		public Cell Cell { get; set; }

		public void SetTableViewCell(UITableViewCell value)
		{
			if (ReferenceEquals(_tableViewCell, value))
				return;
			_tableViewCell?.RemoveFromSuperview();
			_tableViewCell = value;
			AddSubview(value);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			foreach (var item in Subviews)
				item.Frame = Bounds;
		}
	}

	internal sealed class FormsUITableViewController : UITableViewController
	{
		ListView _list;
		UIRefreshControl _refresh;

		bool _refreshAdded;
		bool _disposed;
		internal bool _usingLargeTitles;
		bool _isRefreshing;
		bool _isStartRefreshingPending;

		public FormsUITableViewController(ListView element, bool usingLargeTitles)
		: base(element.OnThisPlatform().GetGroupHeaderStyle() == GroupHeaderStyle.Plain
			? UITableViewStyle.Plain
			  : UITableViewStyle.Grouped)
		{
			TableView.CellLayoutMarginsFollowReadableWidth = false;

			_usingLargeTitles = usingLargeTitles;

			_refresh = new FormsRefreshControl(_usingLargeTitles);
			_refresh.ValueChanged += OnRefreshingChanged;
			_list = element;
		}

		public void UpdateIsRefreshing(bool refreshing)
		{
			if (refreshing)
			{
				if (!_refreshAdded)
				{
					RefreshControl = _refresh;
					_refreshAdded = true;
				}

				if (!_refresh.Refreshing)
				{
					_isStartRefreshingPending = true;
					//hack: On iOS11 with large titles we need to adjust the scroll offset manually
					//since our UITableView is not the first child of the UINavigationController
					//This also forces the spinner color to be correct if we started refreshing immediately after changing it.
					UpdateContentOffset(TableView.ContentOffset.Y - _refresh.Frame.Height, () =>
					{
						if (_refresh == null || _disposed)
							return;

						if (_isStartRefreshingPending)
							StartRefreshing();


						//hack: when we don't have cells in our UITableView the spinner fails to appear
						CheckContentSize();
						TableView.ScrollRectToVisible(new RectangleF(0, 0, _refresh.Bounds.Width, _refresh.Bounds.Height), true);
					});
				}
			}
			else
			{
				if (RefreshControl == null)
					return;

				EndRefreshing();

				UpdateContentOffset(-1);

				_isRefreshing = false;
				if (!_list.IsPullToRefreshEnabled)
					RemoveRefresh();
			}
		}

		void StartRefreshing()
		{
			_isStartRefreshingPending = false;
			if (_refresh?.Refreshing == true)
				return;

			_refresh.BeginRefreshing();
		}

		void EndRefreshing()
		{
			_isStartRefreshingPending = false;
			if (_refresh?.Refreshing == false)
				return;

			_refresh.EndRefreshing();
		}

		public void UpdatePullToRefreshEnabled(bool pullToRefreshEnabled)
		{
			if (pullToRefreshEnabled)
			{
				if (!_refreshAdded)
				{
					_refreshAdded = true;
					RefreshControl = _refresh;
				}
			}
			// https://bugzilla.xamarin.com/show_bug.cgi?id=52962
			// just because pullToRefresh is being disabled does not mean we should kill an in progress refresh.
			// Consider the case where:
			//   1. User pulls to refresh
			//   2. App RefreshCommand fires (at this point _refresh.Refreshing is true)
			//   3. RefreshCommand disables itself via a call to ChangeCanExecute which returns false
			//			(maybe the command it's attached to a button the app wants disabled)
			//   4. OnCommandCanExecuteChanged handler sets RefreshAllowed to false because the RefreshCommand is disabled
			//   5. We end up here; A refresh is in progress while being asked to disable pullToRefresh
		}

		//hack: Form some reason UIKit isn't allowing to scroll negative values with largetitles
		public void ForceRefreshing()
		{
			if (!_list.IsPullToRefreshEnabled)
				return;
			if (!_refresh.Refreshing && !_isRefreshing)
			{
				_isRefreshing = true;
				UpdateContentOffset(TableView.ContentOffset.Y - _refresh.Frame.Height, StartRefreshing);
				_list.SendRefreshing();
			}
		}

		public void UpdateShowHideRefresh(bool shouldHide)
		{
			if (_list.IsPullToRefreshEnabled)
				return;

			if (shouldHide)
				RemoveRefresh();
			else
				UpdateIsRefreshing(_list.IsRefreshing);
		}

		public override void ViewWillAppear(bool animated)
		{
			(TableView?.Source as ListViewRenderer.ListViewDataSource)?.Cleanup();
			if (!_list.IsRefreshing || !_refresh.Refreshing)
				return;

			// Restart the refreshing to get the animation to trigger
			UpdateIsRefreshing(false);
			UpdateIsRefreshing(true);
		}

		public override void ViewWillLayoutSubviews()
		{
			(TableView?.Source as ListViewRenderer.ListViewDataSource)?.DetermineEstimatedRowHeight();
		}

		public void UpdateRefreshControlColor(UIColor color)
		{
			if (RefreshControl != null)
				RefreshControl.TintColor = color;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_refresh != null)
				{
					_refresh.ValueChanged -= OnRefreshingChanged;
					EndRefreshing();
					_refresh.Dispose();
					_refresh = null;
				}

				_list = null;
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		void CheckContentSize()
		{
			//adding a default height of at least 1 pixel tricks iOS to show the spinner
			var contentSize = TableView.ContentSize;
			if (contentSize.Height == 0)
				TableView.ContentSize = new SizeF(contentSize.Width, 1);
		}

		void OnRefreshingChanged(object sender, EventArgs eventArgs)
		{
			if (_refresh.Refreshing)
				_list.SendRefreshing();

			_isRefreshing = _refresh.Refreshing;
		}

		void RemoveRefresh()
		{
			if (!_refreshAdded)
				return;

			if (_refresh.Refreshing || _isRefreshing)
				EndRefreshing();

			RefreshControl = null;
			_refreshAdded = false;
			_isRefreshing = false;
		}

		void UpdateContentOffset(nfloat offset, Action completed = null)
		{
			UIView.Animate(0.2, () => TableView.ContentOffset = new CoreGraphics.CGPoint(TableView.ContentOffset.X, offset), completed);
		}
	}

	public class FormsRefreshControl : UIRefreshControl
	{
		bool _usingLargeTitles;

		public FormsRefreshControl(bool usingLargeTitles)
		{
			_usingLargeTitles = usingLargeTitles;
			AccessibilityIdentifier = "RefreshControl";
		}

		public override bool Hidden
		{
			get
			{
				return base.Hidden;
			}
			set
			{
				//hack: ahahah take that UIKit!
				//when using pull to refresh with Large tiles sometimes iOS tries to hide the UIRefreshControl
				if (_usingLargeTitles && value && Refreshing)
					return;
				base.Hidden = value;
			}
		}

		public override void BeginRefreshing()
		{
			base.BeginRefreshing();
			if (!_usingLargeTitles)
				return;
			Hidden = false;
		}
	}
}
