#if __ANDROID_28__
using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.Platform.Android.Material;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using AViewCompat = Android.Support.V4.View.ViewCompat;
using MButton = Android.Support.Design.Button.MaterialButton;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Button), typeof(MaterialButtonRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialButtonRenderer : MButton,
		IBorderVisualElementRenderer, IVisualElementRenderer, IViewRenderer, ITabStop,
		AView.IOnAttachStateChangeListener, AView.IOnFocusChangeListener, AView.IOnClickListener, AView.IOnTouchListener
	{
		int _defaultCornerRadius = -1;
		int _defaultBorderWidth = -1;
		ColorStateList _defaultBorderColor;
		float _defaultFontSize = -1;
		int? _defaultLabelFor;
		int _defaultIconPadding = -1;
		Typeface _defaultTypeface;

		bool _disposed;
		bool _inputTransparent;
		Thickness _paddingDeltaPix;
		int _imageHeight = -1;

		Button _button;

		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> _platformElementConfiguration;
		VisualElementTracker _tracker;
		VisualElementRenderer _visualElementRenderer;

		readonly AutomationPropertiesProvider _automationPropertiesProvider;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public MaterialButtonRenderer(Context context)
			: base(new ContextThemeWrapper(context, Resource.Style.XamarinFormsMaterialTheme))
		{
			VisualElement.VerifyVisualFlagEnabled();

			_automationPropertiesProvider = new AutomationPropertiesProvider(this);

			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			AddOnAttachStateChangeListener(this);
			OnFocusChangeListener = this;

			Tag = this;
		}

		protected MButton Control => this;

		protected Button Element
		{
			get => _button;
			set
			{
				if (_button == value)
					return;

				var oldElement = _button;

				_button = value;
				_platformElementConfiguration = null;

				Performance.Start(out string reference);

				if (oldElement != null)
				{
					oldElement.PropertyChanged -= OnElementPropertyChanged;
				}

				_button.PropertyChanged += OnElementPropertyChanged;

				// Can't set up the tracker in the constructor because it access the Element (for now)
				if (_tracker == null)
					_tracker = new VisualElementTracker(this);

				if (_visualElementRenderer == null)
					_visualElementRenderer = new VisualElementRenderer(this);

				OnElementChanged(new ElementChangedEventArgs<Button>(oldElement, Element));

				_button.SendViewInitialized(this);

				Performance.Stop(reference);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				SetOnClickListener(null);
				SetOnTouchListener(null);
				RemoveOnAttachStateChangeListener(this);

				_automationPropertiesProvider?.Dispose();
				_tracker?.Dispose();
				_visualElementRenderer?.Dispose();

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (!Enabled || (_inputTransparent && Enabled))
				return false;

			return base.OnTouchEvent(e);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (e.NewElement != null && !_disposed)
			{
				this.EnsureId();

				UpdateBorder();
				UpdateFont();
				UpdateImage();
				UpdatePadding();
				UpdateText();
				UpdatePrimaryColors();
				UpdateInputTransparent();

				ElevationHelper.SetElevation(this, e.NewElement);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName || e.PropertyName == Button.CornerRadiusProperty.PropertyName)
				UpdateBorder();
			else if (e.PropertyName == Button.FontProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
				UpdateImage();
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
				UpdatePadding();
			else if (e.PropertyName == Button.TextProperty.PropertyName || e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
				UpdateText();
			else if (e.PropertyName == Button.TextColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdatePrimaryColors();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (_disposed || Element == null)
				return;

			if (_imageHeight > -1)
			{
				// We've got an image (and no text); it's already centered horizontally,
				// we just need to adjust the padding so it centers vertically
				var diff = ((bottom - Context.ToPixels(Element.Padding.Bottom + Element.Padding.Top)) - top - _imageHeight) / 2;
				diff = Math.Max(diff, 0);
				UpdateContentEdge(new Thickness(0, diff, 0, -diff));
			}
			else
			{
				UpdateContentEdge();
			}

			base.OnLayout(changed, left, top, right, bottom);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			var images = TextViewCompat.GetCompoundDrawablesRelative(this);
			if (images.Length > 0 && images[0] != null && Element.ContentLayout.Position == Button.ButtonContentLayout.ImagePosition.Right)
			{
				var bounds = images[0].Bounds;
				int width = images[0].IntrinsicWidth;
				Icon.SetBounds(-bounds.Left, bounds.Top, width - bounds.Left, bounds.Bottom);
				TextViewCompat.SetCompoundDrawablesRelative(this, null, null, Icon, null);
			}
		}

		void UpdateImage()
		{
			if (_disposed || Element == null)
				return;

			FileImageSource elementImage = Element.Image;
			string imageFile = elementImage?.File;
			_imageHeight = -1;

			if (elementImage == null || string.IsNullOrEmpty(imageFile))
			{
				SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
				return;
			}

			Drawable image = Context.GetDrawable(imageFile);
			Button.ButtonContentLayout layout = Element.ContentLayout;

			if (_defaultIconPadding == -1)
				_defaultIconPadding = IconPadding;

			if (_defaultIconPadding == -1)
				_defaultIconPadding = 0;

			// disable tint for now
			IconTint = null;
			Icon = image;

			if (layout.Position == Button.ButtonContentLayout.ImagePosition.Right || layout.Position == Button.ButtonContentLayout.ImagePosition.Left)
			{
				IconGravity = IconGravityTextStart;
				// setting the icon property causes the base class to calculate things like padding
				// required to set the image to the start of the text
				if (string.IsNullOrEmpty(Element.Text))
					IconPadding = 0;
				else
					IconPadding = (int)Context.ToPixels(layout.Spacing) + _defaultIconPadding;

				image?.Dispose();
				image = TextViewCompat.GetCompoundDrawablesRelative(this)[0];
			}

			switch (layout.Position)
			{
				case Button.ButtonContentLayout.ImagePosition.Top:
					TextViewCompat.SetCompoundDrawablesRelativeWithIntrinsicBounds(this, null, image, null, null);
					break;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					TextViewCompat.SetCompoundDrawablesRelativeWithIntrinsicBounds(this, null, null, null, image);
					break;
				case Button.ButtonContentLayout.ImagePosition.Right:
					// this gets set and updated inside OnMeasure
					break;
				default:
					// Defaults to image on the left
					TextViewCompat.SetCompoundDrawablesRelative(this, image, null, null, null);
					break;
			}
		}

		void UpdateFont()
		{
			if (_disposed || Element == null)
				return;

			Font font = Element.Font;
			if (font == Font.Default && _defaultTypeface == null && _defaultFontSize < 0f)
				return;

			if (_defaultTypeface == null)
				_defaultTypeface = Typeface;

			if (font == Font.Default)
				Typeface = _defaultTypeface;
			else
				Typeface = font.ToTypeface();

			if (_defaultFontSize < 0f)
				_defaultFontSize = TextSize;

			if (Element.FontSize < 0f)
				SetTextSize(ComplexUnitType.Px, _defaultFontSize);
			else
				SetTextSize(ComplexUnitType.Sp, font.ToScaledPixel());
		}

		void UpdateText()
		{
			if (_disposed || Element == null)
				return;

			string oldText = Text;
			Text = Element.Text;

			// If we went from or to having no text, we need to update the image position
			if (string.IsNullOrEmpty(oldText) != string.IsNullOrEmpty(Text))
			{
				UpdateImage();
			}
		}

		void UpdatePadding()
		{
			if (Element.IsSet(Button.PaddingProperty))
			{
				SetPadding(
					(int)(Context.ToPixels(Element.Padding.Left) + _paddingDeltaPix.Left),
					(int)(Context.ToPixels(Element.Padding.Top) + _paddingDeltaPix.Top),
					(int)(Context.ToPixels(Element.Padding.Right) + _paddingDeltaPix.Right),
					(int)(Context.ToPixels(Element.Padding.Bottom) + _paddingDeltaPix.Bottom));
			}
		}

		void UpdateBorder()
		{
			if (_disposed || Element == null)
				return;

			var cornerRadius = Element.CornerRadius;
			if (cornerRadius >= 0f || _defaultCornerRadius >= 0f)
			{
				if (_defaultCornerRadius < 0f)
					_defaultCornerRadius = CornerRadius;

				if (cornerRadius < 0f)
					CornerRadius = _defaultCornerRadius;
				else
					CornerRadius = (int)Context.ToPixels(cornerRadius);
			}

			Color borderColor = Element.BorderColor;
			if (!borderColor.IsDefault || _defaultBorderColor != null)
			{
				if (_defaultBorderColor == null)
					_defaultBorderColor = StrokeColor;

				if (borderColor.IsDefault)
					StrokeColor = _defaultBorderColor;
				else
					base.StrokeColor = new ColorStateList(new[] { new int[0] }, new int[] { borderColor.ToAndroid() });
			}

			double borderWidth = Element.BorderWidth;
			if (borderWidth >= 0f || _defaultBorderWidth >= 0f)
			{
				if (_defaultBorderWidth < 0)
					_defaultBorderWidth = StrokeWidth;

				if (borderWidth < 0f)
					StrokeWidth = _defaultBorderWidth;
				else
					base.StrokeWidth = (int)Context.ToPixels(borderWidth);
			}
		}

		void UpdateInputTransparent()
		{
			if (_disposed || Element == null)
				return;

			_inputTransparent = Element.InputTransparent;
		}

		void UpdatePrimaryColors()
		{
			if (_disposed || Element == null)
				return;

			// background
			Color backgroundColor = Element.BackgroundColor;
			AColor background;
			if (backgroundColor.IsDefault)
				background = MaterialColors.Light.PrimaryColor;
			else
				background = backgroundColor.ToAndroid();

			// text
			Color textColor = Element.TextColor;
			AColor text;
			if (textColor.IsDefault)
				text = MaterialColors.Light.OnPrimaryColor;
			else
				text = textColor.ToAndroid();

			// apply
			SetTextColor(MaterialColors.CreateButtonTextColors(background, text));
			AViewCompat.SetBackgroundTintList(this, MaterialColors.CreateButtonBackgroundColors(background));
		}

		void UpdateContentEdge(Thickness? delta = null)
		{
			_paddingDeltaPix = delta ?? new Thickness();
			UpdatePadding();
		}

		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> OnThisPlatform() =>
			_platformElementConfiguration ?? (_platformElementConfiguration = Element.OnThisPlatform());

		// IOnAttachStateChangeListener

		void IOnAttachStateChangeListener.OnViewAttachedToWindow(AView attachedView) =>
			UpdateText();

		void IOnAttachStateChangeListener.OnViewDetachedFromWindow(AView detachedView)
		{
		}

		// IOnFocusChangeListener

		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus) =>
			Element.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);

		// IOnClickListener

		void IOnClickListener.OnClick(AView v) => ButtonElementManager.OnClick(Element, Element, v);

		// IOnTouchListener

		bool IOnTouchListener.OnTouch(AView v, MotionEvent e) => ButtonElementManager.OnTouch(Element, Element, v, e);

		// IBorderVisualElementRenderer

		float IBorderVisualElementRenderer.ShadowRadius => ShadowRadius;
		float IBorderVisualElementRenderer.ShadowDx => ShadowDx;
		float IBorderVisualElementRenderer.ShadowDy => ShadowDy;
		AColor IBorderVisualElementRenderer.ShadowColor => ShadowColor;
		bool IBorderVisualElementRenderer.UseDefaultPadding() => OnThisPlatform().UseDefaultPadding();
		bool IBorderVisualElementRenderer.UseDefaultShadow() => OnThisPlatform().UseDefaultShadow();
		bool IBorderVisualElementRenderer.IsShadowEnabled() => true;
		VisualElement IBorderVisualElementRenderer.Element => Element;
		AView IBorderVisualElementRenderer.View => this;

		// IVisualElementRenderer

		VisualElement IVisualElementRenderer.Element => Element;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		AView IVisualElementRenderer.View => this;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			UpdateText();

			AView view = this;

			// with material something is removing the padding between it being set and 
			// the measure call that's requested here
			UpdatePadding();
			view.Measure(widthConstraint, heightConstraint);

			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size());
		}

		void IVisualElementRenderer.SetElement(VisualElement element) =>
			Element = (element as Button) ?? throw new ArgumentException("Element must be of type Button.");

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);
			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		void IVisualElementRenderer.UpdateLayout() =>
			_tracker?.UpdateLayout();

		// IViewRenderer

		void IViewRenderer.MeasureExactly() =>
			ViewRenderer.MeasureExactly(this, Element, Context);

		// ITabStop

		AView ITabStop.TabStop => this;
	}
}
#endif
