using System;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		UIColor _colorToRenderer;
		SizeF _previousSize;
		nfloat _topLeft;
		nfloat _topRight;
		nfloat _bottomLeft;
		nfloat _bottomRight;

		const float PI = (float)Math.PI;
		const float PIAndAHalf = PI * 1.5f;
		const float HalfPI = PI * .5f;
		const float TwoPI = PI * 2;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public BoxRenderer()
		{

		}

		public override void Draw(RectangleF rect)
		{
			UIBezierPath bezierPath = new UIBezierPath();

			bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + Bounds.Width - _topRight, Bounds.Y + _topRight), _topRight, PIAndAHalf, TwoPI, true);
			bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + Bounds.Width - _bottomRight, Bounds.Y + Bounds.Height - _bottomRight), _bottomRight, 0, HalfPI, true);
			bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + _bottomLeft, Bounds.Y + Bounds.Height - _bottomLeft), _bottomLeft, HalfPI, PI, true);
			bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + _topLeft, Bounds.Y + _topLeft), _topLeft, PI, PIAndAHalf, true);

			_colorToRenderer.SetFill();
			bezierPath.Fill();

			base.Draw(rect);

			_previousSize = Bounds.Size;
		}

		public override void LayoutSubviews()
		{
			if (Element != null && _previousSize != Bounds.Size)
			{
				SetBackground(Element.Background);
				SetNeedsDisplay();
			}

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

		protected override void SetBackground(Brush brush)
		{
			if (Element == null)
				return;

			if (Brush.IsNullOrEmpty(brush))
				SetBackgroundColor(Element.BackgroundColor);
			else
			{
				if (brush is SolidColorBrush solidColorBrush)
					_colorToRenderer = solidColorBrush.Color.ToUIColor();
				else
				{
					var backgroundImage = this.GetBackgroundImage(brush);
					_colorToRenderer = backgroundImage != null ? UIColor.FromPatternImage(backgroundImage) : UIColor.Clear;
				}
			}

			SetNeedsDisplay();
		}

		void SetCornerRadius()
		{
			if (Element == null)
				return;

			var elementCornerRadius = Element.CornerRadius;

			_topLeft = (nfloat)elementCornerRadius.TopLeft;
			_topRight = (nfloat)elementCornerRadius.TopRight;
			_bottomLeft = (nfloat)elementCornerRadius.BottomLeft;
			_bottomRight = (nfloat)elementCornerRadius.BottomRight;

			SetNeedsDisplay();
		}
	}
}