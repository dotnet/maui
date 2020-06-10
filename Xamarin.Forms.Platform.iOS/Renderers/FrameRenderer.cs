using System.ComponentModel;
using System.Drawing;
using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class FrameRenderer : VisualElementRenderer<Frame>, ITabStop
	{
		UIView _actualView = new UIView();
		CGSize _previousSize;

		UIView ITabStop.TabStop => this;

		[Internals.Preserve(Conditional = true)]
		public FrameRenderer()
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				// Add the subviews to the actual view.
				foreach (var item in NativeView.Subviews)
				{
					_actualView.AddSubview(item);
				}

				// Make sure the gestures still work on our subview
				if (NativeView.GestureRecognizers != null)
				{
					foreach (var gesture in NativeView.GestureRecognizers)
						_actualView.AddGestureRecognizer(gesture);
				}
				else if (_actualView.Subviews.Length == 0)
				{
					_actualView.UserInteractionEnabled = false;
				}

				AddSubview(_actualView);
				SetupLayer();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
			    e.PropertyName == Xamarin.Forms.Frame.BorderColorProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.HasShadowProperty.PropertyName ||
				e.PropertyName == Xamarin.Forms.Frame.CornerRadiusProperty.PropertyName ||
				e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				SetupLayer();
		}

		public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
		{
			base.TraitCollectionDidChange(previousTraitCollection);
#if __XCODE11__
			// Make sure the control adheres to changes in UI theme
			if (Forms.IsiOS13OrNewer && previousTraitCollection?.UserInterfaceStyle != TraitCollection.UserInterfaceStyle)
				SetupLayer();
#endif
		}

		public virtual void SetupLayer()
		{
			float cornerRadius = Element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = 5f; // default corner radius

			_actualView.Layer.CornerRadius = cornerRadius;

			if (Element.BackgroundColor == Color.Default)
				_actualView.Layer.BackgroundColor = ColorExtensions.BackgroundColor.CGColor;
			else
				_actualView.Layer.BackgroundColor = Element.BackgroundColor.ToCGColor();

			if (Element.BorderColor == Color.Default)
				_actualView.Layer.BorderColor = UIColor.Clear.CGColor;
			else
			{
				_actualView.Layer.BorderColor = Element.BorderColor.ToCGColor();
				_actualView.Layer.BorderWidth = 1;
			}

			if (Element.HasShadow)
			{
				Layer.ShadowRadius = 5;
				Layer.ShadowColor = UIColor.Black.CGColor;
				Layer.ShadowOpacity = 0.8f;
				Layer.ShadowOffset = new SizeF();
			}
			else
			{
				Layer.ShadowOpacity = 0;
			}

			Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			Layer.ShouldRasterize = true;

			_actualView.Layer.RasterizationScale = UIScreen.MainScreen.Scale;
			_actualView.Layer.ShouldRasterize = true;
		}

		public override void LayoutSubviews()
		{
			if (_previousSize != Bounds.Size)
				SetNeedsDisplay();

			base.LayoutSubviews();
		}

		public override void Draw(CGRect rect)
		{
			_actualView.Frame = Bounds;

			base.Draw(rect);

			_previousSize = Bounds.Size;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_actualView != null)
				{
					
					for (var i = 0; i < _actualView.GestureRecognizers?.Length; i++)
						_actualView.GestureRecognizers.Remove(_actualView.GestureRecognizers[i]);

					for (var j = 0; j < _actualView.Subviews.Length; j++)
						_actualView.Subviews.Remove(_actualView.Subviews[j]);

					_actualView.RemoveFromSuperview();
					_actualView.Dispose();
					_actualView = null;
				}
			}
		}
	}
}