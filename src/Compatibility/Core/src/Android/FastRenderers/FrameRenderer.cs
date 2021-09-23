using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.CardView.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers
{
	public class FrameRenderer : CardView, IVisualElementRenderer, IViewRenderer, ITabStop
	{
		float _defaultElevation = -1f;
		float _defaultCornerRadius = -1f;
		int? _defaultLabelFor;

		int _height;
		int _width;
		bool _hasLayoutOccurred;
		bool _disposed;
		Frame _element;
		GradientDrawable _backgroundDrawable;

		VisualElementPackager _visualElementPackager;
		VisualElementTracker _visualElementTracker;
		VisualElementRenderer _visualElementRenderer;

		readonly MotionEventHelper _motionEventHelper = new MotionEventHelper();

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;
		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public FrameRenderer(Context context) : base(context)
		{
			_visualElementRenderer = new VisualElementRenderer(this);
		}

		protected CardView Control => this;

		AView ITabStop.TabStop => this;

		protected Frame Element
		{
			get { return _element; }
			set
			{
				if (_element == value)
					return;

				Frame oldElement = _element;
				_element = value;

				OnElementChanged(new ElementChangedEventArgs<Frame>(oldElement, _element));

				_element?.SendViewInitialized(Control);
			}
		}

		VisualElement IVisualElementRenderer.Element => Element;
		AView IVisualElementRenderer.View => this;

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthMeasureSpec, int heightMeasureSpec)
		{
			Measure(widthMeasureSpec, heightMeasureSpec);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight));
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			var frame = element as Frame;
			if (frame == null)
				throw new ArgumentException("Element must be of type Frame");
			Element = frame;
			_motionEventHelper.UpdateElement(frame);

			if (!string.IsNullOrEmpty(Element.AutomationId))
				ContentDescription = Element.AutomationId;
		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
				_defaultLabelFor = ViewCompat.GetLabelFor(this);

			ViewCompat.SetLabelFor(this, (int)(id ?? _defaultLabelFor));
		}

		VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

		void IVisualElementRenderer.UpdateLayout()
		{
			VisualElementTracker tracker = _visualElementTracker;
			tracker?.UpdateLayout();
		}

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
				}

				if (_visualElementTracker != null)
				{
					_visualElementTracker.Dispose();
					_visualElementTracker = null;
				}

				if (_visualElementPackager != null)
				{
					_visualElementPackager.Dispose();
					_visualElementPackager = null;
				}

				if (_backgroundDrawable != null)
				{
					_backgroundDrawable.Dispose();
					_backgroundDrawable = null;
				}

				if (_visualElementRenderer != null)
				{
					_visualElementRenderer.Dispose();
					_visualElementRenderer = null;
				}

				while (ChildCount > 0)
				{
					AView child = GetChildAt(0);
					child.RemoveFromParent();
					child.Dispose();
				}

				if (Element != null)
				{
					if (Platform.GetRenderer(Element) == this)
						Element.ClearValue(Platform.RendererProperty);
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
				_backgroundDrawable = new GradientDrawable();
				_backgroundDrawable.SetShape(ShapeType.Rectangle);
				this.SetBackground(_backgroundDrawable);

				if (_visualElementTracker == null)
				{
					_visualElementTracker = new VisualElementTracker(this);
					_visualElementPackager = new VisualElementPackager(this);
					_visualElementPackager.Load();
				}

				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateShadow();
				UpdateBackgroundColor();
				UpdateCornerRadius();
				UpdateBorderColor();
				UpdateClippedToBounds();

				ElevationHelper.SetElevation(this, e.NewElement);
			}
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			if (Element == null)
				return;

			var children = ((IElementController)Element).LogicalChildren;
			for (var i = 0; i < children.Count; i++)
			{
				var visualElement = children[i] as VisualElement;
				if (visualElement == null)
					continue;
				IVisualElementRenderer renderer = Platform.GetRenderer(visualElement);
				renderer?.UpdateLayout();
			}

			_hasLayoutOccurred = true;
		}

		public override void Draw(Canvas canvas)
		{
			canvas.ClipShape(Context, Element);

			base.Draw(canvas);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (base.OnTouchEvent(e))
			{
				return true;
			}

			return _motionEventHelper.HandleMotionEvent(Parent, e);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);

			if (w != _width || h != _height)
				UpdateBackground();
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (this.IsDisposed())
			{
				return;
			}

			ElementPropertyChanged?.Invoke(this, e);

			if (Control?.LayoutParameters == null && _hasLayoutOccurred)
			{
				return;
			}

			if (e.PropertyName == Frame.HasShadowProperty.PropertyName)
				UpdateShadow();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
			else if (e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Frame.CornerRadiusProperty.PropertyName)
				UpdateCornerRadius();
			else if (e.PropertyName == Frame.BorderColorProperty.PropertyName)
				UpdateBorderColor();
			else if (e.Is(Microsoft.Maui.Controls.Compatibility.Layout.IsClippedToBoundsProperty))
				UpdateClippedToBounds();
		}

		void UpdateClippedToBounds()
		{
			var shouldClip = Element.IsSet(Microsoft.Maui.Controls.Compatibility.Layout.IsClippedToBoundsProperty)
					? Element.IsClippedToBounds : Element.CornerRadius > 0f;

			this.SetClipToOutline(shouldClip);
		}

		void UpdateBackgroundColor()
		{
			if (_disposed)
				return;

			Color bgColor = Element.BackgroundColor;
			_backgroundDrawable.SetColor(bgColor?.ToAndroid() ?? AColor.White);
		}

		void UpdateBackground()
		{
			if (_disposed)
				return;

			Brush background = Element.Background;

			if (Brush.IsNullOrEmpty(background))
			{
				if (_backgroundDrawable.UseGradients())
				{
					_backgroundDrawable.Dispose();
					_backgroundDrawable = null;
					this.SetBackground(null);

					_backgroundDrawable = new GradientDrawable();
					_backgroundDrawable.SetShape(ShapeType.Rectangle);
					this.SetBackground(_backgroundDrawable);
				}

				UpdateBackgroundColor();
			}
			else
			{
				_height = Control.Height;
				_width = Control.Width;
				_backgroundDrawable.UpdateBackground(background, _height, _width);
			}
		}

		void UpdateBorderColor()
		{
			if (_disposed)
				return;

			Color borderColor = Element.BorderColor;

			if (borderColor == null)
				_backgroundDrawable.SetStroke(0, AColor.Transparent);
			else
				_backgroundDrawable.SetStroke(3, borderColor.ToAndroid());
		}

		void UpdateShadow()
		{
			if (_disposed)
				return;

			float elevation = _defaultElevation;

			if (elevation == -1f)
				_defaultElevation = elevation = CardElevation;

			if (Element.HasShadow)
				CardElevation = elevation;
			else
				CardElevation = 0f;
		}

		void UpdateCornerRadius()
		{
			if (_disposed)
				return;

			if (_defaultCornerRadius == -1f)
			{
				_defaultCornerRadius = Radius;
			}

			float cornerRadius = Element.CornerRadius;

			if (cornerRadius == -1f)
				cornerRadius = _defaultCornerRadius;
			else
				cornerRadius = Context.ToPixels(cornerRadius);

			_backgroundDrawable.SetCornerRadius(cornerRadius);

			UpdateClippedToBounds();
		}
	}
}
