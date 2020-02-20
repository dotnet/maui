using System;
using System.ComponentModel;
using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Android.Graphics.Drawables;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using AShapes = Android.Graphics.Drawables.Shapes;
using AShapeType = Android.Graphics.Drawables.ShapeType;

namespace Xamarin.Forms.Platform.Android
{
	public class IndicatorViewRenderer : LinearLayout, IVisualElementRenderer, IViewRenderer, ITabStop
	{
		VisualElementTracker _visualElementTracker;
		readonly VisualElementRenderer _visualElementRenderer;
		const int DefaultPadding = 4;

		void IViewRenderer.MeasureExactly()
		{
			ViewRenderer.MeasureExactly(this, Element, Context);
		}
		AView ITabStop.TabStop => this;

		protected IndicatorView IndicatorsView;

		int? _defaultLabelFor;
		bool _disposed;
		int _selectedIndex = 0;
		AColor _currentPageIndicatorTintColor;
		AShapeType _shapeType = AShapeType.Oval;
		Drawable _currentPageShape = null;
		Drawable _pageShape = null;
		AColor _pageIndicatorTintColor;
		bool IsVisible => Visibility != ViewStates.Gone;

		public VisualElement Element => IndicatorsView;

		public VisualElementTracker Tracker => _visualElementTracker;

		public ViewGroup ViewGroup => null;

		public AView View => this;

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

		public IndicatorViewRenderer(Context context) : base(context)
		{
			SetGravity(GravityFlags.Center);
			_visualElementRenderer = new VisualElementRenderer(this);
		}

		SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size());
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			Tracker?.UpdateLayout();
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!(element is IndicatorView))
			{
				throw new ArgumentException($"{nameof(element)} must be of type {typeof(IndicatorView).Name}");
			}

			var oldElement = IndicatorsView;
			var newElement = (IndicatorView)element;

			TearDownOldElement(oldElement);
			SetUpNewElement(newElement);

			OnElementChanged(oldElement, newElement);

		}

		void IVisualElementRenderer.SetLabelFor(int? id)
		{
			if (_defaultLabelFor == null)
			{
				_defaultLabelFor = LabelFor;
			}

			LabelFor = (int)(id ?? _defaultLabelFor);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				Tracker?.Dispose();

				if (Element != null)
				{
					TearDownOldElement(Element as IndicatorView);

					if (Platform.GetRenderer(Element) == this)
					{
						Element.ClearValue(Platform.RendererProperty);
					}
				}
			}
		}

		void OnElementChanged(IndicatorView oldElement, IndicatorView newElement)
		{
			ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(oldElement, newElement));
			OnElementChanged(new ElementChangedEventArgs<IndicatorView>(oldElement, newElement));
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<IndicatorView> elementChangedEvent)
		{

		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			ElementPropertyChanged?.Invoke(this, changedProperty);

			if (changedProperty.Is(IndicatorView.IndicatorTemplateProperty))
			{
				UpdateIndicatorTemplate();
			}
			else if (changedProperty.IsOneOf(IndicatorView.IndicatorsShapeProperty,
											IndicatorView.IndicatorColorProperty,
											IndicatorView.IndicatorSizeProperty,
											IndicatorView.SelectedIndicatorColorProperty))
			{
				ResetIndicators();
			}
			else if (changedProperty.Is(VisualElement.BackgroundColorProperty))
			{
				UpdateBackgroundColor();
			}
			else if (changedProperty.Is(IndicatorView.PositionProperty))
			{
				UpdateSelectedIndicator();
			}
			else if (changedProperty.Is(IndicatorView.CountProperty))
			{
				UpdateItemsSource();
			}
		}

		protected virtual void UpdateBackgroundColor(Color? color = null)
		{
			if (Element == null)
			{
				return;
			}

			SetBackgroundColor((color ?? Element.BackgroundColor).ToAndroid());
		}

		void SetUpNewElement(IndicatorView newElement)
		{
			if (newElement == null)
			{
				IndicatorsView = null;
				return;
			}

			IndicatorsView = newElement;

			IndicatorsView.PropertyChanged += OnElementPropertyChanged;

			if (Tracker == null)
			{
				_visualElementTracker = new VisualElementTracker(this);
			}

			this.EnsureId();

			UpdateBackgroundColor();

			if (IndicatorsView.IndicatorTemplate != null)
				UpdateIndicatorTemplate();
			else
				UpdateItemsSource();

			ElevationHelper.SetElevation(this, newElement);

			UpdateSelectedIndicator();
		}

		void UpdateSelectedIndicator()
		{
			var maxVisible = IndicatorsView.MaximumVisible;
			var position = IndicatorsView.Position;
			_selectedIndex = position >= maxVisible ? maxVisible - 1 : position;
			UpdateIndicators();
		}

		void UpdateItemsSource()
		{
			ResetIndicators();
			UpdateIndicatorCount();
		}

		void TearDownOldElement(IndicatorView oldElement)
		{
			if (oldElement == null)
			{
				return;
			}

			oldElement.PropertyChanged -= OnElementPropertyChanged;
		}

		void UpdateIndicatorCount()
		{
			if (!IsVisible)
				return;

			var count = IndicatorsView.Count;

			if (IndicatorsView.MaximumVisible != int.MaxValue)
				count = IndicatorsView.MaximumVisible;

			var childCount = ChildCount;

			for (int i = childCount; i < count; i++)
			{
				var imageView = new ImageView(Context);
				if (Orientation == Orientation.Horizontal)
					imageView.SetPadding((int)Context.ToPixels(DefaultPadding), 0, (int)Context.ToPixels(DefaultPadding), 0);
				else
					imageView.SetPadding(0, (int)Context.ToPixels(DefaultPadding), 0, (int)Context.ToPixels(DefaultPadding));

				imageView.SetImageDrawable(_selectedIndex == i ? _currentPageShape : _pageShape);
				AddView(imageView);
			}

			childCount = ChildCount;

			for (int i = count; i < childCount; i++)
			{
				RemoveViewAt(ChildCount - 1);
			}
			IndicatorsView.NativeSizeChanged();
		}

		void ResetIndicators()
		{
			if (!IsVisible)
				return;

			_pageIndicatorTintColor = IndicatorsView.IndicatorColor.ToAndroid();
			_currentPageIndicatorTintColor = IndicatorsView.SelectedIndicatorColor.ToAndroid();
			_shapeType = IndicatorsView.IndicatorsShape == IndicatorShape.Circle ? AShapeType.Oval : AShapeType.Rectangle;
			_pageShape = null;
			_currentPageShape = null;

			if (IndicatorsView.IndicatorTemplate == null)
				UpdateShapes();
			else
				UpdateIndicatorTemplate();

			UpdateIndicators();
		}

		void UpdateIndicatorTemplate()
		{
			if (IndicatorsView.IndicatorLayout == null)
				return;

			var renderer = IndicatorsView.IndicatorLayout.GetRenderer() ?? Platform.CreateRendererWithContext(IndicatorsView.IndicatorLayout, Context);
			Platform.SetRenderer(IndicatorsView.IndicatorLayout, renderer);

			RemoveAllViews();
			AddView(renderer.View);

			var indicatorLayoutSizeRequest = IndicatorsView.IndicatorLayout.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.IncludeMargins);
			IndicatorsView.IndicatorLayout.Layout(new Rectangle(0, 0, indicatorLayoutSizeRequest.Request.Width, indicatorLayoutSizeRequest.Request.Height));
		}

		void UpdateIndicators()
		{
			if (!IsVisible)
				return;

			var count = ChildCount;
			for (int i = 0; i < count; i++)
			{
				ImageView view = GetChildAt(i) as ImageView;
				if (view == null)
					continue;
				var drawableToUse = _selectedIndex == i ? _currentPageShape : _pageShape;
				if (drawableToUse != view.Drawable)
					view.SetImageDrawable(drawableToUse);
			}
		}

		void UpdateShapes()
		{
			if (_currentPageShape != null)
				return;

			_currentPageShape = GetShape(_currentPageIndicatorTintColor);
			_pageShape = GetShape(_pageIndicatorTintColor);
		}

		Drawable GetShape(AColor color)
		{
			var indicatorSize = IndicatorsView.IndicatorSize;
			ShapeDrawable shape;

			if (_shapeType == AShapeType.Oval)
				shape = new ShapeDrawable(new AShapes.OvalShape());
			else
				shape = new ShapeDrawable(new AShapes.RectShape());

			shape.SetIntrinsicHeight((int)Context.ToPixels(indicatorSize));
			shape.SetIntrinsicWidth((int)Context.ToPixels(indicatorSize));
			shape.Paint.Color = color;

			return shape;
		}
	}
}