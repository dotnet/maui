#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class UIContainerView : UIView, IPlatformMeasureInvalidationController
	{
		readonly View _view;
		bool _invalidateParentWhenMovedToWindow;
		bool _measureInvalidated;
		IPlatformViewHandler _renderer;
		UIView _platformView;
		bool _disposed;
		double _measuredHeight;

		internal event EventHandler PlatformMeasureInvalidated;

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

		internal double? Width
		{
			get;
			set;
		}

		public virtual Thickness Margin
		{
			get;
		}

		private protected void OnPlatformMeasureInvalidated()
		{
			PlatformMeasureInvalidated?.Invoke(this, EventArgs.Empty);
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

		public override void WillRemoveSubview(UIView uiview)
		{
			Disconnect();
			base.WillRemoveSubview(uiview);
		}

		public override void AddSubview(UIView view)
		{
			base.AddSubview(view);

		}

		public override void LayoutSubviews()
		{
			if (!IsPlatformViewValid())
				return;

			var height = Height ?? MeasuredHeight;
			var width = Width ?? Frame.Width;

			if (double.IsNaN(height))
				return;

			var platformFrame = new Rect(0, 0, width, height);


			if (MatchHeight)
			{
				(_view as IView).Measure(width, height);
			}

			(_view as IView).Arrange(platformFrame);
		}

		internal void Disconnect()
		{
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

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			_invalidateParentWhenMovedToWindow = true;
		}

		void IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			_measureInvalidated = true;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			if (_invalidateParentWhenMovedToWindow)
			{
				_invalidateParentWhenMovedToWindow = false;
				this.InvalidateAncestorsMeasures();
			}
		}

		internal void NotifyMeasureInvalidated()
		{
			if (_measureInvalidated)
			{
				_measureInvalidated = false;
				OnPlatformMeasureInvalidated();
			}
		}
	}
}
