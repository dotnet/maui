#nullable disable
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
		UIView _platformView;
		bool _disposed;
		internal event EventHandler HeaderSizeChanged;

		public UIContainerView(View view)
		{
			_view = view;

			UpdatePlatformView();
			ClipsToBounds = true;
			MeasuredHeight = double.NaN;
			Margin = new Thickness(0);
		}

		internal void UpdatePlatformView()
		{
			_renderer = (IPlatformViewHandler)_view.ToHandler(_view.FindMauiContext());
			_platformView = _view.ToPlatform();

			if (_platformView.Superview != this)
				AddSubview(_platformView);
		}

		bool IsPlatformViewValid()
		{
			if (View == null || _platformView == null || _renderer == null)
				return false;

			return _platformView.Superview == this;
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
			if (!IsPlatformViewValid())
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
			if (!IsPlatformViewValid())
				return;

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
			if (!IsPlatformViewValid())
				return;

			ReMeasure();
			LayoutSubviews();
		}

		public override void WillMoveToSuperview(UIView newSuper)
		{
			base.WillMoveToSuperview(newSuper);
			ReMeasure();
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			Disconnect();
			base.WillRemoveSubview(uiview);
		}

		public override void AddSubview(UIView view)
		{
			if (view == _platformView)
				_view.MeasureInvalidated += OnMeasureInvalidated;

			base.AddSubview(view);

		}

		public override void LayoutSubviews()
		{
			if (!IsPlatformViewValid())
				return;

			var platformFrame = new Rect(0, 0, Width ?? Frame.Width, Height ?? MeasuredHeight);

			var width = Width ?? Frame.Width;
			var height = Height ?? MeasuredHeight;

			if (MatchHeight)
			{
				(_view as IView).Measure(width, height);
			}

			(_view as IView).Arrange(platformFrame);
		}

		internal void Disconnect()
		{
			if (_view != null)
				_view.MeasureInvalidated -= OnMeasureInvalidated;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				Disconnect();

				if (_platformView.Superview == this)
					_platformView.RemoveFromSuperview();

				_renderer = null;
				_platformView = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}
	}
}
