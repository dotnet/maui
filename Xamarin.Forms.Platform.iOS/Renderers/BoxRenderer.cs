using System;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		UIColor _colorToRenderer;
		SizeF _previousSize;
		nfloat _topLeft;
		nfloat _topRight;
		nfloat _bottomLeft;
		nfloat _bottomRight;

		public override void Draw(RectangleF rect)
		{
			UIBezierPath bezierPath = new UIBezierPath();

			bezierPath.AddArc(new CoreGraphics.CGPoint((float)Bounds.X + Bounds.Width - _topRight, (float)Bounds.Y + _topRight), _topRight, (float)(Math.PI * 1.5), (float)Math.PI * 2, true);
			bezierPath.AddArc(new CoreGraphics.CGPoint((float)Bounds.X + Bounds.Width - _bottomRight, (float)Bounds.Y + Bounds.Height - _bottomRight), _bottomRight, 0, (float)(Math.PI * .5), true);
			bezierPath.AddArc(new CoreGraphics.CGPoint((float)Bounds.X + _bottomLeft, (float)Bounds.Y + Bounds.Height - _bottomLeft), _bottomLeft, (float)(Math.PI * .5), (float)Math.PI, true);
			bezierPath.AddArc(new CoreGraphics.CGPoint((float)Bounds.X + _topLeft, (float)Bounds.Y + _topLeft), (float)_topLeft, (float)Math.PI, (float)(Math.PI * 1.5), true);

			_colorToRenderer.SetFill();
			bezierPath.Fill();

			base.Draw(rect);

			_previousSize = Bounds.Size;
		}

		public override void LayoutSubviews()
		{
			if (_previousSize != Bounds.Size)
				SetNeedsDisplay();

			base.LayoutSubviews();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			if (Element != null)
			{
				SetBackgroundColor(Element.BackgroundColor);
				SetCornerRadius();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetBackgroundColor(Element.BackgroundColor);
			else if (e.PropertyName == BoxView.CornerRadiusProperty.PropertyName)
				SetCornerRadius();
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName && Element.IsVisible)
				SetNeedsDisplay();
		}

		protected override void SetBackgroundColor(Color color)
		{
			if (Element == null)
				return;

			var elementColor = Element.Color;
			if (!elementColor.IsDefault)
				_colorToRenderer = elementColor.ToUIColor();
			else
				_colorToRenderer = color.ToUIColor();

			SetNeedsDisplay();
		}

		void SetCornerRadius()
		{
			if (Element == null)
				return;

			var elementCornerRadius = Element.CornerRadius;

			_topLeft = (float)elementCornerRadius.TopLeft;
			_topRight = (float)elementCornerRadius.TopRight;
			_bottomLeft = (float)elementCornerRadius.BottomLeft;
			_bottomRight = (float)elementCornerRadius.BottomRight;

			SetNeedsDisplay();
		}
	}
}