using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui
{
	public partial class WrapperView : UIView
	{
		SizeF _lastMaskSize;

		public WrapperView()
		{
		}

		public WrapperView(CGRect frame)
			: base(frame)
		{
		}

		CAShapeLayer? Mask
		{
			get => Layer.Mask as CAShapeLayer;
			set => Layer.Mask = value;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (Subviews.Length == 0)
				return;

			var child = Subviews[0];

			child.Frame = Bounds;

			if (Mask != null)
				Mask.Frame = Bounds;

			SetClip();
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (Subviews.Length == 0)
				return base.SizeThatFits(size);

			var child = Subviews[0];

			return child.SizeThatFits(size);
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();

			Superview?.SetNeedsLayout();
		}

		partial void ClipChanged()
		{
			_lastMaskSize = SizeF.Zero;

			if (Frame == CGRect.Empty)
				return;
		}

		void SetClip()
		{
			var mask = Mask;

			if (mask == null && Clip == null)
				return;

			mask ??= Mask = new CAShapeLayer();
			var frame = Frame;
			var bounds = new RectangleF(0, 0, (float)frame.Width, (float)frame.Height);

			if (bounds.Size == _lastMaskSize)
				return;

			_lastMaskSize = bounds.Size;
			var path = _clip?.PathForBounds(bounds);
			mask.Path = path?.AsCGPath();
		}
	}
}