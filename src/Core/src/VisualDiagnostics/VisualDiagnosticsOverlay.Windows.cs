using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	/// <summary>
	/// Visual Diagnostics Overlay.
	/// </summary>
	public partial class VisualDiagnosticsOverlay : IVisualDiagnosticsOverlay, IDrawable
	{
		private W2DGraphicsView? _visualDiagnosticsGraphicsView;
		private bool disableUITouchEventPassthrough;
		HashSet<Tuple<IScrollView, ScrollViewer>> _scrollViews = new HashSet<Tuple<IScrollView, ScrollViewer>>();

		/// <inheritdoc/>
		public bool DisableUITouchEventPassthrough
		{
			get { return disableUITouchEventPassthrough; }
			set 
			{ 
				disableUITouchEventPassthrough = value;
				if (this._visualDiagnosticsGraphicsView != null)
					this._visualDiagnosticsGraphicsView.IsHitTestVisible = value;
			}
		}

		/// <inheritdoc/>
		public IReadOnlyCollection<Tuple<IScrollView, ScrollViewer>> ScrollViews => this._scrollViews.ToList().AsReadOnly();

		/// <inheritdoc/>
		public void InitializeNativeLayer(IMauiContext context, Microsoft.Maui.RootPanel nativeLayer)
		{
			var nativeWindow = this.Window.Content.GetNative(true);

			// Capture when the frame is navigating.
			// When it is, we will clear existing adorners.
			if (nativeWindow is Frame frame)
			{
				frame.Navigating += Frame_Navigating;
			}


			this._visualDiagnosticsGraphicsView = new W2DGraphicsView() { Drawable = this };
			this._visualDiagnosticsGraphicsView.Tapped += VisualDiagnosticsGraphicsView_Tapped;
			this._visualDiagnosticsGraphicsView.SetValue(Canvas.ZIndexProperty, 99);
			this._visualDiagnosticsGraphicsView.IsHitTestVisible = false;
			nativeLayer.Children.Add(this._visualDiagnosticsGraphicsView);
			this.IsNativeViewInitialized = true;
		}

		/// <inheritdoc/>
		public void AddScrollableElementHandler(IScrollView scrollBar)
		{
			var nativeScroll = scrollBar.GetNative(true);
			if (nativeScroll != null && nativeScroll is ScrollViewer viewer)
			{
				if (this._scrollViews.Add(new Tuple<IScrollView, ScrollViewer>(scrollBar, viewer)))
				{
					viewer.ViewChanging += Viewer_ViewChanging;
				}
			}
		}

		/// <inheritdoc/>
		public void RemoveScrollableElementHandler()
		{
			foreach (var scrollBar in this.ScrollViews)
			{
				scrollBar.Item2.ViewChanging -= Viewer_ViewChanging;
			}

			this._scrollViews.Clear();
		}

		/// <inheritdoc/>
		public void Invalidate()
		{
			this._visualDiagnosticsGraphicsView?.Invalidate();
		}

		private void Viewer_ViewChanging(object? sender, ScrollViewerViewChangingEventArgs e)
		{
			this.Invalidate();
		}

		private void Frame_Navigating(object sender, UI.Xaml.Navigation.NavigatingCancelEventArgs e)
		{
			if (this._adornerBorders.Any())
				this.RemoveAdorners();

			this.Invalidate();
		}

		private void VisualDiagnosticsGraphicsView_Tapped(object sender, UI.Xaml.Input.TappedRoutedEventArgs e)
		{
			if (e == null)
				return;

			e.Handled = this.DisableUITouchEventPassthrough;
			var position = e.GetPosition(this._visualDiagnosticsGraphicsView);

			var point = new Point(position.X, position.Y);
			this.OnTouchInternal(point, true);
		}
	}
}
