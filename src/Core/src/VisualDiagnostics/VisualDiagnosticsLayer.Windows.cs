using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

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
				if (this.VisualDiagnosticsGraphicsView != null)
					this.VisualDiagnosticsGraphicsView.IsHitTestVisible = value;
			}
		}

		private HashSet<Tuple<IScrollView, ScrollViewer>> _scrollViews = new HashSet<Tuple<IScrollView, ScrollViewer>>();

		public void AddScrollableElementHandlers()
		{
			var scrollBars = this.GetScrollViews();
			foreach (var scrollBar in scrollBars)
			{
				if (!_scrollViews.Any(x => x.Item1 == scrollBar))
				{
					var nativeScroll = ((IScrollView)scrollBar).GetNative(true);
					if (nativeScroll != null && nativeScroll is ScrollViewer viewer)
					{
						viewer.ViewChanging += Viewer_ViewChanging;
						this._scrollViews.Add(new Tuple<IScrollView, ScrollViewer>(scrollBar, viewer));
					}
				}
			}
		}

		private void Viewer_ViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
		{
			this.Invalidate();
		}

		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in this._scrollViews)
			{
				scrollBar.Item2.ViewChanging -= Viewer_ViewChanging;
			}

			this._scrollViews.Clear();
		}

		public W2DGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }

		public void InitializeNativeLayer(IMauiContext context, Microsoft.Maui.RootPanel nativeLayer)
		{
			var nativeWindow = this.Window.Content.GetNative(true);
			if (nativeWindow != null && nativeWindow is Frame frame)
			{
				frame.Navigating += Frame_Navigating;
			}

			this.VisualDiagnosticsGraphicsView = new W2DGraphicsView() { Drawable = this };
			this.VisualDiagnosticsGraphicsView.Tapped += VisualDiagnosticsGraphicsView_Tapped;
			this.VisualDiagnosticsGraphicsView.SetValue(Canvas.ZIndexProperty, 99);
			this.VisualDiagnosticsGraphicsView.IsHitTestVisible = false;
			nativeLayer.Children.Add(this.VisualDiagnosticsGraphicsView);
			this.IsNativeViewInitialized = true;
		}

		private void Frame_Navigating(object sender, UI.Xaml.Navigation.NavigatingCancelEventArgs e)
		{
			if (this.AdornerBorders.Any())
				this.RemoveAdorners();

			this.Invalidate();
		}

		private void VisualDiagnosticsGraphicsView_Tapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (e == null)
				return;

			var position = e.GetPosition(this.VisualDiagnosticsGraphicsView);

			var point = new Point(position.X, position.Y);

			var elements = new List<IVisualTreeElement>();
			if (!this.DisableUITouchEventPassthrough)
			{
				return;
			}
			else
			{
				e.Handled = true;
				var visualWindow = this.Window as IVisualTreeElement;
				if (visualWindow != null)
					elements.AddRange(visualWindow.GetVisualTreeElements(point));
			}

			this.OnTouch?.Invoke(this, new VisualDiagnosticsHitEvent(point, elements));
		}

		public void Invalidate()
		{
			this.VisualDiagnosticsGraphicsView?.Invalidate();
		}
	}
}
