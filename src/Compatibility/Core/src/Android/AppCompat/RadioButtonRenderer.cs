using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers;
using Microsoft.Maui.Controls.Internals;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public class RadioButtonRenderer : AppCompatRadioButton,
		IBorderVisualElementRenderer, IVisualElementRenderer, IViewRenderer, ITabStop,
		AView.IOnFocusChangeListener,
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
		IPlatformElementConfiguration<PlatformConfiguration.Android, RadioButton> _platformElementConfiguration;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public RadioButtonRenderer(Context context) : base(context)
		{
			Initialize();
		}

		protected RadioButton Element { get; set; }
		protected AppCompatRadioButton Control => this;

		VisualElement IBorderVisualElementRenderer.Element => Element;

		VisualElement IVisualElementRenderer.Element => Element;
		AView IVisualElementRenderer.View => this;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		VisualElementTracker IVisualElementRenderer.Tracker => _tracker;

		AView ITabStop.TabStop => this;

		void IOnFocusChangeListener.OnFocusChange(AView v, bool hasFocus)
		{
			((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, hasFocus);
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!(element is RadioButton))
			{
				throw new ArgumentException($"{nameof(element)} must be of type {nameof(RadioButton)}");
			}

			RadioButton oldElement = Element;
			Element = (RadioButton)element;

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

			OnElementChanged(new ElementChangedEventArgs<RadioButton>(oldElement, Element));

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

				if (Element != null)
				{
					if (AppCompat.Platform.GetRenderer(Element) == this)
						Element.ClearValue(AppCompat.Platform.RendererProperty);
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

		protected virtual void OnElementChanged(ElementChangedEventArgs<RadioButton> e)
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
				UpdateIsChecked();
				UpdateContent();
				ElevationHelper.SetElevation(this, e.NewElement);
			}

			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == RadioButton.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.IsOneOf(RadioButton.FontAttributesProperty, RadioButton.FontFamilyProperty, RadioButton.FontSizeProperty))
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
			else if (e.PropertyName == RadioButton.ContentProperty.PropertyName)
			{
				UpdateContent();
			}

			ElementPropertyChanged?.Invoke(this, e);
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
			_backgroundTracker = new BorderBackgroundManager(this);

			SoundEffectsEnabled = false;
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

			Font font = Font.OfSize(Element.FontFamily, Element.FontSize).WithAttributes(Element.FontAttributes);

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

			_textColorSwitcher.Value.UpdateTextColor(this, Element.TextColor);
		}

		void UpdateIsChecked()
		{
			if (Element == null || Control == null)
				return;

			Checked = ((RadioButton)Element).IsChecked;
		}

		void UpdateContent()
		{
			if (Element == null || Control == null)
			{
				return;
			}

			Control.Text = Element.ContentAsString();
		}

		void IOnCheckedChangeListener.OnCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			((IElementController)Element).SetValueFromRenderer(RadioButton.IsCheckedProperty, isChecked);
		}

		float IBorderVisualElementRenderer.ShadowRadius => ShadowRadius;
		float IBorderVisualElementRenderer.ShadowDx => ShadowDx;
		float IBorderVisualElementRenderer.ShadowDy => ShadowDy;
		AColor IBorderVisualElementRenderer.ShadowColor => ShadowColor;
		bool IBorderVisualElementRenderer.IsShadowEnabled() => true;
		AView IBorderVisualElementRenderer.View => this;

		IPlatformElementConfiguration<PlatformConfiguration.Android, RadioButton> OnThisPlatform()
		{
			if (_platformElementConfiguration == null)
				_platformElementConfiguration = Element.OnThisPlatform();

			return _platformElementConfiguration;
		}

		bool IBorderVisualElementRenderer.UseDefaultPadding() => true;
		bool IBorderVisualElementRenderer.UseDefaultShadow() => true;
	}
}