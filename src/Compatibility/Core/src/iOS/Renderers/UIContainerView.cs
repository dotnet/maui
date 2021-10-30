using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
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
			Margin = new Thickness(0);
		}

		internal View View => _view;

		internal bool MatchHeight { get; set; }

		internal double MeasuredHeight { get; private set; }

		internal double? Height
		{
			get;
			set;
		}

		internal double? Width
		{
			get;
			set;
		}

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

		public virtual Thickness Margin
		{
			get;
		}

		void ReMeasure()
		{
			if (Height != null && MatchHeight)
			{
				MeasuredHeight = Height.Value;
			}
			else
			{
				var request = _view.Measure(Frame.Width, double.PositiveInfinity, MeasureFlags.None);
				MeasuredHeight = request.Request.Height;
			}

			HeaderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnMeasureInvalidated(object sender, System.EventArgs e)
		{
			ReMeasure();
			LayoutSubviews();
		}

		public override void WillMoveToSuperview(UIView newSuper)
		{
			base.WillMoveToSuperview(newSuper);
			ReMeasure();
		}

		public override void LayoutSubviews()
		{
			_view.Layout(new Rectangle(0, Margin.Top, Width ?? Frame.Width, Height ?? MeasuredHeight));
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_view != null)
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
