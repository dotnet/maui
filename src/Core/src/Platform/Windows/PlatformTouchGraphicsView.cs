using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Microsoft.Maui.Platform
{
	public class PlatformTouchGraphicsView : UserControl
	{
		IGraphicsView? _graphicsView;
		readonly W2DGraphicsView _platformGraphicsView;
		bool _isTouching = false;
		bool _isInBounds = false;

		public PlatformTouchGraphicsView()
		{
			ManipulationMode = ManipulationModes.All;

			Content = _platformGraphicsView = new W2DGraphicsView();
		}

		public void UpdateDrawable(IGraphicsView graphicsView)
		{
			_platformGraphicsView.UpdateDrawable(graphicsView);
			_graphicsView = graphicsView;
		}

		public void Invalidate() => _platformGraphicsView.Invalidate();

		PointF[] GetViewPoints(PointerRoutedEventArgs e)
		{
			var point = e.GetCurrentPoint(this).Position;
			return new[] { new PointF((float)point.X, (float)point.Y) };
		}

		protected override void OnPointerEntered(PointerRoutedEventArgs e)
		{
			_isInBounds = true;
			_graphicsView?.StartHoverInteraction(GetViewPoints(e));
		}

		protected override void OnPointerCanceled(PointerRoutedEventArgs e)
		{
			if (_isTouching)
			{
				_isTouching = false;
				_graphicsView?.EndInteraction(GetViewPoints(e), _isInBounds);
				_graphicsView?.CancelInteraction();
			}
		}

		protected override void OnPointerExited(PointerRoutedEventArgs e)
		{
			_isInBounds = false;

			_graphicsView?.EndHoverInteraction();

			if (_isTouching)
			{
				_isTouching = false;
				_graphicsView?.EndInteraction(GetViewPoints(e), _isInBounds);
			}
		}

		protected override void OnPointerMoved(PointerRoutedEventArgs e)
		{
			var points = GetViewPoints(e);

			_graphicsView?.MoveHoverInteraction(points);

			if (_isTouching)
				_graphicsView?.DragInteraction(points);
		}

		protected override void OnPointerPressed(PointerRoutedEventArgs e)
		{
			var points = GetViewPoints(e);
			_isTouching = true;
			_graphicsView?.StartInteraction(points);
		}

		protected override void OnPointerReleased(PointerRoutedEventArgs e)
		{
			var points = GetViewPoints(e);

			if (_isTouching)
			{
				_isTouching = false;
				//Only fires if it's inside on windows
				_graphicsView?.EndInteraction(points, _isInBounds);
			}
		}

		public void Connect(IGraphicsView graphicsView) => _graphicsView = graphicsView;

		public void Disconnect() => _graphicsView = null;
	}
}