using System;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class UIContainerView : UIView
	{
		readonly View _view;
		IVisualElementRenderer _renderer;
		bool _disposed;
		internal event EventHandler HeaderSizeChanged;

		public UIContainerView(View view)
		{
			_view = view;

			_renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, _renderer);

			AddSubview(_renderer.NativeView);
			ClipsToBounds = true;
			view.MeasureInvalidated += OnMeasureInvalidated;
			MeasuredHeight = double.NaN;
			_view.BatchCommitted += _view_BatchCommitted;
		}

		private void _view_BatchCommitted(object sender, Internals.EventArg<VisualElement> e)
		{
		}

		internal View View => _view;

		internal double MeasuredHeight { get; private set; }

		internal bool MeasureIfNeeded()
		{
			if (View == null)
				return false;

			if (double.IsNaN(MeasuredHeight) || Frame.Width != View.Width)
			{
				ReMeasure();
				return true;
			}
			return false;
		}

		public Thickness Margin
		{
			get
			{
				if(!_view.IsSet(View.MarginProperty))
				{
					var newMargin = new Thickness(0, (float)Platform.SafeAreaInsetsForWindow.Top, 0, 0);

					if (newMargin != _view.Margin)
					{
						_view.Margin = newMargin;
					}
				}

				return _view.Margin;
			}
		}

		void ReMeasure()
		{
			var request = _view.Measure(Frame.Width, double.PositiveInfinity, MeasureFlags.None);
			MeasuredHeight = request.Request.Height;
			HeaderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnMeasureInvalidated(object sender, System.EventArgs e)
		{
			ReMeasure();
			LayoutSubviews();
		}

		public override void WillMoveToSuperview(UIView newsuper)
		{
			base.WillMoveToSuperview(newsuper);
			ReMeasure();
		}

		public override void LayoutSubviews()
		{
			_view.Layout(new Rectangle(0, Margin.Top, Frame.Width, MeasuredHeight));
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if(_view != null)
					_view.MeasureInvalidated -= OnMeasureInvalidated;

				_renderer?.Dispose();
				_renderer = null;
				_view.ClearValue(Platform.RendererProperty);

				_disposed = true;
			}

			base.Dispose(disposing);
		}
	}
}