using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml.Input;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : UserControl
	{
		IGraphicsView? graphicsView;
		W2DGraphicsView platformGraphicsView;
		public PlatformTouchGraphicsView()
		{
			Content = platformGraphicsView = new W2DGraphicsView();
		}
		public void UpdateDrawable(IGraphicsView graphicsView)
		{
			platformGraphicsView.UpdateDrawable(graphicsView);
			this.graphicsView = graphicsView;
		}
		public void Invalidate() => platformGraphicsView.Invalidate();


		private PointF[] GetViewPoints(PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(this).Position;
			return new[] { new PointF((float)point.X, (float)point.Y) };
		}

		protected override void OnPointerEntered(PointerRoutedEventArgs e)
		{
			e.Handled = true;
			graphicsView?.StartHoverInteraction(GetViewPoints(e));
		}
		protected override void OnPointerCanceled(PointerRoutedEventArgs e)
		{
			e.Handled = true;
			graphicsView?.CancelInteraction();
		}

		protected override void OnPointerExited(PointerRoutedEventArgs e)
		{
			e.Handled = true;
			graphicsView?.EndHoverInteraction();
			graphicsView?.CancelInteraction();
			//pressedContained = false;
		}
		protected override void OnPointerMoved(PointerRoutedEventArgs e)
		{
			e.Handled = true;
			graphicsView?.DragInteraction(GetViewPoints(e));
		}
		protected override void OnPointerPressed(PointerRoutedEventArgs e)
		{
			e.Handled = true;
			var points = GetViewPoints(e);
			graphicsView?.StartInteraction(points);


		}
		protected override void OnPointerReleased(PointerRoutedEventArgs e)
		{
			var points = GetViewPoints(e);
			//Only fires if it's inside on windows
			graphicsView?.EndInteraction(points,true);
		}

		public void Connect(IGraphicsView graphicsView) => this.graphicsView = graphicsView;
		public void Disconnect() => graphicsView = null;
	}
}
