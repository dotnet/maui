#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Platform
{
	public class MauiLabel : UILabel, IUIViewLifeCycleEvents
	{
		UIControlContentVerticalAlignment _verticalAlignment = UIControlContentVerticalAlignment.Center;

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

			if (_verticalAlignment != UIControlContentVerticalAlignment.Center
				&& _verticalAlignment != UIControlContentVerticalAlignment.Fill)
			{
				rect = AlignVertical(rect);
			}

			base.DrawText(rect);
		}

		RectangleF AlignVertical(RectangleF rect)
		{
			var availableSize = rect.Size;
			var requiredHeight = Lines == 1 ? Font.LineHeight : base.SizeThatFits(rect.Size).Height;

			if (requiredHeight < availableSize.Height)
			{
				if (_verticalAlignment == UIControlContentVerticalAlignment.Top)
				{
					rect.Height = requiredHeight;
				}
				else if (_verticalAlignment == UIControlContentVerticalAlignment.Bottom)
				{
					rect = new RectangleF(rect.X, rect.Bottom - requiredHeight, rect.Width, requiredHeight);
				}
			}

			return rect;
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			// Prior to calculating the text size, reduce the padding, and then add the padding back in the AddInsets method.
			var adjustedWidth = size.Width - TextInsets.Left - TextInsets.Right;
			var adjustedHeight = size.Height - TextInsets.Top - TextInsets.Bottom;
			var requestedSize = base.SizeThatFits(new SizeF(adjustedWidth, adjustedHeight));

			// Let's be sure the label is not larger than the container
			return AddInsets(new Size
			{
				Width = nfloat.Min(requestedSize.Width, size.Width),
				Height = nfloat.Min(requestedSize.Height, size.Height),
			});
		}

		SizeF AddInsets(SizeF size) => new SizeF(
			width: size.Width + TextInsets.Left + TextInsets.Right,
			height: size.Height + TextInsets.Top + TextInsets.Bottom);

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = IUIViewLifeCycleEvents.UnconditionalSuppressMessage)]
		EventHandler _movedToWindow;
		event EventHandler IUIViewLifeCycleEvents.MovedToWindow
		{
			add => _movedToWindow += value;
			remove => _movedToWindow -= value;
		}

		public override void MovedToWindow()
		{
			base.MovedToWindow();
			_movedToWindow?.Invoke(this, EventArgs.Empty);
		}
	}
}