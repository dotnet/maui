using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class UIContainerView : UIView
	{
		readonly View _view;
		IPlatformViewHandler _renderer;
		bool _disposed;
		internal event EventHandler HeaderSizeChanged;

		public UIContainerView(View view)
		{
			_view = view;

			_renderer = (IPlatformViewHandler)view.ToHandler(view.FindMauiContext());

			AddSubview(_renderer.PlatformView);
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
				var request = (_view as IView).Measure(Frame.Width, double.PositiveInfinity);
				MeasuredHeight = request.Height;
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
			_view.LayoutToSize(Width ?? Frame.Width, Height ?? MeasuredHeight);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_view != null)
					_view.MeasureInvalidated -= OnMeasureInvalidated;

				_renderer?.DisconnectHandler();
				_renderer = null;

				_disposed = true;
			}

			base.Dispose(disposing);
		}
	}
}
