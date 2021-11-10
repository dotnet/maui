using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.CoreGraphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		private bool disableUITouchEventPassthrough;
		public bool DisableUITouchEventPassthrough
		{
			get { return disableUITouchEventPassthrough; }
			set
			{
				disableUITouchEventPassthrough = value;
				if (this._uiView != null)
					this._uiView.DisableUITouchEventPassthrough = value;
			}
		}

		public NativeGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }
		private UIWindow? _window;
		private PassthroughView? _uiView;
		private HashSet<Tuple<UIScrollView, IDisposable>> _scrollViews = new HashSet<Tuple<UIScrollView, IDisposable>>();

		public void AddScrollableElementHandlers()
		{
			if (this._window != null && this._window.RootViewController != null && this._window.RootViewController.View != null)
			{
				var scrolls = this.GetUIScrollViews(this._window.RootViewController.View);
				foreach (var scroll in scrolls)
				{
					if (!this._scrollViews.Any(n => n.Item1 == scroll))
					{
						var testing = scroll.AddObserver("contentOffset", Foundation.NSKeyValueObservingOptions.New, HandleAction);
						this._scrollViews.Add(new Tuple<UIScrollView, IDisposable>(scroll, testing));
					}
				}
			}
		}

		private void Scroll_Scrolled(object? sender, EventArgs e)
		{
			this.Invalidate();
		}

		public void RemoveScrollableElementHandler()
		{
			foreach(var scroll in this._scrollViews)
			{
				scroll.Item2.Dispose();
			}

			this._scrollViews.Clear();
		}

		public void InitializeNativeLayer(IMauiContext context, UIKit.UIWindow nativeLayer)
		{
			
			if (nativeLayer.RootViewController == null || nativeLayer.RootViewController.View == null)
				return;

			this._window = nativeLayer;


			_uiView = new PassthroughView(nativeLayer.RootViewController.View.Frame);
			this.VisualDiagnosticsGraphicsView = new NativeGraphicsView(_uiView.Frame, this, new DirectRenderer());
			this.VisualDiagnosticsGraphicsView.UserInteractionEnabled = false;
			_uiView.AddSubview(this.VisualDiagnosticsGraphicsView);
			if (this.VisualDiagnosticsGraphicsView == null)
			{
				System.Diagnostics.Debug.WriteLine("VisualDiagnosticsLayer: Could not set up touch layer canvas.");
				return;
			}

			
			var observer = nativeLayer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.OldNew, HandleAction);
			this.VisualDiagnosticsGraphicsView.UserInteractionEnabled = false;
			this.VisualDiagnosticsGraphicsView.BackgroundColor = UIColor.FromWhiteAlpha(1, 0.0f);
			nativeLayer.RootViewController.View.AddSubview(_uiView);
			nativeLayer.RootViewController.View.BringSubviewToFront(_uiView);
			this._uiView.OnTouch += _uiView_OnTouch;
			this.IsNativeViewInitialized = true;
		}

		private void _uiView_OnTouch(object? sender, CGPoint e)
		{
			var point = new Point(e.X, e.Y);
			OnTouchInternal(point, true);
		}

		public List<UIScrollView> GetUIScrollViews(UIView view, List<UIScrollView>? views = null)
		{
			System.Diagnostics.Debug.WriteLine(view.GetType().Name);
			if (views == null)
				views = new List<UIScrollView>();

			if (view is UIScrollView scrollView)
				views.Add(scrollView);

			foreach (var children in view.Subviews)
				GetUIScrollViews(children, views);

			return views;
		}

		private void HandleAction(Foundation.NSObservedChange obj)
		{
			this.Invalidate();
		}

		public void Invalidate()
		{
			this.VisualDiagnosticsGraphicsView?.InvalidateIntrinsicContentSize();
			this.VisualDiagnosticsGraphicsView?.InvalidateDrawable();
		}
	}

	internal class PassthroughView : UIView
	{
		public bool DisableUITouchEventPassthrough { get; set; }

		public event EventHandler<CGPoint>? OnTouch;

		public PassthroughView()
		{
		}

		public PassthroughView(NSCoder coder) : base(coder)
		{
		}

		public PassthroughView(CGRect frame) : base(frame)
		{
		}

		protected PassthroughView(NSObjectFlag t) : base(t)
		{
		}

		protected internal PassthroughView(IntPtr handle) : base(handle)
		{
		}

		public override bool PointInside(CGPoint point, UIEvent? uievent)
		{
			this.OnTouch?.Invoke(this, point);
			return !DisableUITouchEventPassthrough;
		}
	}
}
