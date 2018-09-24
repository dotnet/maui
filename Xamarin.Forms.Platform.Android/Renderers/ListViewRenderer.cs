using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Support.V4.Widget;
using Android.Views;
using AListView = Android.Widget.ListView;
using AView = Android.Views.View;
using Xamarin.Forms.Internals;
using System;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class ListViewRenderer : ViewRenderer<ListView, AListView>, SwipeRefreshLayout.IOnRefreshListener
	{
		ListViewAdapter _adapter;
		bool _disposed;
		IVisualElementRenderer _headerRenderer;
		IVisualElementRenderer _footerRenderer;
		Container _headerView;
		Container _footerView;
		bool _isAttached;
		ScrollToRequestedEventArgs _pendingScrollTo;

		SwipeRefreshLayout _refresh;
		IListViewController Controller => Element;
		ITemplatedItemsView<Cell> TemplatedItemsView => Element;

		public ListViewRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ListViewRenderer(Context) instead.")]
		public ListViewRenderer()
		{
			AutoPackage = false;
		}

		void SwipeRefreshLayout.IOnRefreshListener.OnRefresh()
		{
			IListViewController controller = Element;
			controller.SendRefreshing();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (_headerRenderer != null)
				{
					Platform.ClearRenderer(_headerRenderer.View);
					_headerRenderer.Dispose();
					_headerRenderer = null;
				}

				_headerView?.Dispose();
				_headerView = null;

				if (_footerRenderer != null)
				{
					Platform.ClearRenderer(_footerRenderer.View);
					_footerRenderer.Dispose();
					_footerRenderer = null;
				}

				_footerView?.Dispose();
				_footerView = null;

				// Unhook the adapter from the ListView before disposing of it
				if (Control != null)
				{
					Control.Adapter = null;
				}

				if (_adapter != null)
				{
					_adapter.Dispose();
					_adapter = null;
				}

				Controller.ScrollToRequested -= OnScrollToRequested;
			}

			base.Dispose(disposing);
		}

		protected override Size MinimumSize()
		{
			return new Size(40, 40);
		}

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			_isAttached = true;
			_adapter.IsAttachedToWindow = _isAttached;
			UpdateIsRefreshing(isInitialValue: true);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();

			_isAttached = false;
			_adapter.IsAttachedToWindow = _isAttached;
		}

		protected override AListView CreateNativeControl()
		{
			return new AListView(Context);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<ListView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				((IListViewController)e.OldElement).ScrollToRequested -= OnScrollToRequested;

				if (_adapter != null)
				{
					// Unhook the adapter from the ListView before disposing of it
					if (Control != null)
					{
						Control.Adapter = null;
					}

					_adapter.Dispose();
					_adapter = null;
				}
			}

			if (e.NewElement != null)
			{
				AListView nativeListView = Control;
				if (nativeListView == null)
				{
					var ctx = Context;
					nativeListView = CreateNativeControl();
					_refresh = new SwipeRefreshLayout(ctx);
					_refresh.SetOnRefreshListener(this);
					_refresh.AddView(nativeListView, LayoutParams.MatchParent);
					SetNativeControl(nativeListView, _refresh);

					_headerView = new Container(ctx);
					nativeListView.AddHeaderView(_headerView, null, false);
					_footerView = new Container(ctx);
					nativeListView.AddFooterView(_footerView, null, false);
				}

				((IListViewController)e.NewElement).ScrollToRequested += OnScrollToRequested;

				nativeListView.DividerHeight = 0;
				nativeListView.Focusable = false;
				nativeListView.DescendantFocusability = DescendantFocusability.AfterDescendants;
				nativeListView.OnFocusChangeListener = this;
				nativeListView.Adapter = _adapter = e.NewElement.IsGroupingEnabled && e.NewElement.OnThisPlatform().IsFastScrollEnabled() ? new GroupedListViewAdapter(Context, nativeListView, e.NewElement) : new ListViewAdapter(Context, nativeListView, e.NewElement);
				_adapter.HeaderView = _headerView;
				_adapter.FooterView = _footerView;
				_adapter.IsAttachedToWindow = _isAttached;

				UpdateHeader();
				UpdateFooter();
				UpdateIsSwipeToRefreshEnabled();
				UpdateFastScrollEnabled();
				UpdateSelectionMode();
			}
		}

		internal void LongClickOn(AView viewCell)
		{
			if (Control == null)
			{
				return;
			}

			var position = Control.GetPositionForView(viewCell);
			var id = Control.GetItemIdAtPosition(position);

			viewCell.PerformHapticFeedback(FeedbackConstants.ContextClick);
			_adapter.OnItemLongClick(Control, viewCell, position, id);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == "HeaderElement")
				UpdateHeader();
			else if (e.PropertyName == "FooterElement")
				UpdateFooter();
			else if (e.PropertyName == "RefreshAllowed")
				UpdateIsSwipeToRefreshEnabled();
			else if (e.PropertyName == ListView.IsPullToRefreshEnabledProperty.PropertyName)
				UpdateIsSwipeToRefreshEnabled();
			else if (e.PropertyName == ListView.IsRefreshingProperty.PropertyName)
				UpdateIsRefreshing();
			else if (e.PropertyName == ListView.SeparatorColorProperty.PropertyName || e.PropertyName == ListView.SeparatorVisibilityProperty.PropertyName)
				_adapter.NotifyDataSetChanged();
			else if (e.PropertyName == PlatformConfiguration.AndroidSpecific.ListView.IsFastScrollEnabledProperty.PropertyName)
				UpdateFastScrollEnabled();
			else if (e.PropertyName == ListView.SelectionModeProperty.PropertyName)
				UpdateSelectionMode();
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);

			if (_pendingScrollTo != null)
			{
				OnScrollToRequested(this, _pendingScrollTo);
				_pendingScrollTo = null;
			}
		}

		void OnScrollToRequested(object sender, ScrollToRequestedEventArgs e)
		{
			if (!_isAttached)
			{
				_pendingScrollTo = e;
				return;
			}

			Cell cell;
			int position;
			var scrollArgs = (ITemplatedItemsListScrollToRequestedEventArgs)e;

			var templatedItems = TemplatedItemsView.TemplatedItems;
			if (Element.IsGroupingEnabled)
			{
				var results = templatedItems.GetGroupAndIndexOfItem(scrollArgs.Group, scrollArgs.Item);
				if (results.Item1 == -1 || results.Item2 == -1)
					return;

				var group = templatedItems.GetGroup(results.Item1);
				cell = group[results.Item2];

				position = templatedItems.GetGlobalIndexForGroup(group) + results.Item2 + 1;
			}
			else
			{
				position = templatedItems.GetGlobalIndexOfItem(scrollArgs.Item);
				if (position == -1)
					return;

				cell = templatedItems[position];
			}

			//Android offsets position of cells when using header
			int realPositionWithHeader = position + 1;

			if (e.Position == ScrollToPosition.MakeVisible)
			{
				if (e.ShouldAnimate)
					Control.SmoothScrollToPosition(realPositionWithHeader);
				else
					Control.SetSelection(realPositionWithHeader);
				return;
			}

			int height = Control.Height;
			var cellHeight = (int)cell.RenderHeight;
			if (cellHeight == -1)
			{
				int first = Control.FirstVisiblePosition;
				if (first <= position && position <= Control.LastVisiblePosition)
					cellHeight = Control.GetChildAt(position - first).Height;
				else
				{
					AView view = _adapter.GetView(position, null, null);
					view.Measure(MeasureSpecFactory.MakeMeasureSpec(Control.Width, MeasureSpecMode.AtMost), MeasureSpecFactory.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
					cellHeight = view.MeasuredHeight;
				}
			}

			var y = 0;

			if (e.Position == ScrollToPosition.Center)
				y = height / 2 - cellHeight / 2;
			else if (e.Position == ScrollToPosition.End)
				y = height - cellHeight;

			if (e.ShouldAnimate)
				Control.SmoothScrollToPositionFromTop(realPositionWithHeader, y);
			else
				Control.SetSelectionFromTop(realPositionWithHeader, y);
		}

		void UpdateFooter()
		{
			var footer = (VisualElement)Controller.FooterElement;
			if (_footerRenderer != null)
			{
				var reflectableType = _footerRenderer as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _footerRenderer.GetType();
				if (footer == null || Registrar.Registered.GetHandlerTypeForObject(footer) != rendererType)
				{
					if (_footerView != null)
						_footerView.Child = null;
					Platform.ClearRenderer(_footerRenderer.View);
					_footerRenderer.Dispose();
					_footerRenderer = null;
				}
			}

			if (footer == null)
				return;

			if (_footerRenderer != null)
				_footerRenderer.SetElement(footer);
			else
			{
				_footerRenderer = Platform.CreateRenderer(footer, Context);
				if (_footerView != null)
					_footerView.Child = _footerRenderer;
			}

			Platform.SetRenderer(footer, _footerRenderer);
		}

		void UpdateHeader()
		{
			var header = (VisualElement)Controller.HeaderElement;
			if (_headerRenderer != null)
			{
				var reflectableType = _headerRenderer as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : _headerRenderer.GetType();
				if (header == null || Registrar.Registered.GetHandlerTypeForObject(header) != rendererType)
				{
					if (_headerView != null)
						_headerView.Child = null;
					Platform.ClearRenderer(_headerRenderer.View);
					_headerRenderer.Dispose();
					_headerRenderer = null;
				}
			}

			if (header == null)
				return;

			if (_headerRenderer != null)
				_headerRenderer.SetElement(header);
			else
			{
				_headerRenderer = Platform.CreateRenderer(header, Context);
				if (_headerView != null)
					_headerView.Child = _headerRenderer;
			}

			Platform.SetRenderer(header, _headerRenderer);
		}

		void UpdateIsRefreshing(bool isInitialValue = false)
		{
			if (_refresh != null)
			{
				var isRefreshing = Element.IsRefreshing;
				if (isRefreshing && isInitialValue)
				{
					_refresh.Refreshing = false;
					_refresh.Post(() =>
					{
						_refresh.Refreshing = true;
					});
				}
				else
					_refresh.Refreshing = isRefreshing;
			}
		}

		void UpdateIsSwipeToRefreshEnabled()
		{
			if (_refresh != null)
				_refresh.Enabled = Element.IsPullToRefreshEnabled && (Element as IListViewController).RefreshAllowed;
		}

		void UpdateFastScrollEnabled()
		{
			if (Control != null)
			{
				Control.FastScrollEnabled = Element.OnThisPlatform().IsFastScrollEnabled();
			}
		}

		void UpdateSelectionMode()
		{
			if (Control != null)
			{
				if (Element.SelectionMode == ListViewSelectionMode.None)
				{
					Control.ChoiceMode = ChoiceMode.None;
					Element.SelectedItem = null;
				}
				else if (Element.SelectionMode == ListViewSelectionMode.Single)
				{
					Control.ChoiceMode = ChoiceMode.Single;
				}
			}
		}

		internal class Container : ViewGroup
		{
			IVisualElementRenderer _child;

			public Container(IntPtr p, global::Android.Runtime.JniHandleOwnership o) : base(p, o)
			{
				// Added default constructor to prevent crash when accessing header/footer row in ListViewAdapter.Dispose
			}

			public Container(Context context) : base(context)
			{
			}

			public IVisualElementRenderer Child
			{
				set
				{
					if (_child != null)
						RemoveView(_child.View);

					_child = value;

					if (value != null)
						AddView(value.View);
				}
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				if (_child == null)
					return;

				_child.UpdateLayout();
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				if (_child == null)
				{
					SetMeasuredDimension(0, 0);
					return;
				}

				VisualElement element = _child.Element;

				Context ctx = Context;

				var width = (int)ctx.FromPixels(MeasureSpecFactory.GetSize(widthMeasureSpec));

				SizeRequest request = _child.Element.Measure(width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(_child.Element, new Rectangle(0, 0, width, request.Request.Height));

				int widthSpec = MeasureSpecFactory.MakeMeasureSpec((int)ctx.ToPixels(width), MeasureSpecMode.Exactly);
				int heightSpec = MeasureSpecFactory.MakeMeasureSpec((int)ctx.ToPixels(request.Request.Height), MeasureSpecMode.Exactly);

				_child.View.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(widthSpec, heightSpec);
			}
		}
	}
}
