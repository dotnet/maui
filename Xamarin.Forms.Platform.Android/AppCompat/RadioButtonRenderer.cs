using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
#if __ANDROID_29__
using AndroidX.Core.View;
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V7.Widget;
using Android.Support.V4.View;
#endif
using Android.Util;
using Android.Views;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using Android.Graphics.Drawables;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public class RadioButtonRenderer : AppCompatRadioButton,
		IBorderVisualElementRenderer, IButtonLayoutRenderer, IVisualElementRenderer, IViewRenderer, ITabStop,
		AView.IOnAttachStateChangeListener, AView.IOnFocusChangeListener, AView.IOnClickListener, AView.IOnTouchListener,
		CompoundButton.IOnCheckedChangeListener
	{
		float _defaultFontSize;
		int? _defaultLabelFor;
		Typeface _defaultTypeface;
		bool _isDisposed;
		bool _inputTransparent;
		Lazy<TextColorSwitcher> _textColorSwitcher;
		AutomationPropertiesProvider _automationPropertiesProvider;
		VisualElementTracker _tracker;
		VisualElementRenderer _visualElementRenderer;
		BorderBackgroundManager _backgroundTracker;
		ButtonLayoutManager _buttonLayoutManager;
		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> _platformElementConfiguration;
		Button _button;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public RadioButtonRenderer(Context context) : base(context)
		{
			Initialize();
		}

		protected Button Element => Button;
		protected AppCompatRadioButton Control => this;

		VisualElement IBorderVisualElementRenderer.Element => Element;

		VisualElement IVisualElementRenderer.Element => Element;
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

		void IOnAttachStateChangeListener.OnViewAttachedToWindow(AView attachedView) =>
			_buttonLayoutManager.OnViewAttachedToWindow(attachedView);

		void IOnAttachStateChangeListener.OnViewDetachedFromWindow(AView detachedView) =>
			_buttonLayoutManager.OnViewDetachedFromWindow(detachedView);

		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			((IElementController)Button).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			return _buttonLayoutManager.GetDesiredSize(widthConstraint, heightConstraint);
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
				OnFocusChangeListener = null;
				SetOnCheckedChangeListener(null);

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				_automationPropertiesProvider?.Dispose();
				_tracker?.Dispose();
				_visualElementRenderer?.Dispose();
				_backgroundTracker?.Dispose();
				_backgroundTracker = null;
				_buttonLayoutManager?.Dispose();
				_buttonLayoutManager = null;

				if (Element != null)
				{
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
			if (e.NewElement != null && !_isDisposed)
			{
				this.EnsureId();

				_textColorSwitcher = new Lazy<TextColorSwitcher>(
					() => new TextColorSwitcher(TextColors, e.NewElement.UseLegacyColorManagement()));

				UpdateFont();
				UpdateTextColor();
				UpdateInputTransparent();
				UpdateBackgroundColor();
				_buttonLayoutManager?.Update();
				UpdateButtonImage(true);
				UpdateIsChecked();
				ElevationHelper.SetElevation(this, e.NewElement);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Button.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == Button.FontProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent();
			}
			else if (e.PropertyName == RadioButton.IsCheckedProperty.PropertyName)
			{
				UpdateIsChecked();
			}
			else if (e.PropertyName == RadioButton.ButtonSourceProperty.PropertyName)
			{
				UpdateButtonImage(false);
			}

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			_buttonLayoutManager?.OnLayout(changed, l, t, r, b);
			base.OnLayout(changed, l, t, r, b);
		}

		void SetTracker(VisualElementTracker tracker)
		{
			_tracker = tracker;
		}

		void UpdateBackgroundColor()
		{
			_backgroundTracker?.UpdateDrawable();
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
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_buttonLayoutManager = new ButtonLayoutManager(this);
			_backgroundTracker = new BorderBackgroundManager(this);

			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			AddOnAttachStateChangeListener(this);
			OnFocusChangeListener = this;
			SetOnCheckedChangeListener(this);

			Tag = this;
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

		void UpdateTextColor()
		{
			if (Element == null || _isDisposed || _textColorSwitcher == null)
			{
				return;
			}

			_textColorSwitcher.Value.UpdateTextColor(this, Button.TextColor);
		}

		void UpdateButtonImage(bool isInitializing)
		{
			if (Element == null || _isDisposed)
				return;

			ImageSource buttonSource = ((RadioButton)Element).ButtonSource;
			if (buttonSource != null && !buttonSource.IsEmpty)
			{
				Drawable currButtonImage = Control.ButtonDrawable;

				this.ApplyDrawableAsync(RadioButton.ButtonSourceProperty, Context, image =>
				{
					if (image == currButtonImage)
						return;
					Control.SetButtonDrawable(image);

					Element.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
				});
			}
			else if(!isInitializing)
				Control.SetButtonDrawable(null);
		}

		void UpdateIsChecked()
		{
			if (Element == null || Control == null)
				return;

			Checked = ((RadioButton)Element).IsChecked;
		}

		void IOnCheckedChangeListener.OnCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			((IElementController)Element).SetValueFromRenderer(RadioButton.IsCheckedProperty, isChecked);
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

		AppCompatButton IButtonLayoutRenderer.View => null;

		Button IButtonLayoutRenderer.Element => this.Element;

	}
}