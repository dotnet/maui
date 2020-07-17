using System;
using System.ComponentModel;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using MCard = MaterialComponents.Card;

namespace Xamarin.Forms.Material.iOS
{
	public class MaterialFrameRenderer : MCard,
		IVisualElementRenderer
	{
		CardScheme _defaultCardScheme;
		CardScheme _cardScheme;
		float _defaultCornerRadius = -1;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;
		EventTracker _events;
		CGSize? _backgroundSize;

		bool _disposed = false;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public Frame Element { get; private set; }

		public override void WillRemoveSubview(UIView uiview)
		{
			var content = Element?.Content;
			if (content != null && uiview == Platform.iOS.Platform.GetRenderer(content))
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
				var renderer = Platform.iOS.Platform.GetRenderer(content);
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

			ApplyThemeIfNeeded();
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

					_events = new EventTracker(this);
					_events.LoadEvents(this);
				}

				Element.PropertyChanged += OnElementPropertyChanged;

				ApplyTheme();
			}

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			if (element != null)
				element.SendViewInitialized(this);

			if (!string.IsNullOrEmpty(element?.AutomationId))
				AccessibilityIdentifier = element.AutomationId;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (_packager == null)
					return;

				_packager.Dispose();
				_packager = null;

				_tracker.Dispose();
				_tracker = null;

				_events.Dispose();
				_events = null;

				if (Element != null)
				{
					Element.ClearValue(Platform.iOS.Platform.RendererProperty);
					SetElement(null);
				}
			}

			base.Dispose(disposing);
		}


		protected virtual CardScheme CreateCardScheme()
		{
			return new CardScheme
			{
				ColorScheme = MaterialColors.Light.CreateColorScheme()
			};
		}

		protected virtual void ApplyTheme()
		{
			UpdateCornerRadius();
			UpdateBorderColor();
			UpdateBackground();

			if (Element.BorderColor.IsDefault)
				CardThemer.ApplyScheme(_cardScheme, this);
			else
				CardThemer.ApplyOutlinedVariant(_cardScheme, this);

			// a special case for no shadow
			if (!Element.HasShadow)
				SetShadowElevation(0, UIControlState.Normal);

			if (Element.GestureRecognizers != null && Element.GestureRecognizers.Any())
			{
				Interactable = true;

				// disable ink (ripple) and elevation effect while tapped
				InkView.Hidden = true;
				if (Element.HasShadow)
					SetShadowElevation(1f, UIControlState.Highlighted);
			}
			else
			{
				// this is set in the theme, so we must always disable it		
				Interactable = false;
			}
		}

		protected virtual void ApplyThemeIfNeeded()
		{
			var bgBrush = Element.Background;

			if (Brush.IsNullOrEmpty(bgBrush))
				return;

			var backgroundImage = this.GetBackgroundImage(bgBrush);

			if (_backgroundSize != null && _backgroundSize != backgroundImage?.Size)
				ApplyTheme();
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
				updatedTheme = true;
			}
			else if (e.PropertyName == Xamarin.Forms.Frame.BorderColorProperty.PropertyName)
			{
				updatedTheme = true;
			}
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				updatedTheme = true;
			}
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
			{
				updatedTheme = true;
			}

			if (updatedTheme)
				ApplyTheme();
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e) => ElementChanged?.Invoke(this, e);

		void UpdateCornerRadius()
		{
			// set the default radius on the first time
			if (_defaultCornerRadius < 0)
				_defaultCornerRadius = (float)MaterialColors.kFrameCornerRadiusDefault;

			var cornerRadius = Element.CornerRadius;
			if (cornerRadius < 0)
				cornerRadius = _defaultCornerRadius;

			if (_cardScheme != null)
			{
				var shapeScheme = new ShapeScheme();
				var shapeCategory = new ShapeCategory(ShapeCornerFamily.Rounded, cornerRadius);

				shapeScheme.SmallComponentShape = shapeCategory;
				shapeScheme.MediumComponentShape = shapeCategory;
				shapeScheme.LargeComponentShape = shapeCategory;

				_cardScheme.ShapeScheme = shapeScheme;
			}

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

				SetBorderWidth(borderColor.IsDefault ? 0f : 1f, UIControlState.Normal);
			}
		}
				
		void UpdateBackground()
		{
			if (_cardScheme.ColorScheme is SemanticColorScheme colorScheme)
			{
				colorScheme.SurfaceColor = UIColor.Clear;

				var bgBrush = Element.Background;

				if (Brush.IsNullOrEmpty(bgBrush))
				{
					var bgColor = Element.BackgroundColor;
					if (bgColor.IsDefault)
						colorScheme.SurfaceColor = _defaultCardScheme.ColorScheme.SurfaceColor;
					else
						colorScheme.SurfaceColor = bgColor.ToUIColor();
				}
				else
				{
					var backgroundImage = this.GetBackgroundImage(bgBrush);
					_backgroundSize = backgroundImage?.Size;
					colorScheme.SurfaceColor = UIColor.FromPatternImage(backgroundImage);
				}
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

		void IVisualElementRenderer.SetElementSize(Size size)
		{
			Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
			UpdateBackground();
		}
	}
}
