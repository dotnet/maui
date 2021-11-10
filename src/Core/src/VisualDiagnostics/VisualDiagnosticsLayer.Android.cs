using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		public bool DisableUITouchEventPassthrough { get; set; }

		public NativeGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }
		Activity? _nativeActivity;
		private HashSet<Tuple<IScrollView, Android.Views.View>> _scrollViews = new HashSet<Tuple<IScrollView, Android.Views.View>>();

		public void AddScrollableElementHandlers()
		{
			if (this.Window == null)
				return;
			var content = this.Window.Content as IVisualTreeElement;
			if (content == null)
				return;

			var scrollBars = content.GetEntireVisualTreeElementChildren().Where(n => n is IScrollView).Cast<IScrollView>();
			foreach(var scrollBar in scrollBars)
			{
				if (!_scrollViews.Any(x => x.Item1 == scrollBar))
				{
					var nativeScroll = ((IScrollView)scrollBar).GetNative(true);
					if (nativeScroll != null)
					{
						nativeScroll.ScrollChange += scroll_ScrollChange;
						this._scrollViews.Add(new Tuple<IScrollView, View>(scrollBar, nativeScroll));
					}
				}
			}
		}

		private void scroll_ScrollChange(object? sender, View.ScrollChangeEventArgs e)
		{
			this.Invalidate();
		}

		public void RemoveScrollableElementHandler()
		{
			foreach(var scrollBar in this._scrollViews)
			{
				if (!scrollBar.Item2.IsDisposed())
					scrollBar.Item2.ScrollChange -= scroll_ScrollChange;
			}

			this._scrollViews.Clear();
		}

		public void InitializeNativeLayer(IMauiContext context, ViewGroup nativeLayer)
		{
			if (nativeLayer == null || nativeLayer.Context == null)
				return;

			if (nativeLayer.Context is Activity activity)
				_nativeActivity = activity;

			if (_nativeActivity == null || _nativeActivity.WindowManager == null || _nativeActivity.WindowManager.DefaultDisplay == null)
			{
				System.Diagnostics.Debug.WriteLine("VisualDiagnosticsLayer: Could not cast native Android activity.");
				return;
			}

			var measuredHeight = nativeLayer.MeasuredHeight;

			if (_nativeActivity.Window != null)
			{
				_nativeActivity.Window.DecorView.LayoutChange += DecorView_LayoutChange;
			}

			if (_nativeActivity != null && _nativeActivity.Resources != null && _nativeActivity.Resources.DisplayMetrics != null)
				this.DPI = _nativeActivity.Resources.DisplayMetrics.Density;

			this.VisualDiagnosticsGraphicsView = new NativeGraphicsView(nativeLayer.Context, this);
			if (this.VisualDiagnosticsGraphicsView == null)
			{
				System.Diagnostics.Debug.WriteLine("VisualDiagnosticsLayer: Could not set up touch layer canvas.");
				return;
			}
			this.VisualDiagnosticsGraphicsView.Touch += TouchLayer_Touch;
			nativeLayer.AddView(this.VisualDiagnosticsGraphicsView, 0, new CoordinatorLayout.LayoutParams(CoordinatorLayout.LayoutParams.MatchParent, CoordinatorLayout.LayoutParams.MatchParent));
			this.VisualDiagnosticsGraphicsView.BringToFront();
			this.IsNativeViewInitialized = true;
		}

		private Rectangle GenerateAdornerOffset(Activity _nativeActivity, NativeGraphicsView graphicsView)
		{
			if (_nativeActivity.Resources == null || _nativeActivity.Resources.DisplayMetrics == null)
				return new Rectangle();

			if (graphicsView == null)
				return new Rectangle();

			float dpi = _nativeActivity.Resources.DisplayMetrics.Density;
			float heightPixels = _nativeActivity.Resources.DisplayMetrics.HeightPixels;

			return new Rectangle(0, -(heightPixels - graphicsView.MeasuredHeight) / dpi, 0, 0);
		}

		private void TouchLayer_Touch(object? sender, View.TouchEventArgs e)
		{
			if (e == null || e.Event == null)
				return;

			e.Handled = this.DisableUITouchEventPassthrough;
			var point = new Point(e.Event.RawX, e.Event.RawY);
			OnTouchInternal(point, true);
		}

		private void DecorView_LayoutChange(object? sender, View.LayoutChangeEventArgs e)
		{
			if (this.AdornerBorders.Any())
				this.RemoveAdorners();

			if (this.VisualDiagnosticsGraphicsView != null && this._nativeActivity != null)
				this.Offset = GenerateAdornerOffset(this._nativeActivity, this.VisualDiagnosticsGraphicsView);

			this.Invalidate();
		}

		public void Invalidate()
		{
			this.VisualDiagnosticsGraphicsView?.Invalidate();
		}
	}
}
