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
		private double _measuredHeight;

		internal event EventHandler HeaderSizeChanged;

		public UIContainerView(View view)
		{
			_view = view;

			UpdatePlatformView();

			AddSubview(view.ToPlatform());
			ClipsToBounds = true;
			MeasuredHeight = double.NaN;
			Margin = new Thickness(0);
		}

		internal void UpdatePlatformView()
		{
			_renderer = _view.ToHandler(_view.FindMauiContext());
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

		internal double MeasuredHeight
		{
			get
			{
				if (MatchHeight && Height != null)
					return Height.Value;

				return _measuredHeight;
			}

			private set => _measuredHeight = value;
		}

		internal double? Height
		{
			get;
			set;
		}

		public virtual Thickness Margin
		{
			get;
		}

		private protected void OnHeaderSizeChanged()
		{
			HeaderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

		void OnMeasureInvalidated(object sender, System.EventArgs e)
		{
			if (!IsPlatformViewValid())
				return;
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

		public override CGSize SizeThatFits(CGSize size)
		{
			var measuredSize = (_view as IView).Measure(size.Width, size.Height);

			if (Height != null && MatchHeight)
			{
				MeasuredHeight = Height.Value;
			}
			else
			{
				MeasuredHeight = measuredSize.Height;
			}

			return new CGSize(size.Width, MeasuredHeight);
		}

		public override void LayoutSubviews()
		{
			if (!IsPlatformViewValid())
				return;

			var height = Height ?? MeasuredHeight;
			if (double.IsNaN(height))
				return;

			var platformFrame = new Rect(0, 0, Frame.Width, height);
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
				_disposed = true;
			}

			base.Dispose(disposing);
		}
	}
}
