
using System;
using System.ComponentModel;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
#if __ANDROID_29__
using AndroidX.Core.View;
#else
using Android.Support.V4.View;
#endif
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
using MButton = Google.Android.Material.Button.MaterialButton;
#else
using Android.Support.V7.Widget;
using MButton = Android.Support.Design.Button.MaterialButton;
#endif
using Android.Util;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.Material.Android;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using Xamarin.Forms.Platform.Android;


namespace Xamarin.Forms.Material.Android
{
	public class MaterialButtonRenderer : MButton,
		IBorderVisualElementRenderer, IButtonLayoutRenderer, IVisualElementRenderer, IViewRenderer, ITabStop,
		AView.IOnAttachStateChangeListener, AView.IOnFocusChangeListener, AView.IOnClickListener, AView.IOnTouchListener
	{
		int _defaultCornerRadius = -1;
		int _defaultBorderWidth = -1;
		ColorStateList _defaultBorderColor;
		float _defaultFontSize = -1;
		int? _defaultLabelFor;
		Typeface _defaultTypeface;
		bool _disposed;
		bool _inputTransparent;
		Button _button;
		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> _platformElementConfiguration;
		VisualElementTracker _tracker;
		VisualElementRenderer _visualElementRenderer;
		ButtonLayoutManager _buttonLayoutManager;
		readonly AutomationPropertiesProvider _automationPropertiesProvider;

		public MaterialButtonRenderer(Context context)
			: this(MaterialContextThemeWrapper.Create(context), null) { }

		public MaterialButtonRenderer(Context context, BindableObject element)
			: base(MaterialContextThemeWrapper.Create(context))
		{
			_automationPropertiesProvider = new AutomationPropertiesProvider(this);
			_buttonLayoutManager = new ButtonLayoutManager(this,
				alignIconWithText: true,
				preserveInitialPadding: true,
				borderAdjustsPadding: false,
				maintainLegacyMeasurements: false);

			SoundEffectsEnabled = false;
			SetOnClickListener(this);
			SetOnTouchListener(this);
			AddOnAttachStateChangeListener(this);
			OnFocusChangeListener = this;

			Tag = this;
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;
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

		public override void Draw(Canvas canvas)
		{
			if(Element == null || Element.CornerRadius <= 0)
			{
				base.Draw(canvas);
				return;
			}

			try
			{
				var radiusToPixels = (float)Context.ToPixels(Element.CornerRadius);

				using (var path = new Path())
				{
					RectF rect = new RectF(0, 0, canvas.Width, canvas.Height);
					path.AddRoundRect(rect, radiusToPixels, radiusToPixels, Path.Direction.Ccw);
					canvas.Save();
					canvas.ClipPath(path);
					base.Draw(canvas);
				}

				canvas.Restore();
				return;
			}
			catch (Exception ex)
			{
				Internals.Log.Warning(nameof(MaterialButtonRenderer), $"Unable to create circle image: {ex}");
			}

			base.Draw(canvas);


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
				_buttonLayoutManager?.Dispose();
				_buttonLayoutManager = null;

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.Android.Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.Android.Platform.RendererProperty);
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

				_buttonLayoutManager?.Update();
				UpdateBorder();
				UpdateFont();
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
			else if (e.PropertyName == Button.TextColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdatePrimaryColors();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
			else if (e.PropertyName == Button.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();

			ElementPropertyChanged?.Invoke(this, e);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			_buttonLayoutManager?.OnLayout(changed, left, top, right, bottom);
			base.OnLayout(changed, left, top, right, bottom);
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
					StrokeColor = new ColorStateList(new[] { new int[0] }, new int[] { borderColor.ToAndroid() });
			}

			double borderWidth = Element.BorderWidth;
			if (borderWidth >= 0f || _defaultBorderWidth >= 0f)
			{
				if (_defaultBorderWidth < 0)
					_defaultBorderWidth = StrokeWidth;

				// TODO: The Material button does not support borders:
				//       https://github.com/xamarin/Xamarin.Forms/issues/4951
				if (borderWidth > 1)
					borderWidth = 1;

				if (borderWidth < 0f)
					StrokeWidth = _defaultBorderWidth;
				else
					StrokeWidth = (int)Context.ToPixels(borderWidth);
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
			ViewCompat.SetBackgroundTintList(this, MaterialColors.CreateButtonBackgroundColors(background));
		}

		void UpdateCharacterSpacing()
		{
			LetterSpacing = Element.CharacterSpacing.ToEm();
		}

		IPlatformElementConfiguration<PlatformConfiguration.Android, Button> OnThisPlatform() =>
			_platformElementConfiguration ?? (_platformElementConfiguration = Element.OnThisPlatform());

		// IOnAttachStateChangeListener
		void IOnAttachStateChangeListener.OnViewAttachedToWindow(AView attachedView) =>
			_buttonLayoutManager?.OnViewAttachedToWindow(attachedView);

		void IOnAttachStateChangeListener.OnViewDetachedFromWindow(AView detachedView) =>
			_buttonLayoutManager?.OnViewDetachedFromWindow(detachedView);

		// IOnFocusChangeListener
		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus) =>
			Element.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);

		// IOnClickListener
		void IOnClickListener.OnClick(AView v) =>
			ButtonElementManager.OnClick(Element, Element, v);

		// IOnTouchListener
		bool IOnTouchListener.OnTouch(AView v, MotionEvent e) =>
			ButtonElementManager.OnTouch(Element, Element, v, e);

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
			return _buttonLayoutManager.GetDesiredSize(widthConstraint, heightConstraint);
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

		// IButtonLayoutRenderer
		AppCompatButton IButtonLayoutRenderer.View => this;

		Button IButtonLayoutRenderer.Element => this.Element;

		event EventHandler<VisualElementChangedEventArgs> IButtonLayoutRenderer.ElementChanged
		{
			add => ((IVisualElementRenderer)this).ElementChanged += value;
			remove => ((IVisualElementRenderer)this).ElementChanged -= value;
		}
	}
}
