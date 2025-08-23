using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class HitTestingPage : IWindowOverlayElement
	{
		enum State
		{
			SingleSelection,
			RectangleSelectionPickFirst,
			RectangleSelectionPickSecond
		}

		Microsoft.Maui.Controls.Window? _window;
		WindowOverlay? _overlay;
		double _firstPosX;
		double _firstPosY;
		double _currentMousePosX;
		double _currentMousePosY;
		State _state = State.SingleSelection;
		bool _tappedWithoutMove;

		Microsoft.Maui.Controls.View[]? _allChildren = null;

		public HitTestingPage()
		{
			InitializeComponent();
		}

		private void RectangleSelectionCheckBox_CheckedChanged(object sender, Microsoft.Maui.Controls.CheckedChangedEventArgs e)
		{
			_state = RectangleSelectionCheckBox.IsChecked ? State.RectangleSelectionPickFirst : State.SingleSelection;
		}

		private void ContentPage_Loaded(object sender, EventArgs e)
		{
			_window = this.GetParentWindow();
			_overlay = new WindowOverlay(_window);
			_overlay.AddWindowElement(this);
			_window.AddOverlay(_overlay);

			_allChildren = this.GetVisualTreeDescendants()
				.OfType<Microsoft.Maui.Controls.View>()
				.Where(x => x != RectangleSelectionCheckBox)
				.ToArray();
#if WINDOWS
			var platformChildren = _allChildren.Select(v => v.Handler!.PlatformView).OfType<Microsoft.UI.Xaml.UIElement>();
			foreach (var element in platformChildren)
			{
				Debug.Print(element.GetType().FullName);
				element.Tapped += DoHandleTapped;
				element.PointerMoved += DoHandlePointerMoved;
			}

			void DoHandleTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs args)
			{
				var pos = args.GetPosition(null);
				Debug.WriteLine($"Tapped {sender.GetType().FullName} @ ({pos.X};{pos.Y})");
				HandleTapped(pos.X, pos.Y);
			}

			void DoHandlePointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs args)
			{
				var pos = args.GetCurrentPoint(null);
				HandlePointerMoved(pos.Position.X, pos.Position.Y);
			}
#else
			RectangleSelectionCheckBox.IsEnabled = false;
			_overlay.Tapped += DoHandleOverlayTapped;

			void DoHandleOverlayTapped(object? sender, WindowOverlayTappedEventArgs e)
			{
				var p = e.Point;
				Debug.Print($"{sender!.GetType().Name} tapped! ({p.X};{p.Y})");
				_tappedWithoutMove = false; // No mouse move on iOS/Android
				HandleTapped(p.X, p.Y);
			}
#endif
		}



		private void ContentPage_Unloaded(object sender, EventArgs e)
		{
			_overlay!.RemoveWindowElement(this);
			_window!.RemoveOverlay(_overlay);
		}

		private void HandlePointerMoved(double x, double y)
		{
			_currentMousePosX = x;
			_currentMousePosY = y;
			_tappedWithoutMove = false;

			if (_state == State.RectangleSelectionPickSecond)
			{
				_overlay!.Invalidate();
			}
		}

		private void HandleTapped(double x, double y)
		{
			if (_tappedWithoutMove) // "Tapped" may fire multiple times for containers & overlapping controls
				return;

			_tappedWithoutMove = true;
			IEnumerable<IVisualTreeElement>? elements = null;

			if (_state == State.SingleSelection)
			{
				elements = VisualTreeElementExtensions.GetVisualTreeElements(this.Window, x, y);
			}
			else if (_state == State.RectangleSelectionPickFirst)
			{
				_firstPosX = x;
				_firstPosY = y;
				_state = State.RectangleSelectionPickSecond;
				return;
			}
			else if (_state == State.RectangleSelectionPickSecond)
			{
				var rect = GetCurrentRect();
				elements = VisualTreeElementExtensions.GetVisualTreeElements(this.Window, rect.Left, rect.Top, rect.Right, rect.Bottom);
				_state = State.RectangleSelectionPickFirst;
				_overlay!.Invalidate();
			}

			foreach (var c in _allChildren!)
			{
				c.BackgroundColor = null;
			}

			SelectionLabel.Text = "Selected: " + string.Join(" <- ", elements!.Select(x => x.GetType().Name));
			var e = elements!.FirstOrDefault() as Microsoft.Maui.Controls.View;

			e?.BackgroundColor = new Microsoft.Maui.Graphics.Color(255, 0, 0);
		}

		// IWindowOverlayElement/IDrawable is implemented to show the rectangle selection lasso

		bool IWindowOverlayElement.Contains(Point point)
		{
			return this.Frame.Contains(point);
		}

		void IDrawable.Draw(ICanvas canvas, RectF dirtyRect)
		{
			if (_state == State.RectangleSelectionPickSecond)
			{
				Rect rect = GetCurrentRect();
				canvas.StrokeColor = Colors.Pink;
				canvas.StrokeSize = 2;
				canvas.DrawRectangle(rect);
			}
		}

		Rect GetCurrentRect()
		{
			double minX = Math.Min(_firstPosX, _currentMousePosX);
			double minY = Math.Min(_firstPosY, _currentMousePosY);
			double maxX = Math.Max(_firstPosX, _currentMousePosX);
			double maxY = Math.Max(_firstPosY, _currentMousePosY);
			return Rect.FromLTRB(minX, minY, maxX, maxY);
		}
	}
}
