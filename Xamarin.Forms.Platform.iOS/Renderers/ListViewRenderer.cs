using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
		bool _shouldEstimateRowHeight = true;
			
		FormsUITableViewController _tableViewController;
		IListViewController Controller => Element;
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;
		public override UIViewController ViewController => _tableViewController;

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

		protected override void Dispose(bool disposing)
		{
			// check inset tracker for null to 
			if (disposing && _insetTracker != null)
			{
				_insetTracker.Dispose();
				_insetTracker = null;

				var viewsToLookAt = new Stack<UIView>(Subviews);
				while (viewsToLookAt.Count > 0)
				{
					var view = viewsToLookAt.Pop();
					var viewCellRenderer = view as ViewCellRenderer.ViewTableCell;
					if (viewCellRenderer != null)
						viewCellRenderer.Dispose();
					else
					{
						foreach (var child in view.Subviews)
							viewsToLookAt.Push(child);
					}
				}

				if (Element != null)
				{
					var templatedItems = TemplatedItemsView.TemplatedItems;
					templatedItems.CollectionChanged -= OnCollectionChanged;
					templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
				}

				if (_tableViewController != null)
				{
					_tableViewController.Dispose();
					_tableViewController = null;
				}
			}

			if (disposing)
			{
				if (_headerRenderer != null)
				{
					var platform = _headerRenderer.Element.Platform as Platform;
					if (platform != null)
						platform.DisposeModelAndChildrenRenderers(_headerRenderer.Element);
					_headerRenderer = null;
				}
				if (_footerRenderer != null)
				{
					var platform = _footerRenderer.Element.Platform as Platform;
					if (platform != null)
						platform.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
					_footerRenderer = null;
				}

				var headerView = Controller?.HeaderElement as VisualElement;
				if (headerView != null)
					headerView.MeasureInvalidated -= OnHeaderMeasureInvalidated;
				Control?.TableHeaderView?.Dispose();

				var footerView = Controller?.FooterElement as VisualElement;
				if (footerView != null)
					footerView.MeasureInvalidated -= OnFooterMeasureInvalidated;
				Control?.TableFooterView?.Dispose();
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			_requestedScroll = null;

			if (e.OldElement != null)
			{
				var controller = (IListViewController)e.OldElement;
				var headerView = (VisualElement)controller.HeaderElement;
				if (headerView != null)
					headerView.MeasureInvalidated -= OnHeaderMeasureInvalidated;

				var footerView = (VisualElement)controller.FooterElement;
				if (footerView != null)
					footerView.MeasureInvalidated -= OnFooterMeasureInvalidated;

				controller.ScrollToRequested -= OnScrollToRequested;
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
				_shouldEstimateRowHeight = true;

				var controller = (IListViewController)e.NewElement;

				controller.ScrollToRequested += OnScrollToRequested;
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
			if (e.PropertyName == ListView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == ListView.IsGroupingEnabledProperty.PropertyName)
				_dataSource.UpdateGrouping();
			else if (e.PropertyName == ListView.HasUnevenRowsProperty.PropertyName)
			{
				_estimatedRowHeight = false;
				Control.Source = _dataSource = Element.HasUnevenRows ? new UnevenListViewDataSource(_dataSource) : new ListViewDataSource(_dataSource);
			}
			else if (e.PropertyName == ListView.IsPullToRefreshEnabledProperty.PropertyName)
				UpdatePullToRefreshEnabled();
			else if (e.PropertyName == ListView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.PropertyName == ListView.SeparatorColorProperty.PropertyName)
				UpdateSeparatorColor();
			else if (e.PropertyName == ListView.SeparatorVisibilityProperty.PropertyName)
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
					Control.ScrollToRow(NSIndexPath.FromRowSection(index, 0), position, e.ShouldAnimate);
				}
			}
		}

		void UpdateEstimatedRowHeight()
		{
			if (_estimatedRowHeight)
				return;

			var rowHeight = Element.RowHeight;
			if (Element.HasUnevenRows && rowHeight == -1)
			{
				var source = _dataSource as UnevenListViewDataSource;
				if (_shouldEstimateRowHeight)
				{
					if (source != null)
					{
						Control.EstimatedRowHeight = source.GetEstimatedRowHeight(Control);
						_estimatedRowHeight = true;
					}
					else
					{
						//We need to set a default estimated row height, because re-setting it later(when we have items on the TIL)
						//will cause the UITableView to reload, and throw an Exception
						Control.EstimatedRowHeight = DefaultRowHeight;
					}
				}
			}
			else
			{
				if (Forms.IsiOS7OrNewer)
					Control.EstimatedRowHeight = 0;
				_estimatedRowHeight = true;
			}
		}

		void UpdateFooter()
		{
			var footer = Controller.FooterElement;
			var footerView = (View)footer;

			if (footerView != null)
			{
				if (_footerRenderer != null)
				{
					_footerRenderer.Element.MeasureInvalidated -= OnFooterMeasureInvalidated;
					if (footer != null && _footerRenderer.GetType() == Registrar.Registered.GetHandlerType(footer.GetType()))
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
				var platform = _footerRenderer.Element.Platform as Platform;
				if (platform != null)
					platform.DisposeModelAndChildrenRenderers(_footerRenderer.Element);
				_footerRenderer.Dispose();
				_footerRenderer = null;
			}
		}

		void UpdateHeader()
		{
			var header = Controller.HeaderElement;
			var headerView = (View)header;

			if (headerView != null)
			{
				if (_headerRenderer != null)
				{
					_headerRenderer.Element.MeasureInvalidated -= OnHeaderMeasureInvalidated;
					if (header != null && _headerRenderer.GetType() == Registrar.Registered.GetHandlerType(header.GetType()))
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

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:

					UpdateEstimatedRowHeight();
					if (e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					Control.BeginUpdates();
					Control.InsertRows(GetPaths(section, e.NewStartingIndex, e.NewItems.Count), UITableViewRowAnimation.Automatic);
					Control.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					Control.BeginUpdates();
					Control.DeleteRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), UITableViewRowAnimation.Automatic);

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
					Control.ReloadRows(GetPaths(section, e.OldStartingIndex, e.OldItems.Count), UITableViewRowAnimation.Automatic);
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
				var isPullToRequestEnabled = Element.IsPullToRefreshEnabled && Controller.RefreshAllowed;
				_tableViewController.UpdatePullToRefreshEnabled(isPullToRequestEnabled);
			}
		}

		void UpdateRowHeight()
		{
			var rowHeight = Element.RowHeight;
			
			if (Element.HasUnevenRows && rowHeight == -1 && Forms.IsiOS8OrNewer)
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
				var firstCell = templatedItems.First();

				if (firstCell.Height > 0) 
				{
					// Seems like we've got cells which already specify their height; since the heights are known,
					// we don't need to use estimatedRowHeight at all; zero will disable it and use the known heights
					return 0; 
				}

				return CalculateHeightForCell(table, firstCell);
			}

			public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
			{
				var cell = GetCellForPath(indexPath);

				if (List.RowHeight == -1 && cell.Height == -1 && cell is ViewCell)
				{
					// only doing ViewCell because its the only one that matters (the others dont adjust ANYWAY)
					if (Forms.IsiOS8OrNewer)
						return UITableView.AutomaticDimension;
					return CalculateHeightForCell(tableView, cell);
				}

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
					{
						_prototype = Platform.CreateRenderer(target);
						Platform.SetRenderer(target, _prototype);
					}
					else
					{
						_prototype.SetElement(target);
						Platform.SetRenderer(target, _prototype);
					}

					var req = target.Measure(tableView.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);

					target.ClearValue(Platform.RendererProperty);
					foreach (var descendant in target.Descendants())
						descendant.ClearValue(Platform.RendererProperty);

					return (nfloat)req.Request.Height;
				}
				var renderHeight = cell.RenderHeight;
				return renderHeight > 0 ? (nfloat)renderHeight : DefaultRowHeight;
			}
		}

		internal class ListViewDataSource : UITableViewSource
		{
			const int DefaultItemTemplateId = 1;
			static int s_dataTemplateIncrementer = 2; // lets start at not 0 because
			readonly nfloat _defaultSectionHeight;
			readonly Dictionary<DataTemplate, int> _templateToId = new Dictionary<DataTemplate, int>();
			readonly UITableView _uiTableView;
			readonly FormsUITableViewController _uiTableViewController;
			protected readonly ListView List;
			IListViewController Controller => List;
			protected ITemplatedItemsView<Cell> TemplatedItemsView => List;
			bool _isDragging;
			bool _selectionFromNative;

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
				_defaultSectionHeight = Forms.IsiOS8OrNewer ? DefaultRowHeight : _uiTableView.SectionHeaderHeight;
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
				UITableViewCell nativeCell = null;

				var cachingStrategy = Controller.CachingStrategy;
				if (cachingStrategy == ListViewCachingStrategy.RetainElement)
				{
					var cell = GetCellForPath(indexPath);
					nativeCell = CellTableViewCell.GetNativeCell(tableView, cell);
				}
				else if (cachingStrategy == ListViewCachingStrategy.RecycleElement)
				{
					var id = TemplateIdForPath(indexPath);
					nativeCell = tableView.DequeueReusableCell(ContextActionsCell.Key + id);
					if (nativeCell == null)
					{
						var cell = GetCellForPath(indexPath);
						nativeCell = CellTableViewCell.GetNativeCell(tableView, cell, true, id.ToString());
					}
					else
					{
						var templatedList = TemplatedItemsView.TemplatedItems.GetGroup(indexPath.Section);
						var cell = (Cell)((INativeElementView)nativeCell).Element;
						ICellController controller = cell;
						controller.SendDisappearing();
						templatedList.UpdateContent(cell, indexPath.Row);
						controller.SendAppearing();
					}
				}
				else
					throw new NotSupportedException();

				var bgColor = tableView.IndexPathForSelectedRow != null && tableView.IndexPathForSelectedRow.Equals(indexPath) ? UIColor.Clear : DefaultBackgroundColor;

				SetCellBackgroundColor(nativeCell, bgColor);

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
				if (List.IsGroupingEnabled && List.GroupHeaderTemplate != null)
				{
					var cell = TemplatedItemsView.TemplatedItems[(int)section];
					if (cell.HasContextActions)
						throw new NotSupportedException("Header cells do not support context actions");

					var renderer = (CellRenderer)Registrar.Registered.GetHandler(cell.GetType());

					var view = new HeaderWrapperView();
					view.AddSubview(renderer.GetCell(cell, null, tableView));

					return view;
				}

				return null;
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
				if (Controller.CachingStrategy == ListViewCachingStrategy.RecycleElement)
					formsCell = (Cell)((INativeElementView)cell).Element;

				SetCellBackgroundColor(cell, UIColor.Clear);

				_selectionFromNative = true;

				tableView.EndEditing(true);
				Controller.NotifyRowTapped(indexPath.Section, indexPath.Row, formsCell);
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

			public override string TitleForHeader(UITableView tableView, nint section)
			{
				if (!List.IsGroupingEnabled)
					return null;

				var sl = GetSectionList((int)section);
				sl.PropertyChanged -= OnSectionPropertyChanged;
				sl.PropertyChanged += OnSectionPropertyChanged;

				return sl.Name;
			}

			public void UpdateGrouping()
			{
				UpdateShortNameListener();
				_uiTableView.ReloadData();
			}

			protected Cell GetCellForPath(NSIndexPath indexPath)
			{
				var templatedItems = TemplatedItemsView.TemplatedItems;
				if (List.IsGroupingEnabled)
					templatedItems = (TemplatedItemsList<ItemsView<Cell>, Cell>)((IList)templatedItems)[indexPath.Section];

				var cell = templatedItems[indexPath.Row];
				return cell;
			}

			ITemplatedItemsList<Cell> GetSectionList(int section)
			{
				return (ITemplatedItemsList<Cell>)((IList)TemplatedItemsView.TemplatedItems)[section];
			}

			void OnSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				var currentSelected = _uiTableView.IndexPathForSelectedRow;

				var til = (TemplatedItemsList<ItemsView<Cell>, Cell>)sender;
				var groupIndex = ((IList)TemplatedItemsView.TemplatedItems).IndexOf(til);
				if (groupIndex == -1)
				{
					til.PropertyChanged -= OnSectionPropertyChanged;
					return;
				}

				_uiTableView.ReloadSections(NSIndexSet.FromIndex(groupIndex), UITableViewRowAnimation.Automatic);
				_uiTableView.SelectRow(currentSelected, false, UITableViewScrollPosition.None);
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

				var templatedList = TemplatedItemsView.TemplatedItems;
				if (List.IsGroupingEnabled)
					templatedList = (TemplatedItemsList<ItemsView<Cell>, Cell>)((IList)templatedList)[indexPath.Section];

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
		readonly ListView _list;
		IListViewController Controller => _list;
		UIRefreshControl _refresh;

		bool _refreshAdded;

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
			else if (_refreshAdded)
			{
				if (_refresh.Refreshing)
					_refresh.EndRefreshing();

				RefreshControl = null;
				_refreshAdded = false;
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
		    if (!_list.IsRefreshing || !_refresh.Refreshing) return;

		    // Restart the refreshing to get the animation to trigger
		    UpdateIsRefreshing(false);
		    UpdateIsRefreshing(true);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing && _refresh != null)
			{
				_refresh.ValueChanged -= OnRefreshingChanged;
				_refresh.EndRefreshing();
				_refresh.Dispose();
				_refresh = null;
			}
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
				Controller.SendRefreshing();
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
