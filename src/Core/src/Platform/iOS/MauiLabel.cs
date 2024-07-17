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
			var frameSize = Frame.Size;
			var height = Lines == 1 ? Font.LineHeight : SizeThatFits(frameSize).Height;

			if (height < frameSize.Height)
			{
				if (_verticalAlignment == UIControlContentVerticalAlignment.Top)
				{
					rect.Height = height;
				}
				else if (_verticalAlignment == UIControlContentVerticalAlignment.Bottom)
				{
					rect = new RectangleF(rect.X, rect.Bottom - height, rect.Width, height);
				}
			}

			return rect;
		}

		public override SizeF SizeThatFits(SizeF size)
		{
			var requestedSize = base.SizeThatFits(size);

			// Let's be sure the label is not larger than the container
			return AddInsets(new Size()
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