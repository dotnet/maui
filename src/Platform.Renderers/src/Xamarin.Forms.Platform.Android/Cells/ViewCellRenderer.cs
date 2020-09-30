using System;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ViewCellRenderer : CellRenderer
	{
		protected override AView GetCellCore(Cell item, AView convertView, ViewGroup parent, Context context)
		{
			Performance.Start(out string reference, "GetCellCore");
			var cell = (ViewCell)item;

			var container = convertView as ViewCellContainer;
			if (container != null)
			{
				container.Update(cell);
				Performance.Stop(reference, "GetCellCore");
				return container;
			}

			BindableProperty unevenRows = null, rowHeight = null;
			if (ParentView is TableView)
			{
				unevenRows = TableView.HasUnevenRowsProperty;
				rowHeight = TableView.RowHeightProperty;
			}
			else if (ParentView is ListView)
			{
				cell.IsContextActionsLegacyModeEnabled = item.On<PlatformConfiguration.Android>().GetIsContextActionsLegacyModeEnabled();

				unevenRows = ListView.HasUnevenRowsProperty;
				rowHeight = ListView.RowHeightProperty;
			}

			if (cell.View == null)
				throw new InvalidOperationException($"ViewCell must have a {nameof(cell.View)}");

			IVisualElementRenderer view = Platform.CreateRenderer(cell.View, context);
			Platform.SetRenderer(cell.View, view);
			cell.View.IsPlatformEnabled = true;
			var c = new ViewCellContainer(context, view, cell, ParentView, unevenRows, rowHeight);

			Performance.Stop(reference, "GetCellCore");

			return c;
		}

		internal class ViewCellContainer : ViewGroup, INativeElementView
		{
			readonly View _parent;
			readonly BindableProperty _rowHeight;
			readonly BindableProperty _unevenRows;
			IVisualElementRenderer _view;
			ViewCell _viewCell;
			GestureDetector _tapGestureDetector;
			GestureDetector _longPressGestureDetector;
			ListViewRenderer _listViewRenderer;
			bool _watchForLongPress;

			ListViewRenderer ListViewRenderer
			{
				get
				{
					if (_listViewRenderer != null)
					{
						return _listViewRenderer;
					}

					var listView = _parent as ListView;

					if (listView == null)
					{
						return null;
					}

					_listViewRenderer = Platform.GetRenderer(listView) as ListViewRenderer;

					return _listViewRenderer;
				}
			}

			GestureDetector TapGestureDetector
			{
				get
				{
					if (_tapGestureDetector != null)
					{
						return _tapGestureDetector;
					}

					_tapGestureDetector = new GestureDetector(Context, new TapGestureListener(TriggerClick));
					return _tapGestureDetector;
				}
			}

			GestureDetector LongPressGestureDetector
			{
				get
				{
					if (_longPressGestureDetector != null)
					{
						return _longPressGestureDetector;
					}

					_longPressGestureDetector = new GestureDetector(Context, new LongPressGestureListener(TriggerLongClick));
					return _longPressGestureDetector;
				}
			}

			public ViewCellContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
			{
				// Added default constructor to prevent crash when accessing selected row in ListViewAdapter.Dispose
			}

			public ViewCellContainer(Context context, IVisualElementRenderer view, ViewCell viewCell, View parent,
				BindableProperty unevenRows, BindableProperty rowHeight) : base(context)
			{
				_view = view;
				_parent = parent;
				_unevenRows = unevenRows;
				_rowHeight = rowHeight;
				_viewCell = viewCell;
				AddView(view.View);
				UpdateIsEnabled();
				UpdateWatchForLongPress();
			}

			protected bool ParentHasUnevenRows
			{
				get { return (bool)_parent.GetValue(_unevenRows); }
			}

			protected int ParentRowHeight
			{
				get { return (int)_parent.GetValue(_rowHeight); }
			}

			public Element Element
			{
				get { return _viewCell; }
			}

			public override bool OnInterceptTouchEvent(MotionEvent ev)
			{
				if (!Enabled)
					return true;

				return base.OnInterceptTouchEvent(ev);
			}

			public override bool DispatchTouchEvent(MotionEvent e)
			{
				// Give the child controls a shot at the event (in case they've get Tap gestures and such
				var handled = base.DispatchTouchEvent(e);

				if (_watchForLongPress)
				{
					// Feed the gesture through the LongPress detector; for this to work we *must* return true 
					// afterward (or the LPGD goes nuts and immediately fires onLongPress)
					LongPressGestureDetector.OnTouchEvent(e);
					return true;
				}

				if (WatchForSwipeViewTap())
				{
					TapGestureDetector.OnTouchEvent(e);
					return true;
				}

				return handled;
			}

			public void Update(ViewCell cell)
			{
				Performance.Start(out string reference);
				var renderer = GetChildAt(0) as IVisualElementRenderer;
				var viewHandlerType = Registrar.Registered.GetHandlerTypeForObject(cell.View) ?? typeof(Platform.DefaultRenderer);
				var reflectableType = renderer as System.Reflection.IReflectableType;
				var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : (renderer != null ? renderer.GetType() : typeof(System.Object));
				if (renderer != null && rendererType == viewHandlerType)
				{
					Performance.Start(reference, "Reuse");
					_viewCell = cell;

					cell.View.DisableLayout = true;
					foreach (VisualElement c in cell.View.Descendants())
						c.DisableLayout = true;

					Performance.Start(reference, "Reuse.SetElement");
					renderer.SetElement(cell.View);
					Performance.Stop(reference, "Reuse.SetElement");

					Platform.SetRenderer(cell.View, _view);

					cell.View.DisableLayout = false;
					foreach (VisualElement c in cell.View.Descendants())
						c.DisableLayout = false;

					var viewAsLayout = cell.View as Layout;
					if (viewAsLayout != null)
						viewAsLayout.ForceLayout();

					Invalidate();

					Performance.Stop(reference, "Reuse");
					Performance.Stop(reference);
					return;
				}

				RemoveView(_view.View);
				Platform.SetRenderer(_viewCell.View, null);
				_viewCell.View.IsPlatformEnabled = false;
				_view.View.Dispose();

				_viewCell = cell;
				_view = Platform.CreateRenderer(_viewCell.View, Context);

				Platform.SetRenderer(_viewCell.View, _view);
				AddView(_view.View);

				UpdateIsEnabled();
				UpdateWatchForLongPress();

				Performance.Stop(reference);
			}

			public void UpdateIsEnabled()
			{
				Enabled = _viewCell.IsEnabled;
			}

			protected override void OnLayout(bool changed, int l, int t, int r, int b)
			{
				Performance.Start(out string reference);

				double width = Context.FromPixels(r - l);
				double height = Context.FromPixels(b - t);

				Performance.Start(reference, "Element.Layout");
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(_view.Element, new Rectangle(0, 0, width, height));
				Performance.Stop(reference, "Element.Layout");

				_view.UpdateLayout();
				Performance.Stop(reference);
			}

			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				Performance.Start(out string reference);

				int width = MeasureSpec.GetSize(widthMeasureSpec);
				int height;

				if (ParentHasUnevenRows)
				{
					SizeRequest measure = _view.Element.Measure(Context.FromPixels(width), double.PositiveInfinity, MeasureFlags.IncludeMargins);
					height = (int)Context.ToPixels(_viewCell.Height > 0 ? _viewCell.Height : measure.Request.Height);
				}
				else
					height = (int)Context.ToPixels(ParentRowHeight == -1 ? BaseCellView.DefaultMinHeight : ParentRowHeight);

				SetMeasuredDimension(width, height);

				Performance.Stop(reference);
			}

			bool WatchForSwipeViewTap()
			{
				if (!(_view.Element is SwipeView swipeView))
				{
					return false;
				}
				// If the cell contains a SwipeView, we will have conflicts capturing the touch.
				// So we need to watch locally for Tap and if we see it (and the SwipeView is open),
				// trigger the Click manually.
				if (!((ISwipeViewController)swipeView).IsOpen)
				{
					return true;
				}

				return false;
			}

			void UpdateWatchForLongPress()
			{
				var vw = _view.Element as Xamarin.Forms.View;
				if (vw == null)
				{
					return;
				}

				// If the view cell has any context actions and the View itself has any Tap Gestures, they're going
				// to conflict with one another - the Tap Gesture handling will prevent the ListViewAdapter's
				// LongClick handling from happening. So we need to watch locally for LongPress and if we see it,
				// trigger the LongClick manually.
				_watchForLongPress = _viewCell.ContextActions.Count > 0
					&& HasTapGestureRecognizers(vw);
			}

			static bool HasTapGestureRecognizers(View view)
			{
				return view.GestureRecognizers.Any(t => t is TapGestureRecognizer)
					|| view.LogicalChildren.OfType<View>().Any(HasTapGestureRecognizers);
			}

			void TriggerClick()
			{
				ListViewRenderer?.ClickOn(this);
			}

			void TriggerLongClick()
			{
				ListViewRenderer?.LongClickOn(this);
			}

			internal class TapGestureListener : Java.Lang.Object, GestureDetector.IOnGestureListener
			{
				readonly Action _onClick;

				internal TapGestureListener(Action onClick)
				{
					_onClick = onClick;
				}

				internal TapGestureListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
				{
				}

				public bool OnDown(MotionEvent e)
				{
					return true;
				}

				public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
				{
					return false;
				}

				public void OnLongPress(MotionEvent e)
				{

				}

				public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
				{
					return false;
				}

				public void OnShowPress(MotionEvent e)
				{

				}

				public bool OnSingleTapUp(MotionEvent e)
				{
					_onClick();
					return false;
				}
			}

			internal class LongPressGestureListener : Java.Lang.Object, GestureDetector.IOnGestureListener
			{
				readonly Action _onLongClick;

				internal LongPressGestureListener(Action onLongClick)
				{
					_onLongClick = onLongClick;
				}

				internal LongPressGestureListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
				{
				}

				public bool OnDown(MotionEvent e)
				{
					return true;
				}

				public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
				{
					return false;
				}

				public void OnLongPress(MotionEvent e)
				{
					_onLongClick();
				}

				public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
				{
					return false;
				}

				public void OnShowPress(MotionEvent e)
				{

				}

				public bool OnSingleTapUp(MotionEvent e)
				{
					return false;
				}
			}
		}
	}
}