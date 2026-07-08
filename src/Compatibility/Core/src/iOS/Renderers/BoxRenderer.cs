using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class BoxRenderer : VisualElementRenderer<BoxView>
	{
		UIColor _colorToRenderer;
		SizeF _previousSize;
		nfloat _topLeft;
		nfloat _topRight;
		nfloat _bottomLeft;
		nfloat _bottomRight;

		const float PI = MathF.PI;
		const float PIAndAHalf = PI * 1.5f;
		const float HalfPI = PI * .5f;
		const float TwoPI = PI * 2;

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public BoxRenderer()
		{

		}

		public override void Draw(RectangleF rect)
		{
			if (_colorToRenderer != null)
			{
				UIBezierPath bezierPath = new UIBezierPath();

				bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + Bounds.Width - _topRight, Bounds.Y + _topRight), _topRight, PIAndAHalf, TwoPI, true);
				bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + Bounds.Width - _bottomRight, Bounds.Y + Bounds.Height - _bottomRight), _bottomRight, 0, HalfPI, true);
				bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + _bottomLeft, Bounds.Y + Bounds.Height - _bottomLeft), _bottomLeft, HalfPI, PI, true);
				bezierPath.AddArc(new CoreGraphics.CGPoint(Bounds.X + _topLeft, Bounds.Y + _topLeft), _topLeft, PI, PIAndAHalf, true);

				_colorToRenderer.SetFill();
				bezierPath.Fill();
			}

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
				SetBackground(Element.Background);
				SetCornerRadius();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == BoxView.ColorProperty.PropertyName)
				SetBackground(Element.Background);
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

			if (elementColor != null)
				_colorToRenderer = elementColor.ToPlatform();
			else if (color != null)
				_colorToRenderer = color.ToPlatform();

			SetNeedsDisplay();
		}

		protected override void SetBackground(Brush brush)
		{
			if (Element == null)
				return;

			if (Brush.IsNullOrEmpty(brush))
				brush = Element.Background;

			if (Brush.IsNullOrEmpty(brush))
			{
				SetBackgroundColor(Element.BackgroundColor);
			}
			else
			{
				if (brush is SolidColorBrush solidColorBrush)
					_colorToRenderer = solidColorBrush.Color.ToPlatform();
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

		static float DefaultWidth = 40;
		static float DefaultHeight = 40;
		static SizeF DefaultSize = new SizeF(DefaultWidth, DefaultHeight);

		public override SizeF SizeThatFits(SizeF size)
		{
			// Creating a custom override for measuring the BoxView on iOS; this reports the same default size that's 
			// specified in the old OnMeasure method. Normally we'd just do this centrally in the xplat code or override
			// GetDesiredSize in a BoxViewHandler. But BoxView is a legacy control (replaced by Shapes), so we don't want
			// to bring that into the new stuff. 

			if (Element != null)
			{
				var heightRequest = Element.HeightRequest;
				var widthRequest = Element.WidthRequest;

				var height = heightRequest >= 0 ? heightRequest : DefaultHeight;
				var width = widthRequest >= 0 ? widthRequest : DefaultWidth;

				return new SizeF(width, height);
			}

			return DefaultSize;
		}
	}
}