using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.Platform.MacOS
{
	sealed class VerticallyCenteredTextFieldCell : NSTextFieldCell
	{
		readonly nfloat _yOffset;

		public VerticallyCenteredTextFieldCell(nfloat yOffset, NSFont font = null)
		{
			if (font != null)
				Font = font;
			_yOffset = yOffset;
		}

		public override CGRect DrawingRectForBounds(CGRect theRect)
		{
			// Get the parent's idea of where we should draw.
			CGRect newRect = base.DrawingRectForBounds(theRect);

			// Ideal size for the text.
			CGSize textSize = CellSizeForBounds(theRect);

			// Center in the rect.
			nfloat heightDelta = newRect.Size.Height - textSize.Height;
			if (heightDelta > 0)
			{
				newRect.Size = new CGSize(newRect.Width, newRect.Height - heightDelta);
				newRect.Location = new CGPoint(newRect.X, newRect.Y + heightDelta / 2 + _yOffset);
			}
			return newRect;
		}
	}
}