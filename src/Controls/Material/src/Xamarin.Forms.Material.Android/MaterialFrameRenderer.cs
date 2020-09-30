
using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.View;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.FastRenderers;
using AView = Android.Views.View;
using MaterialCardView = Google.Android.Material.Card.MaterialCardView;

namespace Xamarin.Forms.Material.Android
{
	public class MaterialFrameRenderer : MaterialCardView,
		IVisualElementRenderer, IEffectControlProvider, IViewRenderer, ITabStop
	{
		float _defaultElevation = -1f;
		float _defaultCornerRadius = -1f;
		int _defaultStrokeWidth = -1;
		int? _defaultBackgroundColor;
		int? _defaultStrokeColor;
		int? _defaultLabelFor;
		bool _disposed;
		Frame _element;
		VisualElementTracker _visualElementTracker;
		VisualElementPackager _visualElementPackager;
		readonly GestureManager _gestureManager;
		readonly EffectControlProvider _effectControlProvider;
		readonly MotionEventHelper _motionEventHelper;
		Drawable _defaultBackgroundDrawable;
		GradientDrawable _backgroundGradientDrawable;

		public MaterialFrameRenderer(Context context)
			: base(MaterialContextThemeWrapper.Create(context))
		{
			_gestureManager = new GestureManager(this);
			_effectControlProvider = new EffectControlProvider(this);
			_motionEventHelper = new MotionEventHelper();
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (_gestureManager.OnTouchEvent(e) || base.OnTouchEvent(e))
				return true;

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected MaterialCardView Control => this;

		protected Frame Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				var oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Frame>(oldElement, _element));

				_element?.SendViewInitialized(this);

				_motionEventHelper.UpdateElement(_element);

				if (!string.IsNullOrEmpty(Element.AutomationId))
					ContentDescription = Element.AutomationId;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;
			_disposed = true;

			if (disposing)
			{
				_gestureManager?.Dispose();

				_visualElementTracker?.Dispose();
				_visualElementTracker = null;

				_visualElementPackager?.Dispose();
				_visualElementPackager = null;

				_defaultBackgroundDrawable?.Dispose();
				_defaultBackgroundDrawable = null;

				_backgroundGradientDrawable?.Dispose();
				_backgroundGradientDrawable = null;

				var count = ChildCount;
				for (var i = 0; i < count; i++)
				{
					var child = GetChildAt(i);
					child.Dispose();
				}

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;

					if (Platform.Android.Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.Android.Platform.RendererProperty);
				}
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<Frame> e)
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
				{
					_visualElementTracker = new VisualElementTracker(this);
					_visualElementPackager = new VisualElementPackager(this);
					_visualElementPackager.Load();
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;

				UpdateShadow();
				UpdateCornerRadius();
				UpdateBorder();
				UpdateBackground();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(this, e);

			if (e.PropertyName == Frame.HasShadowProperty.PropertyName)
				UpdateShadow();
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName)
				UpdateBorder();
			else if (e.IsOneOf(VisualElement.BackgroundColorProperty, VisualElement.BackgroundProperty))
				UpdateBackground();
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (Element == null)
				return;

			var children = Element.LogicalChildren;
			for (var i = 0; i < children.Count; i++)
			{
				if (children[i] is VisualElement visualElement)
				{
					var renderer = Platform.Android.Platform.GetRenderer(visualElement);
					renderer?.UpdateLayout();
				}
			}
		}

		void UpdateShadow()
		{
			if (_disposed || Element == null)
				return;

			// set the default elevation on the first time
			if (_defaultElevation < 0)
				_defaultElevation = CardElevation;

			if (Element.HasShadow)
				CardElevation = _defaultElevation;
			else
				CardElevation = 0f;
		}

		void UpdateCornerRadius()
		{
			if (_disposed || Element == null)
				return;

			var cornerRadius = Element.CornerRadius;

			if (_defaultCornerRadius < 0f)
				_defaultCornerRadius = Context.ToPixels(MaterialColors.kFrameCornerRadiusDefault);

			if (cornerRadius < 0f)
				Radius = _defaultCornerRadius;
			else
				Radius = (int)Context.ToPixels(cornerRadius);

			UpdateBorder();
		}

		void UpdateBorder()
		{
			if (_disposed || Element == null)
				return;

			var borderColor = Element.BorderColor;
			if (borderColor.IsDefault && _defaultStrokeColor == null && _defaultStrokeWidth < 0f)
				return;

			if (_defaultStrokeColor == null)
				_defaultStrokeColor = StrokeColor;
			if (_defaultStrokeWidth < 0)
				_defaultStrokeWidth = StrokeWidth;

			if (borderColor.IsDefault)
			{
				StrokeColor = _defaultStrokeColor.Value;
				StrokeWidth = _defaultStrokeWidth;
			}
			else
			{
				StrokeColor = borderColor.ToAndroid();
				StrokeWidth = (int)Context.ToPixels(1);
			}

			// update the native and forms view with the border
			SetContentPadding(0, 0, 0, 0);
		}

		void UpdateBackground()
		{
			if (_disposed || Element == null)
				return;

			var bgBrush = Element.Background;

			if (Brush.IsNullOrEmpty(bgBrush))
			{
				var bgColor = Element.BackgroundColor;

				if (bgColor.IsDefault && _defaultBackgroundColor == null)
					return;

				if (_defaultBackgroundDrawable != null)
					Background = _defaultBackgroundDrawable;

				if (_defaultBackgroundColor == null)
					_defaultBackgroundColor = CardBackgroundColor.DefaultColor;

				SetCardBackgroundColor(bgColor.IsDefault ? _defaultBackgroundColor.Value : bgColor.ToAndroid());
			}
			else
			{
				if (_defaultBackgroundDrawable == null)
					_defaultBackgroundDrawable = Background;

				_backgroundGradientDrawable = new GradientDrawable();
				_backgroundGradientDrawable.SetShape(ShapeType.Rectangle);

				_backgroundGradientDrawable.SetCornerRadius(Radius);
				_backgroundGradientDrawable.UpdateBackground(bgBrush, Height, Width);

				Background = _backgroundGradientDrawable;
			}
		}

		// IVisualElementRenderer
		VisualElement IVisualElementRenderer.Element => Element;
		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;
		ViewGroup IVisualElementRenderer.ViewGroup => this;
		AView IVisualElementRenderer.View => this;
		void IVisualElementRenderer.SetElement(VisualElement element) =>
			Element = (element as Frame) ?? throw new ArgumentException("Element must be of type Frame.");
		void IVisualElementRenderer.UpdateLayout() =>
			_visualElementTracker?.UpdateLayout();

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			var context = Context;
			return new SizeRequest(new Size(context.ToPixels(20), context.ToPixels(20)));
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		// IEffectControlProvider
		void IEffectControlProvider.RegisterEffect(Effect effect) =>
			_effectControlProvider?.RegisterEffect(effect);

		// IViewRenderer
		void IViewRenderer.MeasureExactly() =>
			ViewRenderer.MeasureExactly(this, Element, Context);

		// ITabStop
		AView ITabStop.TabStop => this;
	}
}