using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	public class ButtonRenderer : AppCompatButton,
		IBorderVisualElementRenderer, IButtonLayoutRenderer, IVisualElementRenderer, IViewRenderer, ITabStop,
		AView.IOnAttachStateChangeListener, AView.IOnFocusChangeListener, AView.IOnClickListener, AView.IOnTouchListener
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
		bool _hasLayoutOccurred;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public ButtonRenderer(Context context) : base(context)
		{
			Initialize();
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use ButtonRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ButtonRenderer() : base(Forms.Context)
		{
			Initialize();
		}

		protected Button Element => Button;
		protected AppCompatButton Control => this;

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
			if (_isDisposed)
			{
				return new SizeRequest();
			}

			var hint = Control.Hint;
			bool setHint = Control.LayoutParameters != null;

			if (!string.IsNullOrWhiteSpace(hint) && setHint)
			{
				Control.Hint = string.Empty;
			}

			var result = _buttonLayoutManager.GetDesiredSize(widthConstraint, heightConstraint);

			if (setHint)
				Control.Hint = hint;

			return result;
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

		public override void Draw(Canvas canvas)
		{
			canvas.ClipShape(Context, Element);

			if (_backgroundTracker?.BackgroundDrawable != null)
				_backgroundTracker.BackgroundDrawable.DrawCircle(canvas, canvas.Width, canvas.Height, base.Draw);
			else
				base.Draw(canvas);
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
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null && !_isDisposed)
			{
				this.EnsureId();

				if (_tracker == null)
				{
					// Can't set up the tracker in the constructor because it access the Element (for now)
					SetTracker(new VisualElementTracker(this));
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				_textColorSwitcher = new Lazy<TextColorSwitcher>(
					() => new TextColorSwitcher(TextColors, e.NewElement.UseLegacyColorManagement()));

				UpdateFont();
				UpdateTextColor();
				UpdateInputTransparent();
				UpdateBackgroundColor();
				UpdateCharacterSpacing();
				_buttonLayoutManager?.Update();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			if (Control?.LayoutParameters == null && _hasLayoutOccurred)
			{
				ElementPropertyChanged?.Invoke(this, e);
				return;
			}

			if (e.PropertyName == Button.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == Button.FontProperty.PropertyName)
			{
				UpdateFont();
			}
			else if (e.PropertyName == Button.CharacterSpacingProperty.PropertyName)
			{
				UpdateCharacterSpacing();
			}
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent();
			}

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			_buttonLayoutManager?.OnLayout(changed, l, t, r, b);
			base.OnLayout(changed, l, t, r, b);
			_hasLayoutOccurred = true;
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
			_visualElementRenderer = new VisualElementRenderer(this);

			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			AddOnAttachStateChangeListener(this);
			OnFocusChangeListener = this;

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

		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				LetterSpacing = Button.CharacterSpacing.ToEm();
			}
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

		AppCompatButton IButtonLayoutRenderer.View => this;

		Button IButtonLayoutRenderer.Element => this.Element;
	}
}