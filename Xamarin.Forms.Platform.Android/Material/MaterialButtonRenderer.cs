#if __ANDROID81__
#else
using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;
using static System.String;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.FastRenderers;
using MButton = Android.Support.Design.Button.MaterialButton;
using Xamarin.Forms.Platform.Android.Material;
using Android.Content.Res;
using Android.Support.V4.Widget;
using AMotionEventActions = Android.Views.MotionEventActions;
using Android.Support.V4.View;
using AColor = Android.Graphics.Color;
using AViewCompat = Android.Support.V4.View.ViewCompat;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using ADrawableCompat = Android.Support.V4.Graphics.Drawable.DrawableCompat;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Button), typeof(MaterialButtonRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialButtonRenderer : MButton, IVisualElementRenderer, AView.IOnAttachStateChangeListener,
		AView.IOnFocusChangeListener, AView.IOnClickListener, AView.IOnTouchListener, IViewRenderer, ITabStop, IBorderVisualElementRenderer
	{
		float _defaultFontSize;
		int? _defaultLabelFor;
		readonly int _defaultCornerRadius = 5;
		Typeface _defaultTypeface;
		int _imageHeight = -1;
		int _defaultIconPadding = -1;
		bool _isDisposed;
		bool _inputTransparent;
		Lazy<TextColorSwitcher> _textColorSwitcher;
		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		VisualElementTracker _tracker;
		VisualElementRenderer _visualElementRenderer;
		Thickness _paddingDeltaPix = new Thickness();
		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> _platformElementConfiguration;
		private Button _button;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public MaterialButtonRenderer(Context context) : base(new ContextThemeWrapper(context, Resource.Style.XamarinFormsMaterialTheme))
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

		public Color BackgroundColor => Element?.BackgroundColor != Color.Default ? Element.BackgroundColor : Color.Black;
		public VisualElement Element => Button;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		Button Button
		{
			get => _button;
			set
			{
				_button = value;
				_platformElementConfiguration = null;
			}
		}

		AView ITabStop.TabStop => this;

		void IOnClickListener.OnClick(AView v) => ButtonElementManager.OnClick(Button, Button, v);

		bool IOnTouchListener.OnTouch(AView v, MotionEvent e) => ButtonElementManager.OnTouch(Button, Button, v, e);

		void IOnAttachStateChangeListener.OnViewAttachedToWindow(AView attachedView)
		{
			UpdateText();
		}

		void IOnAttachStateChangeListener.OnViewDetachedFromWindow(AView detachedView)
		{
		}

		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			((IElementController)Button).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			UpdateText();

			AView view = this;

			// with material something is removing the padding between it being set and 
			// the measure call that's requested here
			UpdatePadding();
			view.Measure(widthConstraint, heightConstraint);

			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), MinimumSize());
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!(element is Button))
			{
				throw new ArgumentException($"{nameof(element)} must be of type {nameof(Button)}");
			}

			VisualElement oldElement = Button;
			Button = (Button)element;

			Performance.Start(out string reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}


			element.PropertyChanged += OnElementPropertyChanged;

			if (_tracker == null)
			{
				// Can't set up the tracker in the constructor because it access the Element (for now)
				SetTracker(new VisualElementTracker(this));
			}
			if (_visualElementRenderer == null)
			{
				_visualElementRenderer = new VisualElementRenderer(this);
			}

			OnElementChanged(new ElementChangedEventArgs<Button>(oldElement as Button, Button));

			SendVisualElementInitialized(element, this);

			Performance.Stop(reference);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
			{
				_defaultLabelFor = ViewCompat.GetLabelFor(this);
			}

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		void IVisualElementRenderer.UpdateLayout() => _tracker?.UpdateLayout();

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
			{
				return;
			}

			_isDisposed = true;

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

		Size MinimumSize()
		{
			return new Size();
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Button> e)
		{
			if (e.NewElement != null && !_isDisposed)
			{
				this.EnsureId();

				_textColorSwitcher = new Lazy<TextColorSwitcher>(
					() => new TextColorSwitcher(TextColors, e.NewElement.UseLegacyColorManagement()));

				UpdateFont();
				UpdateText();
				UpdateBitmap();
				UpdateTextColor();
				UpdateInputTransparent();
				UpdateBackgroundColor();
				UpdatePadding();

				ElevationHelper.SetElevation(this, e.NewElement);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Button.TextProperty.PropertyName)
			{
				UpdateText();
			}
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == Button.FontProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Button.ImageProperty.PropertyName)
			{
				UpdateBitmap();
			}
			else if (e.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
			{
				UpdateText();
			}
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent();
			}
			else if (e.PropertyName == Button.PaddingProperty.PropertyName)
			{
				UpdatePadding();
			}
			else if (e.PropertyName == Button.BorderWidthProperty.PropertyName || e.PropertyName == Button.CornerRadiusProperty.PropertyName || e.PropertyName == Button.BorderColorProperty.PropertyName)
				UpdateBorder();

			ElementPropertyChanged?.Invoke(this, e);
		}

		private void UpdateBorder()
		{
			int cornerRadius = _defaultCornerRadius;

			if (Element.IsSet(Button.CornerRadiusProperty) && Button.CornerRadius != (int)Button.CornerRadiusProperty.DefaultValue)
				cornerRadius = Button.CornerRadius;

			this.CornerRadius = (int)Context.ToPixels(cornerRadius);

			int[][] States =
			{
				new int[0]
			};

			Color borderColor = Button.BorderColor;
			if (borderColor.IsDefault)
			{
				StrokeColor = new global::Android.Content.Res.ColorStateList
				(
					States,
					new int[] { AColor.Transparent }
				);

				StrokeWidth = 0;
			}
			else
			{
				StrokeColor = new global::Android.Content.Res.ColorStateList
				(
					States,
					new int[] { borderColor.ToAndroid() }
				);

				StrokeWidth = (int)Button.BorderWidth;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (Element == null || _isDisposed)
			{
				return;
			}

			if (_imageHeight > -1)
			{
				// We've got an image (and no text); it's already centered horizontally,
				// we just need to adjust the padding so it centers vertically
				var diff = ((b - Context.ToPixels(Button.Padding.Bottom + Button.Padding.Top)) - t - _imageHeight) / 2;
				diff = Math.Max(diff, 0);
				UpdateContentEdge(new Thickness(0, diff, 0, -diff));
			}
			else
			{
				UpdateContentEdge();
			}




			base.OnLayout(changed, l, t, r, b);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{

			base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			var images = TextViewCompat.GetCompoundDrawablesRelative(this);
			if (images.Length > 0 && images[0] != null && Button.ContentLayout.Position == Button.ButtonContentLayout.ImagePosition.Right)
			{
				var bounds = images[0].Bounds;
				int width = images[0].IntrinsicWidth;
				Icon.SetBounds(-bounds.Left, bounds.Top, width - bounds.Left, bounds.Bottom);
				TextViewCompat.SetCompoundDrawablesRelative(this, null, null, Icon, null);
			}

		}

		void SetTracker(VisualElementTracker tracker)
		{
			_tracker = tracker;
		}

		void UpdateBackgroundColor()
		{
			int[][] States =
			{
				new int[0] {  }
			};

			ColorStateList colorStateList = new ColorStateList(
						States,
						new int[] { BackgroundColor.ToAndroid() }
				);

			AViewCompat.SetBackgroundTintList(this, colorStateList);
		}

		internal void OnNativeFocusChanged(bool hasFocus)
		{
		}

		internal void SendVisualElementInitialized(VisualElement element, AView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void UpdateBitmap()
		{
			if (Element == null || _isDisposed)
			{
				return;
			}

			FileImageSource elementImage = Button.Image;
			string imageFile = elementImage?.File;
			_imageHeight = -1;

			if (elementImage == null || IsNullOrEmpty(imageFile))
			{
				SetCompoundDrawablesWithIntrinsicBounds(null, null, null, null);
				return;
			}

			Drawable image = Context.GetDrawable(imageFile);
			Button.ButtonContentLayout layout = Button.ContentLayout;

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
				if (IsNullOrEmpty(Button.Text))
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
			if (Element == null || _isDisposed)
			{
				return;
			}

			Font font = Button.Font;

			if (font == Font.Default && _defaultFontSize == 0f)
			{
				return;
			}

			if (_defaultFontSize == 0f)
			{
				_defaultTypeface = Typeface;
				_defaultFontSize = TextSize;
			}

			if (font == Font.Default)
			{
				Typeface = _defaultTypeface;
				SetTextSize(ComplexUnitType.Px, _defaultFontSize);
			}
			else
			{
				Typeface = font.ToTypeface();
				SetTextSize(ComplexUnitType.Sp, font.ToScaledPixel());
			}
		}

		void UpdateInputTransparent()
		{
			if (Element == null || _isDisposed)
			{
				return;
			}

			_inputTransparent = Element.InputTransparent;
		}

		void UpdateText()
		{
			if (Element == null || _isDisposed)
			{
				return;
			}

			string oldText = Text;
			Text = Button.Text;

			// If we went from or to having no text, we need to update the image position
			if (IsNullOrEmpty(oldText) != IsNullOrEmpty(Text))
			{
				UpdateBitmap();
			}
		}

		void UpdateTextColor()
		{
			if (Element == null || _isDisposed || _textColorSwitcher == null)
				return;

			if (Button.TextColor == Color.Default)
				_textColorSwitcher.Value.UpdateTextColor(this, Button.TextColor);
			else
				_textColorSwitcher.Value.UpdateTextColor(this, Color.White);
		}

		void UpdatePadding()
		{
			if (Element.IsSet(Button.PaddingProperty))
				SetPadding(
					(int)(Context.ToPixels(Button.Padding.Left) + _paddingDeltaPix.Left),
					(int)(Context.ToPixels(Button.Padding.Top) + _paddingDeltaPix.Top),
					(int)(Context.ToPixels(Button.Padding.Right) + _paddingDeltaPix.Right),
					(int)(Context.ToPixels(Button.Padding.Bottom) + _paddingDeltaPix.Bottom)
				);
		}

		void UpdateContentEdge(Thickness? delta = null)
		{
			_paddingDeltaPix = delta ?? new Thickness();
			UpdatePadding();
		}

		float IBorderVisualElementRenderer.ShadowRadius => ShadowRadius;
		float IBorderVisualElementRenderer.ShadowDx => ShadowDx;
		float IBorderVisualElementRenderer.ShadowDy => ShadowDy;
		AColor IBorderVisualElementRenderer.ShadowColor => ShadowColor;
		bool IBorderVisualElementRenderer.UseDefaultPadding() => OnThisPlatform().UseDefaultPadding();
		bool IBorderVisualElementRenderer.UseDefaultShadow() => OnThisPlatform().UseDefaultShadow();
		bool IBorderVisualElementRenderer.IsShadowEnabled() => true;
		AView IBorderVisualElementRenderer.View => this;

		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> OnThisPlatform()
		{
			if (_platformElementConfiguration == null)
				_platformElementConfiguration = Button.OnThisPlatform();

			return _platformElementConfiguration;
		}
	}
}
#endif
