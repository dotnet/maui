using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Internals;
using GlobalResource = Android.Resource;
using AView = Android.Views.View;
using AMotionEvent = Android.Views.MotionEvent;
using AMotionEventActions = Android.Views.MotionEventActions;
using static System.String;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal sealed class ButtonRenderer : AppCompatButton, IVisualElementRenderer, AView.IOnAttachStateChangeListener,
		AView.IOnFocusChangeListener, IEffectControlProvider, AView.IOnClickListener, AView.IOnTouchListener
	{
		float _defaultFontSize;
		int? _defaultLabelFor;
		Typeface _defaultTypeface;
		int _imageHeight = -1;
		bool _isDisposed;
		bool _inputTransparent;
		Lazy<TextColorSwitcher> _textColorSwitcher;
		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		readonly EffectControlProvider _effectControlProvider;
		VisualElementTracker _tracker;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public ButtonRenderer(Context context) : base(context)
		{
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_effectControlProvider = new EffectControlProvider(this);

			Initialize();
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ButtonRenderer(Context) instead.")]
		public ButtonRenderer() : base(Forms.Context)
		{
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_effectControlProvider = new EffectControlProvider(this);
			
			Initialize();
		}

		public VisualElement Element => Button;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		Button Button { get; set; }

		public void OnClick(AView v)
		{
			((IButtonController)Button)?.SendClicked();
		}

		public bool OnTouch(AView v, MotionEvent e)
		{
			var buttonController = Element as IButtonController;
			switch (e.Action)
			{
				case AMotionEventActions.Down:
					buttonController?.SendPressed();
					break;
				case AMotionEventActions.Up:
					buttonController?.SendReleased();
					break;
			}

			return false;
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		void IOnAttachStateChangeListener.OnViewAttachedToWindow(AView attachedView)
		{
			UpdateText();
		}

		void IOnAttachStateChangeListener.OnViewDetachedFromWindow(AView detachedView)
		{
		}

		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			OnNativeFocusChanged(hasFocus);

			((IElementController)Button).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			UpdateText();

			AView view = this;
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

			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			Color currentColor = oldElement?.BackgroundColor ?? Color.Default;
			if (element.BackgroundColor != currentColor)
			{
				UpdateBackgroundColor();
			}

			element.PropertyChanged += OnElementPropertyChanged;

			if (_tracker == null)
			{
				// Can't set up the tracker in the constructor because it access the Element (for now)
				SetTracker(new VisualElementTracker(this));
			}

			OnElementChanged(new ElementChangedEventArgs<Button>(oldElement as Button, Button));

			SendVisualElementInitialized(element, this);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			Performance.Stop(reference);
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
			{
				_defaultLabelFor = LabelFor;
			}

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			var reference = Guid.NewGuid().ToString();
			_tracker?.UpdateLayout();
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

		void OnElementChanged(ElementChangedEventArgs<Button> e)
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
				UpdateIsEnabled();
				UpdateInputTransparent();
				UpdateBackgroundColor();

				ElevationHelper.SetElevation(this, e.NewElement);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Button.TextProperty.PropertyName)
			{
				UpdateText();
			}
			else if (e.PropertyName == Button.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
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
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
			{
				UpdateBackgroundColor();
			}

			ElementPropertyChanged?.Invoke(this, e);
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
				int diff = (b - t - _imageHeight) / 2;
				diff = Math.Max(diff, 0);
				SetPadding(0, diff, 0, -diff);
			}

			base.OnLayout(changed, l, t, r, b);
		}

		void SetTracker(VisualElementTracker tracker)
		{
			_tracker = tracker;
		}

		void UpdateBackgroundColor()
		{
			if (Element == null)
			{
				return;
			}

			Color backgroundColor = Element.BackgroundColor;
			if (backgroundColor.IsDefault)
			{
				if (SupportBackgroundTintList != null)
				{
					Context context = Context;
					int id = GlobalResource.Attribute.ButtonTint;
					unchecked
					{
						using (var value = new TypedValue())
						{
							try
							{
								Resources.Theme theme = context.Theme;
								if (theme != null && theme.ResolveAttribute(id, value, true))
#pragma warning disable 618
								{
									SupportBackgroundTintList = Resources.GetColorStateList(value.Data);
								}
#pragma warning restore 618
								else
								{
									SupportBackgroundTintList = new ColorStateList(ColorExtensions.States,
										new[] { (int)0xffd7d6d6, 0x7fd7d6d6 });
								}
							}
							catch (Exception ex)
							{
								Internals.Log.Warning("Xamarin.Forms.Platform.Android.ButtonRenderer",
									"Could not retrieve button background resource: {0}", ex);
								SupportBackgroundTintList = new ColorStateList(ColorExtensions.States,
									new[] { (int)0xffd7d6d6, 0x7fd7d6d6 });
							}
						}
					}
				}
			}
			else
			{
				int intColor = backgroundColor.ToAndroid().ToArgb();
				int disableColor = backgroundColor.MultiplyAlpha(0.5).ToAndroid().ToArgb();
				SupportBackgroundTintList = new ColorStateList(ColorExtensions.States, new[] { intColor, disableColor });
			}
		}

		internal void OnNativeFocusChanged(bool hasFocus)
		{
		}

		internal void SendVisualElementInitialized(VisualElement element, AView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void Initialize()
		{
			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			AddOnAttachStateChangeListener(this);
			OnFocusChangeListener = this;

			Tag = this;
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

			if (IsNullOrEmpty(Button.Text))
			{
				// No text, so no need for relative position; just center the image
				// There's no option for just plain-old centering, so we'll use Top 
				// (which handles the horizontal centering) and some tricksy padding (in OnLayout)
				// to handle the vertical centering 

				// Clear any previous padding and set the image as top/center
				SetPadding(0, 0, 0, 0);
				SetCompoundDrawablesWithIntrinsicBounds(null, image, null, null);

				// Keep track of the image height so we can use it in OnLayout
				_imageHeight = image.IntrinsicHeight;

				image.Dispose();
				return;
			}

			Button.ButtonContentLayout layout = Button.ContentLayout;

			CompoundDrawablePadding = (int)layout.Spacing;

			switch (layout.Position)
			{
				case Button.ButtonContentLayout.ImagePosition.Top:
					SetCompoundDrawablesWithIntrinsicBounds(null, image, null, null);
					break;
				case Button.ButtonContentLayout.ImagePosition.Bottom:
					SetCompoundDrawablesWithIntrinsicBounds(null, null, null, image);
					break;
				case Button.ButtonContentLayout.ImagePosition.Right:
					SetCompoundDrawablesWithIntrinsicBounds(null, null, image, null);
					break;
				default:
					// Defaults to image on the left
					SetCompoundDrawablesWithIntrinsicBounds(image, null, null, null);
					break;
			}

			image?.Dispose();
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

		void UpdateIsEnabled()
		{
			if (Element == null || _isDisposed)
			{
				return;
			}

			Enabled = Element.IsEnabled;
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
			{
				return;
			}

			_textColorSwitcher.Value.UpdateTextColor(this, Button.TextColor);
		}
	}
}
