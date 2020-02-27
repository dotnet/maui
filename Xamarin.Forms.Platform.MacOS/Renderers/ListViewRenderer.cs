using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AppKit;
using Foundation;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.MacOS
{
	public class ListViewRenderer : ViewRenderer<ListView, NSView>
	{
		bool _disposed;
		NSTableView _table;
		ListViewDataSource _dataSource;
		IVisualElementRenderer _headerRenderer;
		IVisualElementRenderer _footerRenderer;

		ITemplatedItemsView<Cell> TemplatedItemsView => Element;

		bool? _defaultHorizontalScrollVisibility;
		bool? _defaultVerticalScrollVisibility;

		public const int DefaultRowHeight = 44;

		public NSTableView NativeTableView => _table;

		public override void ViewWillDraw()
		{
			UpdateHeader();
			base.ViewWillDraw();
		}

		protected virtual NSTableView CreateNSTableView(ListView list)
		{
			NSTableView table = new FormsNSTableView().AsListViewLook();
			table.Source = _dataSource = new ListViewDataSource(list, table);
			return table;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				foreach (NSView subview in Subviews)
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

				if (_headerRenderer != null)
				{
					_headerRenderer.Element?.DisposeModalAndChildRenderers();
					_headerRenderer = null;
				}
				if (_footerRenderer != null)
				{
					_footerRenderer.Element?.DisposeModalAndChildRenderers();
					_footerRenderer = null;
				}
			}

			_disposed = true;


			base.Dispose(disposing);
		}

		void DisposeSubviews(NSView view)
		{
			var ver = view as IVisualElementRenderer;

			if (ver == null)
			{
				// VisualElementRenderers should implement their own dispose methods that will appropriately dispose and remove their child views.
				// Attempting to do this work twice could cause a SIGSEGV (only observed in iOS8), so don't do this work here.
				// Non-renderer views, such as separator lines, etc., can be removed here.

				if (view is NSClipView)
					return;

				foreach (NSView subView in view.Subviews)
					DisposeSubviews(subView);

				view.RemoveFromSuperview();
			}

			view.Dispose();
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);
			if (_table == null)
				return;

			_table.BackgroundColor = color.ToNSColor(NSColor.White);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			if (e.OldElement != null)
			{
				var listView = e.OldElement;
				listView.ScrollToRequested -= OnScrollToRequested;

				var templatedItems = ((ITemplatedItemsView<Cell>)e.OldElement).TemplatedItems;
				templatedItems.CollectionChanged -= OnCollectionChanged;
				templatedItems.GroupedCollectionChanged -= OnGroupedCollectionChanged;
			}

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var scroller = new NSScrollView
					{
						AutoresizingMask = NSViewResizingMask.HeightSizable | NSViewResizingMask.WidthSizable,
						DocumentView = _table = CreateNSTableView(e.NewElement),
						HasVerticalScroller = true,
						DrawsBackground = false,
					};
					SetNativeControl(scroller);
				}

				var listView = e.NewElement;
				listView.ScrollToRequested += OnScrollToRequested;

				var templatedItems = ((ITemplatedItemsView<Cell>)e.NewElement).TemplatedItems;
				templatedItems.CollectionChanged += OnCollectionChanged;
				templatedItems.GroupedCollectionChanged += OnGroupedCollectionChanged;

				UpdateRowHeight();
				UpdateHeader();
				UpdateFooter();
				UpdatePullToRefreshEnabled();
				UpdateIsRefreshing();
				UpdateSeparatorColor();
				UpdateSeparatorVisibility();
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
			if (e.PropertyName == ListView.RowHeightProperty.PropertyName)
				UpdateRowHeight();
			else if (e.PropertyName == ListView.IsGroupingEnabledProperty.PropertyName ||
					(e.PropertyName == ListView.HasUnevenRowsProperty.PropertyName))
				_dataSource.Update();
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
			else if (e.PropertyName == ScrollView.VerticalScrollBarVisibilityProperty.PropertyName)
				UpdateVerticalScrollBarVisibility();
			else if (e.PropertyName == ScrollView.HorizontalScrollBarVisibilityProperty.PropertyName)
				UpdateHorizontalScrollBarVisibility();
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateItems(e, 0, true);
		}

		void OnGroupedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			var til = (TemplatedItemsList<ItemsView<Cell>, Cell>)sender;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			var groupIndex = templatedItems.IndexOf(til.HeaderContent);
			UpdateItems(e, groupIndex, false);
		}

		void UpdateHeader()
		{
			var header = Element.HeaderElement;
			var headerView = (View)header;

			if (headerView != null)
			{
				//Header reuse is not working for now , problem with size of something that is not inside a layout
				//if (_headerRenderer != null)
				//{
				//	var reflectableType = _headerRenderer as System.Reflection.IReflectableType;
				//	var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _headerRenderer.GetType();
				//	if (header != null && rendererType == Registrar.Registered.GetHandlerTypeForObject(header))
				//	{
				//		_headerRenderer.SetElement(headerView);
				//		_table.HeaderView = new CustomNSTableHeaderView(Bounds.Width, _headerRenderer);
				//		//	Layout();
				//		//var customNSTableHeaderView = _table.HeaderView as CustomNSTableHeaderView;
				//		//customNSTableHeaderView?.Update(Bounds.Width, _headerRenderer);
				//		//NativeView.Layout();
				//		//NativeView.SetNeedsDisplayInRect(NativeView.Bounds);
				//		//NativeView.LayoutSubtreeIfNeeded();
				//		//_table.LayoutSubtreeIfNeeded();
				//		//_table.NeedsDisplay = true;
				//		//NativeView.NeedsDisplay = true;
				//		return;
				//	}
				ClearHeader();
				//}

				_headerRenderer = Platform.CreateRenderer(headerView);
				Platform.SetRenderer(headerView, _headerRenderer);
				_table.HeaderView = new CustomNSTableHeaderView(Bounds.Width, _headerRenderer);

				//We need this since the NSSCrollView doesn't know of the new size of our header
				//TODO: Find a better solution 
				(Control as NSScrollView)?.ContentView.ScrollToPoint(new CoreGraphics.CGPoint(0, -_table.HeaderView.Frame.Height));
			}
			else if (_headerRenderer != null)
			{
				ClearHeader();
			}
		}

		void ClearHeader()
		{
			_table.HeaderView = null;
			if (_headerRenderer == null)
				return;
			_headerRenderer.Element.DisposeModalAndChildRenderers();
			_headerRenderer = null;
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
					if (e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					_table.BeginUpdates();
					_table.InsertRows(NSIndexSet.FromArray(Enumerable.Range(e.NewStartingIndex, e.NewItems.Count).ToArray()),
						NSTableViewAnimation.SlideUp);
					_table.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Remove:
					if (e.OldStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;

					_table.BeginUpdates();
					_table.RemoveRows(NSIndexSet.FromArray(Enumerable.Range(e.OldStartingIndex, e.OldItems.Count).ToArray()),
						NSTableViewAnimation.SlideDown);
					_table.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Move:
					if (e.OldStartingIndex == -1 || e.NewStartingIndex == -1 || groupReset)
						goto case NotifyCollectionChangedAction.Reset;
					_table.BeginUpdates();
					for (var i = 0; i < e.OldItems.Count; i++)
					{
						var oldi = e.OldStartingIndex;
						var newi = e.NewStartingIndex;

						if (e.NewStartingIndex < e.OldStartingIndex)
						{
							oldi += i;
							newi += i;
						}

						_table.MoveRow(oldi, newi);
					}
					_table.EndUpdates();

					break;

				case NotifyCollectionChangedAction.Replace:
				case NotifyCollectionChangedAction.Reset:
					_table.ReloadData();
					return;
			}
		}

		void UpdateRowHeight()
		{
			var rowHeight = Element.RowHeight;
			if (Element.HasUnevenRows && rowHeight == -1)
			{
				//	_table.RowHeight = NoIntrinsicMetric;
			}
			else
				_table.RowHeight = rowHeight <= 0 ? DefaultRowHeight : rowHeight;
		}

		//TODO: Implement UpdateIsRefreshing
		void UpdateIsRefreshing()
		{
		}

		//TODO: Implement PullToRefresh
		void UpdatePullToRefreshEnabled()
		{
		}

		//TODO: Implement SeparatorColor
		void UpdateSeparatorColor()
		{
		}

		//TODO: Implement UpdateSeparatorVisibility
		void UpdateSeparatorVisibility()
		{
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			var templatedItems = TemplatedItemsView.TemplatedItems;
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;

			var row = templatedItems?.GetGlobalIndexOfItem(scrollArgs.Item) ?? -1;
			if (row == -1)
				return;

			var rowRect = _table.RectForRow(row);
			var rowHeight = rowRect.Height;
			var clipView = _table.Superview as NSClipView;
			var clipViewHeight = clipView.Frame.Height;
			var scrollToPosition = e.Position;

			if (scrollToPosition == ScrollToPosition.MakeVisible)
			{
				var topVisibleY = clipView.Bounds.Y;
				var bottomVisibleY = clipView.Bounds.Y + clipViewHeight - rowHeight;

				if (topVisibleY > rowRect.Y)
				{
					scrollToPosition = ScrollToPosition.Start;
				}
				else if (bottomVisibleY < rowRect.Y)
				{
					scrollToPosition = ScrollToPosition.End;
				}
				else
				{
					return;
				}
			}

			nfloat y = 0;
			var scrollOrigin = rowRect.Location;

			if (scrollToPosition == ScrollToPosition.Center)
			{
				y = (scrollOrigin.Y - clipViewHeight / 2) + rowHeight / 2;
			}
			else if (scrollToPosition == ScrollToPosition.End)
			{
				y = scrollOrigin.Y - clipViewHeight + rowHeight;
			}
			else
			{
				y = scrollOrigin.Y;
			}

			scrollOrigin.Y = y;

			if (e.ShouldAnimate)
			{
				((NSView)clipView.Animator).SetBoundsOrigin(scrollOrigin);
			}
			else
			{
				clipView.SetBoundsOrigin(scrollOrigin);
			}
		}

		//TODO: Implement Footer
		void UpdateFooter()
		{
		}

		void UpdateVerticalScrollBarVisibility()
		{
			if (_table?.EnclosingScrollView != null)
			{
				if (_defaultVerticalScrollVisibility == null)
					_defaultVerticalScrollVisibility = _table.EnclosingScrollView.HasVerticalScroller;

				switch (Element.VerticalScrollBarVisibility)
				{
					case (ScrollBarVisibility.Always):
						_table.EnclosingScrollView.HasVerticalScroller = true;
						break;
					case (ScrollBarVisibility.Never):
						_table.EnclosingScrollView.HasVerticalScroller = false;
						break;
					case (ScrollBarVisibility.Default):
						_table.EnclosingScrollView.HasVerticalScroller = (bool)_defaultVerticalScrollVisibility;
						break;
				}
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_table?.EnclosingScrollView != null)
			{
				if (_defaultHorizontalScrollVisibility == null)
					_defaultHorizontalScrollVisibility = _table.EnclosingScrollView.HasHorizontalScroller;

				switch (Element.HorizontalScrollBarVisibility)
				{
					case (ScrollBarVisibility.Always):
						_table.EnclosingScrollView.HasHorizontalScroller = true;
						break;
					case (ScrollBarVisibility.Never):
						_table.EnclosingScrollView.HasHorizontalScroller = false;
						break;
					case (ScrollBarVisibility.Default):
						_table.EnclosingScrollView.HasHorizontalScroller = (bool)_defaultHorizontalScrollVisibility;
						break;
				}
			}
		}

		class FormsNSTableView : NSTableView
		{
			//NSTableView doesn't support selection notfications after the items is already selected
			//so we do it ourselves by hooking mouse down event
			public override void MouseDown(NSEvent theEvent)
			{
				var clickLocation = ConvertPointFromView(theEvent.LocationInWindow, null);
				var clickedRow = GetRow(clickLocation);

				base.MouseDown(theEvent);
				if (clickedRow != -1)
					(Source as ListViewDataSource)?.OnRowClicked();
			}

			public override bool ValidateProposedFirstResponder(NSResponder responder, NSEvent forEvent)
			{
				return true;
			}
		}
	}
}