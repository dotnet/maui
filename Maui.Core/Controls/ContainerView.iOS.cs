using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace System.Maui.Core.Controls
{
	public partial class ContainerView : UIView
	{
		public ContainerView()
		{
			AutosizesSubviews = true;
		}

		UIView _mainView;
		public UIView MainView
		{
			get => _mainView;
			set
			{
				if (_mainView == value)
					return;

				if (_mainView != null)
				{
					//Cleanup!
					_mainView.RemoveFromSuperview();
				}
				_mainView = value;
				if (value == null)
					return;
				this.Frame = _mainView.Frame;
				var oldParent = value.Superview;
				if (oldParent != null)
					oldParent.InsertSubviewAbove(this, _mainView);


				//_size = _mainView.Bounds.Size;
				_mainView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				_mainView.Frame = Bounds;

				//if (_overlayView != null)
				//	InsertSubviewBelow (_mainView, _overlayView);
				//else
				AddSubview(_mainView);
			}
		}

		public override void SizeToFit()
		{
			MainView.SizeToFit();
			this.Bounds = MainView.Bounds;
			base.SizeToFit();
		}

		CAShapeLayer Mask
		{
			get => MainView.Layer.Mask as CAShapeLayer;
			set => MainView.Layer.Mask = value;
		}

		partial void ClipShapeChanged()
		{
			lastMaskSize = Size.Zero;
			if (Frame == CGRect.Empty)
				return;
		}
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			SetClipShape();
		}

		Size lastMaskSize = Size.Zero;
		void SetClipShape()
		{
			var mask = Mask;
			if (mask == null && ClipShape == null)
				return;
			mask ??= Mask = new CAShapeLayer();
			var frame = Frame;
			var bounds = new Rectangle(0, 0, frame.Width, frame.Height);
			if (bounds.Size == lastMaskSize)
				return;
			lastMaskSize = bounds.Size;
			var path = _clipShape?.PathForBounds(bounds);
			mask.Path = path?.ToCGPath();
		}

	}
}
