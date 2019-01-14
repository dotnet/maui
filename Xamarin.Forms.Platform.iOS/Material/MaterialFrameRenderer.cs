using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms;
using MCard = MaterialComponents.Card;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Frame), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialFrameRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialFrameRenderer : MCard,
		IVisualElementRenderer
	{
		CardScheme _defaultCardScheme;
		CardScheme _cardScheme;

		nfloat _defaultCornerRadius = -1f;

		VisualElementPackager _packager;
		VisualElementTracker _tracker;
		bool _disposed = false;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public MaterialFrameRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		public Frame Element { get; private set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (_packager == null)
					return;

				SetElement(null);

				_packager.Dispose();
				_packager = null;

				_tracker.NativeControlUpdated -= OnNativeControlUpdated;
				_tracker.Dispose();
				_tracker = null;
			}

			base.Dispose(disposing);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			var content = Element?.Content;
			if (content != null && uiview == Platform.GetRenderer(content))
			{
				uiview.Layer.Mask = null;
			}

			base.WillRemoveSubview(uiview);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			// if the card's shadow has a path/shape, then use that to clip the contents
			var content = Element?.Content;
			if (content != null && Layer is ShapedShadowLayer shadowLayer && shadowLayer.ShapeLayer.Path is CGPath shapePath)
			{
				var renderer = Platform.GetRenderer(content);
				if (renderer is UIView uiview)
				{
					var padding = Element.Padding;
					var offset = CGAffineTransform.MakeTranslation((nfloat)(-padding.Left), (nfloat)(-padding.Top));
					uiview.Layer.Mask = new CAShapeLayer
					{
						Path = new CGPath(shapePath, offset)
					};
				}
			}
		}

		protected virtual CardScheme CreateCardScheme()
		{
			return new CardScheme
			{
				ColorScheme = MaterialColors.Light.CreateColorScheme(),
				ShapeScheme = new ShapeScheme(),
			};
		}

		protected virtual void ApplyTheme()
		{
			if (Element.BorderColor.IsDefault)
				CardThemer.ApplyScheme(_cardScheme, this);
			else
				CardThemer.ApplyOutlinedVariant(_cardScheme, this);

			// a special case for no shadow
			if (!Element.HasShadow)
				SetShadowElevation(0, UIControlState.Normal);

			// this is set in the theme, so we must always disable it
			Interactable = false;
		}

		public void SetElement(VisualElement element)
		{
			_cardScheme?.Dispose();
			_cardScheme = CreateCardScheme();

			var oldElement = Element;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (element is null)
				Element = null;
			else
				Element = element as Frame ?? throw new ArgumentException("Element must be of type Frame.");

			if (Element != null)
			{
				if (_packager == null)
				{
					_defaultCardScheme = CreateCardScheme();

					_packager = new VisualElementPackager(this);
					_packager.Load();

					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += OnNativeControlUpdated;
				}

				Element.PropertyChanged += OnElementPropertyChanged;

				UpdateCornerRadius();
				UpdateBorderColor();
				UpdateBackgroundColor();

				ApplyTheme();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var updatedTheme = false;

			if (e.PropertyName == Xamarin.Forms.Frame.HasShadowProperty.PropertyName)
			{
				// this is handled in ApplyTheme
				updatedTheme = true;
			}
			else if (e.PropertyName == Xamarin.Forms.Frame.CornerRadiusProperty.PropertyName)
			{
				UpdateCornerRadius();
			}
			else if (e.PropertyName == Xamarin.Forms.Frame.BorderColorProperty.PropertyName)
			{
				UpdateBorderColor();
				updatedTheme = true;
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackgroundColor();
				updatedTheme = true;
			}

			if (updatedTheme)
				ApplyTheme();
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			ElementChanged?.Invoke(this, e);
		}

		void OnNativeControlUpdated(object sender, EventArgs eventArgs)
		{
		}

		void UpdateCornerRadius()
		{
			// set the default radius on the first time
			if (_defaultCornerRadius < 0)
				_defaultCornerRadius = CornerRadius;

			var cornerRadius = Element.CornerRadius;
			if (cornerRadius < 0)
				CornerRadius = _defaultCornerRadius;
			else
				CornerRadius = cornerRadius;
		}

		void UpdateBorderColor()
		{
			if (_cardScheme.ColorScheme is SemanticColorScheme colorScheme)
			{
				var borderColor = Element.BorderColor;
				if (borderColor.IsDefault)
					colorScheme.OnSurfaceColor = _defaultCardScheme.ColorScheme.OnSurfaceColor;
				else
					colorScheme.OnSurfaceColor = borderColor.ToUIColor();
			}
		}

		void UpdateBackgroundColor()
		{
			if (_cardScheme.ColorScheme is SemanticColorScheme colorScheme)
			{
				var bgColor = Element.BackgroundColor;
				if (bgColor.IsDefault)
					colorScheme.SurfaceColor = _defaultCardScheme.ColorScheme.SurfaceColor;
				else
					colorScheme.SurfaceColor = bgColor.ToUIColor();
			}
		}

		// IVisualElementRenderer

		VisualElement IVisualElementRenderer.Element => Element;

		UIView IVisualElementRenderer.NativeView => this;

		UIViewController IVisualElementRenderer.ViewController => null;

		SizeRequest IVisualElementRenderer.GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetSizeRequest(widthConstraint, heightConstraint, 44, 44);

		void IVisualElementRenderer.SetElement(VisualElement element) =>
			SetElement(element);

		void IVisualElementRenderer.SetElementSize(Size size) =>
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
	}
}