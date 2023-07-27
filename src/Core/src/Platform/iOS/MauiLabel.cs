#nullable disable

using System;
using CoreAnimation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Platform
{
	public class MauiLabel : UILabel
	{
		UIControlContentVerticalAlignment _verticalAlignment = UIControlContentVerticalAlignment.Center;
		bool _isLimitSize = true;

		public UIEdgeInsets TextInsets { get; set; }
		internal UIControlContentVerticalAlignment VerticalAlignment
		{
			get => _verticalAlignment;
			set
			{
				_verticalAlignment = value;
				SetNeedsDisplay();
			}
		}

		public MauiLabel(RectangleF frame) : base(frame)
		{
		}

		public MauiLabel()
		{
		}

		public override void DrawText(RectangleF rect)
		{
			rect = TextInsets.InsetRect(rect);
			var clipRect = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);

			if (_verticalAlignment != UIControlContentVerticalAlignment.Center
				&& _verticalAlignment != UIControlContentVerticalAlignment.Fill)
			{
				rect = AlignVertical(rect);
			}

			base.DrawText(rect);

			var maskLayer = new CAShapeLayer() { Frame = Bounds, Path = UIBezierPath.FromRect(clipRect).CGPath };
			maskLayer.MasksToBounds = true;
			Layer.Mask = maskLayer;
		}

		public override RectangleF TextRectForBounds(RectangleF bounds, nint numberOfLines)
		{
			var insetRect = TextInsets.InsetRect(bounds);
			var textRect = base.TextRectForBounds(insetRect, numberOfLines);
			var invertedInsets = new UIEdgeInsets(-TextInsets.Top, -TextInsets.Left, -TextInsets.Bottom, -TextInsets.Right);
			return invertedInsets.InsetRect(textRect);
		}

		RectangleF AlignVertical(RectangleF rect)
		{
			var frameSize = Frame.Size;

			_isLimitSize = false;
			var height = Lines == 1 ? Font.LineHeight : SizeThatFits(frameSize).Height;
			_isLimitSize = true;

			if (Lines != 1)
			{
				height -= (TextInsets.Top + TextInsets.Bottom);
			}

			if (_verticalAlignment == UIControlContentVerticalAlignment.Top)
			{
				rect.Height = height;
			}
			else if (_verticalAlignment == UIControlContentVerticalAlignment.Bottom)
			{
				rect = new RectangleF(rect.X, rect.Bottom - height, rect.Width, height);
			}

			return rect;
		}

		public override void InvalidateIntrinsicContentSize()
		{
			base.InvalidateIntrinsicContentSize();

			if (Frame.Width == 0 && Frame.Height == 0)
			{
				// The Label hasn't actually been laid out on screen yet; no reason to request a layout
				return;
			}

			if (!Frame.Size.IsCloseTo(AddInsets(IntrinsicContentSize), (nfloat)0.001))
			{
				// The text or its attributes have changed enough that the size no longer matches the set Frame. It's possible
				// that the Label needs to be laid out again at a different size, so we request that the parent do so. 
				Superview?.SetNeedsLayout();
			}
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			var requestedSize = base.SizeThatFits(size);
			
			if (_isLimitSize)
			{
				// Let's be sure the label is not larger than the container
				return new Size()
				{
					Width = nfloat.Min(requestedSize.Width, size.Width),
					Height = nfloat.Min(requestedSize.Height, size.Height),
				};
			}
			else
			{
				return requestedSize;
			}
		}

		SizeF AddInsets(SizeF size) => new SizeF(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);
	}
}