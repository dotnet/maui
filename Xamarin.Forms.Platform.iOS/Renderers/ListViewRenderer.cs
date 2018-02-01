using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class ListViewRenderer : ViewRenderer<ListView, UITableView>
	{
		const int DefaultRowHeight = 44;
		ListViewDataSource _dataSource;
		bool _estimatedRowHeight;
		IVisualElementRenderer _headerRenderer;
		IVisualElementRenderer _footerRenderer;

		KeyboardInsetTracker _insetTracker;
		RectangleF _previousFrame;
		ScrollToRequestedEventArgs _requestedScroll;

		FormsUITableViewController _tableViewController;
		ListView ListView => Element;
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		public override UIViewController ViewController => _tableViewController;
		bool _disposed;
		protected UITableViewRowAnimation InsertRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation DeleteRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadRowsAnimation { get; set; } = UITableViewRowAnimation.Automatic;
		protected UITableViewRowAnimation ReloadSectionsAnimation
		{
			get { return _dataSource.ReloadSectionsAnimation; }
			set { _dataSource.ReloadSectionsAnimation = value; }
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return Control.GetSizeRequest(widthConstraint, heightConstraint, DefaultRowHeight, DefaultRowHeight);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			double height = Bounds.Height;
			double width = Bounds.Width;
			if (_headerRenderer != null)
			{
				var e = _headerRenderer.Element;
				var request = e.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

				// Time for another story with Jason. Gather round children because the following Math.Ceiling will look like it's completely useless.
				// You will remove it and test and find everything is fiiiiiine, but it is not fine, no it is far from fine. See iOS, or at least iOS 8
				// has an issue where-by if the TableHeaderView happens to NOT be an integer height, it will add padding to the space between the content
				// of the UITableView and the TableHeaderView to the tune of the difference between Math.Ceiling (height) - height. Now this seems fine
				// and when you test it will be, EXCEPT that it does this every time you toggle the visibility of the UITableView causing the spacing to 
				// grow a little each time, which you weren't testing at all were you? So there you have it, the stupid reason we integer align here.
				//
				// The same technically applies to the footer, though that could hardly matter less. We just do it for fun.
				Layout.LayoutChildIntoBoundingRegion(e, new Rectangle(0, 0, width, Math.Ceiling(request.Request.Height)));

				Device.BeginInvokeOnMainThread(() =>
				{
					if (_headerRenderer != null)
						Control.TableHeaderView = _headerRenderer.NativeView;
				});
			}

			if (_footerRenderer != null)
			{
				var e = _footerRenderer.Element;
				var request = e.Measure(width, height, MeasureFlags.IncludeMargins);
				Layout.LayoutChildIntoBoundingRegion(e, new Rectangle(0, 0, width, Math.Ceiling(request.Request.Height)));

				Device.BeginInvokeOnMainThread(() =>
				{
					if (_footerRenderer != null)
						Control.TableFooterView = _footerRenderer.NativeView;
				});
			}

			if (_requestedScroll != null && Superview != null)
			{
				var request = _requestedScroll;
				_requestedScroll = null;
				OnScrollToRequested(this, request);
			}

			if (_previousFrame != Frame)
			{
				_previousFrame = Frame;
				_insetTracker?.UpdateInsets();
			}
		}

		void DisposeSubviews(UIView view)
		{
			var ver = view as IVisualElementRenderer;

			if (ver == null)
			{
				// VisualElementRenderers should implement their own dispose methods that will appropriately dispose and remove their child views.
				// Attempting to do this work twice could cause a SIGSEGV (only observed in iOS8), so don't do this work here.
				// Non-renderer views, such as separator lines, etc., can be removed here.
				foreach (UIView subView in view.Subviews)
					DisposeSubviews(subView);

				view.RemoveFromSuperview();
			}

			view.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_insetTracker != null)
				{
					_insetTracker.Dispose();
					_insetTracker = null;
				}

				foreach (UIView subview in Subviews)
					DisposeSubviews(subview);

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
					var platform = _headerRenderer.Element?.Platform as Platform;
					platform?.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
					_headerRenderer = null;
				}
				if (_footerRenderer != null)
				{
					var platform = _footerRenderer.Element?.Platform as Platform;
					platform?.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
					_footerRenderer = null;
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
					_tableViewController = new FormsUITableViewController(e.NewElement);
					SetNativeControl(_tableViewController.TableView);
					if (Forms.IsiOS9OrNewer)
						Control.CellLayoutMarginsFollowReadableWidth = false;

					_insetTracker = new KeyboardInsetTracker(_tableViewController.TableView, () => Control.Window, insets => Control.ContentInset = Control.ScrollIndicatorInsets = insets, point =>
					{
						var offset = Control.ContentOffset;
						offset.Y += point.Y;
						Control.SetContentOffset(offset, true);
					});
				}

				var listView = e.NewElement;

				listView.ScrollToRequested += OnScrollToRequested;
				var templatedItems = ((ITemplatedItemsView<Cell>)e.NewElement).TemplatedItems;

				templatedItems.CollectionChanged += OnCollectionChanged;
				templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;

				UpdateRowHeight();

				Control.Source = _dataSource = e.NewElement.HasUnevenRows ? new UnevenListViewDataSource(e.NewElement, _tableViewController) : new ListViewDataSource(e.NewElement, _tableViewController);

				UpdateEstimatedRowHeight();
				UpdateHeader();
				UpdateFooter();
				UpdatePullToRefreshEnabled();
				UpdateIsRefreshing();
				UpdateSeparatorColor();
				UpdateSeparatorVisibility();

				var selected = e.NewElement.SelectedItem;
				if (selected != null)
					_dataSource.OnItemSelected(null, new SelectedItemChangedEventArgs(selected));
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Xamarin.Forms.ListView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == Xamarin.Forms.ListView.IsGroupingEnabledProperty.PropertyName)
				_dataSource.UpdateGrouping();
			else if (e.PropertyName == Xamarin.Forms.ListView.HasUnevenRowsProperty.PropertyName)
			{
				_estimatedRowHeight = false;
				Control.Source = _dataSource = Element.HasUnevenRows ? new UnevenListViewDataSource(_dataSource) : new ListViewDataSource(_dataSource);
			}
			else if (e.PropertyName == Xamarin.Forms.ListView.IsPullToRefreshEnabledProperty.PropertyName)
				UpdatePullToRefreshEnabled();
			else if (e.PropertyName == Xamarin.Forms.ListView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.PropertyName == Xamarin.Forms.ListView.SeparatorColorProperty.PropertyName)
				UpdateSeparatorColor();
			else if (e.PropertyName == Xamarin.Forms.ListView.SeparatorVisibilityProperty.PropertyName)
				UpdateSeparatorVisibility();
			else if (e.PropertyName == "HeaderElement")
				UpdateHeader();
			else if (e.PropertyName == "FooterElement")
				UpdateFooter();
			else if (e.PropertyName == "RefreshAllowed")
				UpdatePullToRefreshEnabled();
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

		void OnFooterMeasureInvalidated(object sender, EventArgs eventArgs)
		{
			double width = Bounds.Width;
			if (width == 0)
				return;

			var footerView = (VisualElement)sender;
			var request = footerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Layout.LayoutChildIntoBoundingRegion(footerView, new Rectangle(0, 0, width, request.Request.Height));

			Control.TableFooterView = _footerRenderer.NativeView;
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
			double width = Bounds.Width;
			if (width == 0)
				return;

			var headerView = (VisualElement)sender;
			var request = headerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Layout.LayoutChildIntoBoundingRegion(headerView, new Rectangle(0, 0, width, request.Request.Height));

			Control.TableHeaderView = _headerRenderer.NativeView;
		}

		async void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
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
					if (Forms.IsiOS11OrNewer)
					{
						await Task.Delay(1);
					}
					Control.ScrollToRow(NSIndexPath.FromRowSection(index, 0), position, e.ShouldAnimate);
				}
			}
		}

		void UpdateEstimatedRowHeight()
		{
			if (_estimatedRowHeight)
				return;

			// if even rows OR uneven rows but user specified a row height anyway...
			if (!Element.HasUnevenRows || Element.RowHeight != -1)
			{
				Control.EstimatedRowHeight = 0;
				_estimatedRowHeight = true;
				return;
			}

			var source = _dataSource as UnevenListViewDataSource;

			// We want to make sure we reset the cached defined row heights whenever this is called.
			// Failing to do this will regress Bugzilla 43313 
			// (strange animation when adding rows with uneven heights)
			//source?.CacheDefinedRowHeights();

			if (source == null)
			{
				// We need to set a default estimated row height, 
				// because re-setting it later(when we have items on the TIL)
				// will cause the UITableView to reload, and throw an Exception
				Control.EstimatedRowHeight = DefaultRowHeight;
				return;
			}

			Control.EstimatedRowHeight = source.GetEstimatedRowHeight(Control);
			_estimatedRowHeight = true;
			return;
		}

		void UpdateFooter()
		{
			var footer = ListView.FooterElement;
			var footerView = (View)footer;

			if (footerView != null)
			{
				if (_footerRenderer != null)
				{
					_footerRenderer.Element.MeasureInvalidated -= OnFooterMeasureInvalidated;
					var reflectableType = _footerRenderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _footerRenderer.GetType();
					if (footer != null && rendererType == Internals.Registrar.Registered.GetHandlerTypeForObject(footer))
					{
						_footerRenderer.SetElement(footerView);
						return;
					}
					Control.TableFooterView = null;
					var platform = _footerRenderer.Element.Platform as Platform;
					if (platform != null)
						platform.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
					_footerRenderer.Dispose();
					_footerRenderer = null;
				}

				_footerRenderer = Platform.CreateRenderer(footerView);
				Platform.SetRenderer(footerView, _footerRenderer);

				double width = Bounds.Width;
				var request = footerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Layout.LayoutChildIntoBoundingRegion(footerView, new Rectangle(0, 0, width, request.Request.Height));

				Control.TableFooterView = _footerRenderer.NativeView;
				footerView.MeasureInvalidated += OnFooterMeasureInvalidated;
			}
			else if (_footerRenderer != null)
			{
				Control.TableFooterView = null;
				_footerRenderer.Element.MeasureInvalidated -= OnFooterMeasureInvalidated;

				var platform = _footerRenderer.Element.Platform as Platform;
				if (platform != null)
					platform.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
				_footerRenderer.Dispose();
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
					_headerRenderer.Element.MeasureInvalidated -= OnHeaderMeasureInvalidated;
					var reflectableType = _headerRenderer as System.Reflection.IReflectableType;
					var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _headerRenderer.GetType();
					if (header != null && rendererType == Internals.Registrar.Registered.GetHandlerTypeForObject(header))
					{
						_headerRenderer.SetElement(headerView);
						return;
					}
					Control.TableHeaderView = null;
					var platform = _headerRenderer.Element.Platform as Platform;
					if (platform != null)
						platform.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
					_headerRenderer = null;
				}

				_headerRenderer = Platform.CreateRenderer(headerView);
				// This will force measure to invalidate, which we haven't hooked up to yet because we are smarter!
				Platform.SetRenderer(headerView, _headerRenderer);

				double width = Bounds.Width;
				var request = headerView.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Layout.LayoutChildIntoBoundingRegion(headerView, new Rectangle(0, 0, width, request.Request.Height));

				Control.TableHeaderView = _headerRenderer.NativeView;
				headerView.MeasureInvalidated += OnHeaderMeasureInvalidated;
			}
			else if (_headerRenderer != null)
			{
				Control.TableHeaderView = null;
				_headerRenderer.Element.MeasureInvalidated -= OnHeaderMeasureInvalidated;

				var platform = _headerRenderer.Element.Platform as Platform;
				if (platform != null)
					platform.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
				_headerRenderer.Dispose();
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

			var groupReset = resetWhenGrouped && Element.IsGroupingEnabled;

			if (!groupReset)
			{
				var lastIndex = Control.NumberOfRowsInSection(section);
				if (e.NewStartingIndex > lastIndex || e.OldStartingIndex > lastIndex)
					throw new ArgumentException(
						$"Index '{Math.Max(e.NewStartingIndex, e.OldStartingIndex)}' is greater than the number of rows '{lastIndex}'.");
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:

					UpdateEstimatedRowHeight();
					if (e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					Control.BeginUpdates();
					Control.InsertRows(GetPaths(section, e.NewStartingIndex, e.NewItems.Count), InsertRowsAnimation);
					Control.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					Control.DeleteRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), DeleteRowsAnimation);

					Control.EndUpdates();

					if (_estimatedRowHeight && TemplatedItemsView.TemplatedItems.Count == 0)
						_estimatedRowHeight = false;

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						var oldi = e.OldStartingIndex;
						var newi = e.NewStartingIndex;

						if (e.NewStartingIndex < e.OldStartingIndex)
						{
							oldi += i;
							newi += i;
						}

						Control.MoveRow(NSIndexPath.FromRowSection(oldi, section), NSIndexPath.FromRowSection(newi, section));
					}
					Control.EndUpdates();

					if (_estimatedRowHeight && e.OldStartingIndex == 0)
						_estimatedRowHeight = false;

					break;

				case NotifyCollectionChangedAction.Replace:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					Control.ReloadRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), ReloadRowsAnimation);
					Control.EndUpdates();

					if (_estimatedRowHeight && e.OldStartingIndex == 0)
						_estimatedRowHeight = false;

					break;

				case NotifyCollectionChangedAction.Reset:
					_estimatedRowHeight = false;
					Control.ReloadData();
					return;
			}
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
			Control.SeparatorColor = color.ToUIColor(UIColor.Gray);
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

		internal class UnevenListViewDataSource : ListViewDataSource
		{
			IVisualElementRenderer _prototype;
			bool _disposed;
			Dictionary<object, Cell> _prototypicalCellByTypeOrDataTemplate = new Dictionary<object, Cell>();

			public UnevenListViewDataSource(ListView list, FormsUITableViewController uiTableViewController) : base(list, uiTableViewController)
			{
			}

			public UnevenListViewDataSource(ListViewDataSource source) : base(source)
			{
			}

			internal nfloat GetEstimatedRowHeight(UITableView table)
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

			internal override void InvalidatePrototypicalCellCache()
			{
				_prototypicalCellByTypeOrDataTemplate.Clear();
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


				Cell protoCell;
				if (!_prototypicalCellByTypeOrDataTemplate.TryGetValue(itemTypeOrDataTemplate, out protoCell))
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
						_prototype = Platform.CreateRenderer(target);
					else
						_prototype.SetElement(target);

					Platform.SetRenderer(target, _prototype);

					var req = target.Measure(tableView.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

					target.ClearValue(Platform.RendererProperty);
					foreach (Element descendant in target.Descendants())
					{
						IVisualElementRenderer renderer = Platform.GetRenderer(descendant as VisualElement);

						// Clear renderer from descendent; this will not happen in Dispose as normal because we need to
						// unhook the Element from the renderer before disposing it.
						descendant.ClearValue(Platform.RendererProperty);
						renderer?.Dispose();
						renderer = null;
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
					if (_prototype != null)
					{
						_prototype.Dispose();
						_prototype = null;
					}
				}

				base.Dispose(disposing);
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
			bool _selectionFromNative;
			bool _disposed;
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

			UIColor DefaultBackgroundColor
			{
				get { return UIColor.Clear; }
			}

			internal virtual void InvalidatePrototypicalCellCache()
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

			public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
			{
				Cell cell;
				UITableViewCell nativeCell;

				var reference = Guid.NewGuid().ToString();
				Performance.Start(reference);

				var cachingStrategy = List.CachingStrategy;
				if (cachingStrategy == ListViewCachingStrategy.RetainElement)
				{
					cell = GetCellForPath(indexPath);
					nativeCell = CellTableViewCell.GetNativeCell(tableView, cell);
				}
				else if ((cachingStrategy & ListViewCachingStrategy.RecycleElement) != 0)
				{
					var id = TemplateIdForPath(indexPath);
					nativeCell = tableView.DequeueReusableCell(ContextActionsCell.Key + id);
					if (nativeCell == null)
					{
						cell = GetCellForPath(indexPath);

						nativeCell = CellTableViewCell.GetNativeCell(tableView, cell, true, id.ToString());
					}
					else
					{
						var templatedList = TemplatedItemsView.TemplatedItems.GetGroup(indexPath.Section);

						cell = (Cell)((INativeElementView)nativeCell).Element;
						cell.SendDisappearing();

						templatedList.UpdateContent(cell, indexPath.Row);
						cell.SendAppearing();
					}
				}
				else
					throw new NotSupportedException();

				var bgColor = tableView.IndexPathForSelectedRow != null && tableView.IndexPathForSelectedRow.Equals(indexPath) ? UIColor.Clear : DefaultBackgroundColor;

				SetCellBackgroundColor(nativeCell, bgColor);
				PreserveActivityIndicatorState(cell);
				Performance.Stop(reference);
				return nativeCell;
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
				UIView view = null;

				if (!List.IsGroupingEnabled)
					return view;

				var cell = TemplatedItemsView.TemplatedItems[(int)section];
				if (cell.HasContextActions)
					throw new NotSupportedException("Header cells do not support context actions");

					var renderer = (CellRenderer)Internals.Registrar.Registered.GetHandlerForObject<IRegisterable>(cell);

					view = new HeaderWrapperView();
					view.AddSubview(renderer.GetCell(cell, null, tableView));

				return view;
			}

			public override void HeaderViewDisplayingEnded(UITableView tableView, UIView headerView, nint section)
			{
				if (!List.IsGroupingEnabled)
					return;

				var cell = TemplatedItemsView.TemplatedItems[(int)section];
				cell.SendDisappearing();
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
				if (_isDragging && scrollView.ContentOffset.Y < 0)
					_uiTableViewController.UpdateShowHideRefresh(true);
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
				_uiTableView.ReloadData();
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
				_uiTableView.ReloadSectionIndexTitles();
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
				var templatedList = TemplatedItemsView.TemplatedItems;
				if (List.IsGroupingEnabled)
				{
					if (templatedList.ShortNames != null)
						((INotifyCollectionChanged)templatedList.ShortNames).CollectionChanged += OnShortNamesCollectionChanged;
				}
				else
				{
					if (templatedList.ShortNames != null)
						((INotifyCollectionChanged)templatedList.ShortNames).CollectionChanged -= OnShortNamesCollectionChanged;
				}
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
					var renderer = Platform.GetRenderer(activityIndicator) as ActivityIndicatorRenderer;
					renderer?.PreserveState();
				}
				else
				{
					foreach (Element childElement in (element as IElementController).LogicalChildren)
						PreserveActivityIndicatorState(childElement);
				}
			}
		}
	}

	internal class HeaderWrapperView : UIView
	{
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			foreach (var item in Subviews)
				item.Frame = Bounds;
		}
	}

	internal class FormsUITableViewController : UITableViewController
	{
		ListView _list;
		UIRefreshControl _refresh;

		bool _refreshAdded;
		bool _disposed;

		public FormsUITableViewController(ListView element)
		{
			if (Forms.IsiOS9OrNewer)
				TableView.CellLayoutMarginsFollowReadableWidth = false;
			_refresh = new UIRefreshControl();
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
					_refresh.BeginRefreshing();

					//hack: when we don't have cells in our UITableView the spinner fails to appear
					CheckContentSize();

					TableView.ScrollRectToVisible(new RectangleF(0, 0, _refresh.Bounds.Width, _refresh.Bounds.Height), true);
				}
			}
			else
			{
				_refresh.EndRefreshing();

				if (!_list.IsPullToRefreshEnabled)
					RemoveRefresh();
			}
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
			if (!_list.IsRefreshing || !_refresh.Refreshing) return;

			// Restart the refreshing to get the animation to trigger
			UpdateIsRefreshing(false);
			UpdateIsRefreshing(true);
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
					_refresh.EndRefreshing();
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
		}

		void RemoveRefresh()
		{
			if (!_refreshAdded)
				return;

			if (_refresh.Refreshing)
				_refresh.EndRefreshing();

			RefreshControl = null;
			_refreshAdded = false;
		}
	}
}
