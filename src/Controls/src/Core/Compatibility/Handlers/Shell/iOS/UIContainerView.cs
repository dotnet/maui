#nullable disable
using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class UIContainerView : MauiView
	{
		WeakReference<IView> _childview;
		bool _disposed;
		double _measuredHeight;

		internal event EventHandler HeaderSizeChanged;

		public UIContainerView(View view)
		{
			_childview = new(view);
			UpdatePlatformView();
			MeasuredHeight = double.NaN;
			Margin = new Thickness(0);
		}

		internal new View View => _childview != null && _childview.TryGetTarget(out var v) ? v as View : null;

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

		private protected void OnHeaderSizeChanged()
		{
			HeaderSizeChanged?.Invoke(this, EventArgs.Empty);
		}

		internal void UpdatePlatformView()
		{
			GeneralWrapperView.UpdatePlatformView(this, View, View.FindMauiContext(), out _childview);
		}

		internal CGSize UpdateSize(CGSize size)
		{
			var newSize = SizeThatFits(size);
			if (MeasuredHeight != newSize.Height)
			{
				MeasuredHeight = newSize.Height;
				SetNeedsLayout();
			}
			return newSize;
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			var measuredSize = View.Measure(size.Width, size.Height);
			var height = Height != null && MatchHeight ? Height.Value : measuredSize.Height;
			return new CGSize(size.Width, height);
		}

		internal void Disconnect()
		{
			GeneralWrapperView.Disconnect(this, _childview);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			base.WillRemoveSubview(uiview);
		}

		public override void AddSubview(UIView view)
		{
			base.AddSubview(view);
		}

		public override void LayoutSubviews()
		{
			var height = Height ?? MeasuredHeight;
			var width = Width ?? Frame.Width;

			if (double.IsNaN(height))
				return;

			var platformFrame = new Rect(0, 0, width, height);

			if (MatchHeight)
			{
				View.Measure(width, height);
			}

			View.Arrange(platformFrame);
		}


		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				Disconnect();
				_disposed = true;
			}

			base.Dispose(disposing);
		}
	}
}
