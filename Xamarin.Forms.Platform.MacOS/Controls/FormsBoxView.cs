using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.Platform.macOS.Controls
{
	internal class FormsBoxView : NSView
	{
		NSColor _colorToRenderer;

		nfloat _topLeft;
		nfloat _topRight;
		nfloat _bottomLeft;
		nfloat _bottomRight;

		public override void DrawRect (CGRect dirtyRect)
		{
			_colorToRenderer.SetFill ();

			var innerRect = NSBezierPath.FromRoundedRect(Bounds, 0, 0);

			NSBezierPath bezierPath = new NSBezierPath();

			bezierPath.MoveTo (new CGPoint (innerRect.Bounds.X, innerRect.Bounds.Y + _bottomLeft));

			// Bottom left (origin):
			bezierPath.AppendPathWithArc (new CGPoint (innerRect.Bounds.X + _bottomLeft, innerRect.Bounds.Y + _bottomLeft), _bottomLeft, (float)180.0, (float)270.0);

			// Bottom right:
			bezierPath.AppendPathWithArc (new CGPoint (innerRect.Bounds.X + innerRect.Bounds.Width - _bottomRight, innerRect.Bounds.Y + _bottomRight), _bottomRight, (float)270.0, (float)360.0);

			// Top right:
			bezierPath.AppendPathWithArc (new CGPoint (innerRect.Bounds.X + innerRect.Bounds.Width - _topRight, innerRect.Bounds.Y + innerRect.Bounds.Height - _topRight), _topRight, (float)0.0, (float)90.0);

			// Top left:
			bezierPath.AppendPathWithArc (new CGPoint (innerRect.Bounds.X + _topLeft, innerRect.Bounds.Y + innerRect.Bounds.Height - _topLeft), _topLeft, (float)90.0, (float)180.0);

			// Implicitly creates left edge.
			bezierPath.Fill ();

			base.DrawRect (dirtyRect);
		}

		public void SetColor (NSColor color)
		{
			_colorToRenderer = color;
		}

		public void SetCornerRadius (float topLeft, float topRight, float bottomLeft, float bottomRight)
		{
			_topLeft = topLeft;
			_topRight = topRight;
			_bottomLeft = bottomLeft;
			_bottomRight = bottomRight;
		}
	}
}