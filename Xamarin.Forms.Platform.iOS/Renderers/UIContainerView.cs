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
		}

		internal double MeasuredHeight { get; private set; }

		internal bool MeasureIfNeeded()
		{
			if (double.IsNaN(MeasuredHeight))
			{
				ReMeasure();
				return true;
			}
			return false;
		}

		void ReMeasure()
		{
			var request = _view.Measure(Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			Layout.LayoutChildIntoBoundingRegion(_view, new Rectangle(0, 0, Frame.Width, request.Request.Height));
			MeasuredHeight = request.Request.Height;
			HeaderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnMeasureInvalidated(object sender, System.EventArgs e)
		{
			ReMeasure();
		}

		public override void LayoutSubviews()
		{
			if(!MeasureIfNeeded())
				_view.Layout(Bounds.ToRectangle());
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_renderer?.Dispose();
				_renderer = null;
				_view.ClearValue(Platform.RendererProperty);

				_disposed = true;
			}

			base.Dispose(disposing);
		}
	}
}