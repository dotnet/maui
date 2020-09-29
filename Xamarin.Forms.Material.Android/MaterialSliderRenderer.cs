using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.FastRenderers;
using AView = Android.Views.View;


namespace Xamarin.Forms.Material.Android
{
	public class MaterialSliderRenderer : SeekBar,
		SeekBar.IOnSeekBarChangeListener,
		IVisualElementRenderer, IViewRenderer, ITabStop
	{
		const double MaximumValue = 10000.0;
		int? _defaultLabelFor;
		bool _disposed;
		Slider _element;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;
		MotionEventHelper _motionEventHelper;
		double _max = 0.0;
		double _min = 0.0;
		bool _inputTransparent;

		public MaterialSliderRenderer(Context context)
			: base(MaterialContextThemeWrapper.Create(context), null, Resource.Attribute.materialSliderStyle)
		{
			SetOnSeekBarChangeListener(this);
			Max = (int)MaximumValue;

			_visualElementRenderer = new VisualElementRenderer(this);
			_motionEventHelper = new MotionEventHelper();
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (!Enabled || _inputTransparent)
				return false;

			if (_visualElementRenderer.OnTouchEvent(e) || base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected SeekBar Control => this;

		protected Slider Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				var oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Slider>(oldElement, _element));

				_element?.SendViewInitialized(this);

				_motionEventHelper.UpdateElement(_element);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			_disposed = true;

			if (disposing)
			{
				_visualElementTracker?.Dispose();
				_visualElementTracker = null;

				_visualElementRenderer?.Dispose();
				_visualElementRenderer = null;
				SetOnSeekBarChangeListener(null);

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.Android.Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.Android.Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));

			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				this.EnsureId();

				if (_visualElementTracker == null)
					_visualElementTracker = new VisualElementTracker(this);

				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				UpdateValue();
				UpdateColors();
				UpdateInputTransparent();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.IsOneOf(Slider.ValueProperty, Slider.MinimumProperty, Slider.MaximumProperty))
				UpdateValue();
			else if (e.IsOneOf(VisualElement.BackgroundColorProperty, VisualElement.BackgroundProperty, Slider.MaximumTrackColorProperty, Slider.MinimumTrackColorProperty, Slider.ThumbColorProperty))
				UpdateColors();
			else if (e.PropertyName == VisualElement.InputTransparentProperty.PropertyName)
				UpdateInputTransparent();
		}


		void UpdateInputTransparent()
		{
			if (Element == null)
				return;

			_inputTransparent = Element.InputTransparent;
		}

		void UpdateColors()
		{
			if (Element == null || Control == null)
				return;

			Color backgroundColor = Element.MaximumTrackColor;
			if (backgroundColor == Color.Default)
				backgroundColor = Element.BackgroundColor;
			Color progressColor = Element.MinimumTrackColor;
			Color thumbColor = Element.ThumbColor;

			this.ApplySeekBarColors(progressColor, backgroundColor, thumbColor);
		}

		double Value
		{
			get { return _min + (_max - _min) * (Control.Progress / MaximumValue); }
			set { Control.Progress = (int)((value - _min) / (_max - _min) * MaximumValue); }
		}
		void UpdateValue()
		{
			_min = Element.Minimum;
			_max = Element.Maximum;
			Value = Element.Value;
		}

		// SeekBar.IOnSeekBarChangeListener
		void SeekBar.IOnSeekBarChangeListener.OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			if (fromUser)
				((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, Value);
		}

		void SeekBar.IOnSeekBarChangeListener.OnStartTrackingTouch(SeekBar seekBar) => ((ISliderController)Element)?.SendDragStarted();

		void SeekBar.IOnSeekBarChangeListener.OnStopTrackingTouch(SeekBar seekBar) => ((ISliderController)Element)?.SendDragCompleted();

		// IVisualElementRenderer
		VisualElement IVisualElementRenderer.Element => Element;
		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;
		ViewGroup IVisualElementRenderer.ViewGroup => null;
		AView IVisualElementRenderer.View => this;
		void IVisualElementRenderer.SetElement(VisualElement element) =>
			Element = (element as Slider) ?? throw new ArgumentException($"Element must be of type {nameof(Slider)}.");
		void IVisualElementRenderer.UpdateLayout() =>
			_visualElementTracker?.UpdateLayout();

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(Control.MeasuredWidth, Control.MeasuredHeight), new Size());
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}


		// IViewRenderer
		void IViewRenderer.MeasureExactly() =>
			ViewRenderer.MeasureExactly(this, Element, Context);

		// ITabStop
		AView ITabStop.TabStop => this;


	}
}