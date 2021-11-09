using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
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

		public void AddScrollableElementHandlers()
		{
		}

		public void RemoveScrollableElementHandler()
		{
		}

		public W2DGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }

		public void InitializeNativeLayer(IMauiContext context, Microsoft.Maui.RootPanel nativeLayer)
		{
			this.VisualDiagnosticsGraphicsView = new W2DGraphicsView() { Drawable = this };
			this.VisualDiagnosticsGraphicsView.Tapped += VisualDiagnosticsGraphicsView_Tapped;
			this.VisualDiagnosticsGraphicsView.SetValue(Canvas.ZIndexProperty, 99);
			this.VisualDiagnosticsGraphicsView.IsHitTestVisible = false;
			nativeLayer.Children.Add(this.VisualDiagnosticsGraphicsView);
			this.IsNativeViewInitialized = true;
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
