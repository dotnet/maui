using System.ComponentModel;
using System.Drawing;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class FrameRenderer : VisualElementRenderer<Frame>
	{
		ShadowView _shadowView;

		[Internals.Preserve(Conditional = true)]
		public FrameRenderer()
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
				SetupLayer();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
			    e.PropertyName == Xamarin.Forms.Frame.BorderColorProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.HasShadowProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.CornerRadiusProperty.PropertyName)
				SetupLayer();
		}

		public virtual void SetupLayer()
		{
			float cornerRadius = Element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // default corner radius

			Layer.CornerRadius = cornerRadius;

			if (Element.BackgroundColor == Color.Default)
				Layer.BackgroundColor = UIColor.White.CGColor;
			else
				Layer.BackgroundColor = Element.BackgroundColor.ToCGColor();

			if (Element.BorderColor == Color.Default)
				Layer.BorderColor = UIColor.Clear.CGColor;
			else
			{
				Layer.BorderColor = Element.BorderColor.ToCGColor();
				Layer.BorderWidth = 1;
			}

			if (Element.HasShadow)
			{
				if (_shadowView == null)
				{
					_shadowView = new ShadowView(Layer);
					SetNeedsLayout();
				}
				_shadowView.UpdateBackgroundColor();
				_shadowView.Layer.CornerRadius = Layer.CornerRadius;
				_shadowView.Layer.BorderColor = Layer.BorderColor;
			}
			else
			{
				if (_shadowView != null)
				{
					_shadowView.RemoveFromSuperview();
					_shadowView.Dispose();
					_shadowView = null;
				}
			}

			Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			Layer.ShouldRasterize = true;
		}

		public override void LayoutSubviews()
		{
			if (_shadowView != null)
			{
				if (_shadowView.Superview == null)
					Superview.InsertSubviewBelow(_shadowView, this);

				_shadowView?.SetNeedsLayout();
			}
			base.LayoutSubviews();
		}

		[Preserve(Conditional = true)]
		class ShadowView : UIView
		{
			CALayer _shadowee;
			CGRect _previousBounds;
			CGRect _previousFrame;

			[Preserve(Conditional = true)]
			public ShadowView(CALayer shadowee)
			{
				_shadowee = shadowee;
				Layer.ShadowRadius = 5;
				Layer.ShadowColor = UIColor.Black.CGColor;
				Layer.ShadowOpacity = 0.8f;
				Layer.ShadowOffset = new SizeF();
				Layer.BorderWidth = 1;
				UserInteractionEnabled = false;
			}

			public void UpdateBackgroundColor()
			{
				//Putting a transparent background under any shadowee having a background with alpha < 1
				//Giving the Shadow a background of the same color when shadowee background == 1.
				//The latter will result in a 'darker' shadow as you would expect from something that 
				//isn't transparent. This also mimics the look as it was before with non-transparent Frames.
				if (_shadowee.BackgroundColor.Alpha < 1) 
					BackgroundColor = UIColor.Clear;
				else
					BackgroundColor = new UIColor(_shadowee.BackgroundColor);
			}

			public override void LayoutSubviews()
			{
				if (_shadowee.Bounds != _previousBounds || _shadowee.Frame != _previousFrame)
				{
					base.LayoutSubviews();
					SetBounds();
				}
			}

			void SetBounds()
			{
				Layer.Frame = _shadowee.Frame;
				Layer.Bounds = _shadowee.Bounds;
				_previousBounds = _shadowee.Bounds;
				_previousFrame = _shadowee.Frame;
			}
		}
	}
}