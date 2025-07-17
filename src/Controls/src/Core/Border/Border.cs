using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Content))]
	public class Border : View, IContentView, IBorderView, IPaddingElement, ISafeAreaElement, ISafeAreaPage
	{
		float[]? _strokeDashPattern;

		WeakNotifyPropertyChangedProxy? _strokeShapeProxy = null;
		PropertyChangedEventHandler? _strokeShapeChanged;
		WeakNotifyPropertyChangedProxy? _strokeProxy = null;
		PropertyChangedEventHandler? _strokeChanged;

		~Border()
		{
			_strokeShapeProxy?.Unsubscribe();
			_strokeProxy?.Unsubscribe();
		}

		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View),
			typeof(Border), null, propertyChanged: ContentChanged);

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <summary>Bindable property for <see cref="SafeAreaIgnore"/>.</summary>
		public static readonly BindableProperty SafeAreaIgnoreProperty = SafeAreaElement.SafeAreaIgnoreProperty;

		public View? Content
		{
			get { return (View?)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		/// <summary>
		/// Gets or sets the safe area edges to ignore for this border.
		/// The default value is SafeAreaEdges.Default.
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the border should ignore safe area insets.
		/// Use SafeAreaRegions.Default to respect safe area, SafeAreaRegions.All to ignore all insets, 
		/// SafeAreaRegions.None to ensure content never displays behind blocking UI, or SafeAreaRegions.SoftInput for soft input aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaIgnore
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaIgnoreProperty);
			set => SetValue(SafeAreaElement.SafeAreaIgnoreProperty, value);
		}

		/// <summary>Bindable property for <see cref="StrokeShape"/>.</summary>
		public static readonly BindableProperty StrokeShapeProperty =
			BindableProperty.Create(nameof(StrokeShape), typeof(IShape), typeof(Border), new Rectangle(),
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue is not null)
						(bindable as Border)?.StopNotifyingStrokeShapeChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue is not null)
						(bindable as Border)?.NotifyStrokeShapeChanges();
				});

		void NotifyStrokeShapeChanges()
		{
			var strokeShape = StrokeShape;

			if (strokeShape is VisualElement visualElement)
			{
				AddLogicalChild(visualElement);
				_strokeShapeChanged ??= (sender, e) =>
				{
					if (e.PropertyName != nameof(Window) &&
						e.PropertyName != nameof(Parent))
					{
						OnPropertyChanged(nameof(StrokeShape));
					}
				};
				_strokeShapeProxy ??= new();
				_strokeShapeProxy.Subscribe(visualElement, _strokeShapeChanged);
			}
		}

		void StopNotifyingStrokeShapeChanges()
		{
			var strokeShape = StrokeShape;

			if (strokeShape is VisualElement visualElement)
			{
				RemoveLogicalChild(visualElement);
				_strokeShapeProxy?.Unsubscribe();
			}
		}

		/// <summary>Bindable property for <see cref="Stroke"/>.</summary>
		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Border), null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue is not null)
						(bindable as Border)?.StopNotifyingStrokeChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue is not null)
						(bindable as Border)?.NotifyStrokeChanges();
				});

		void NotifyStrokeChanges()
		{
			var stroke = Stroke;

			if (stroke is ImmutableBrush)
				return;

			if (stroke is not null)
			{
				SetInheritedBindingContext(stroke, BindingContext);
				_strokeChanged ??= (sender, e) => OnPropertyChanged(nameof(Stroke));
				_strokeProxy ??= new();
				_strokeProxy.Subscribe(stroke, _strokeChanged);

				OnParentResourcesChanged(this.GetMergedResources());
				((IElementDefinition)this).AddResourcesChangedListener(stroke.OnParentResourcesChanged);
			}
		}

		void StopNotifyingStrokeChanges()
		{
			var stroke = Stroke;

			if (stroke is ImmutableBrush)
				return;

			if (stroke is not null)
			{
				((IElementDefinition)this).RemoveResourcesChangedListener(stroke.OnParentResourcesChanged);

				SetInheritedBindingContext(stroke, null);
				_strokeProxy?.Unsubscribe();
			}
		}

		/// <summary>Bindable property for <see cref="StrokeThickness"/>.</summary>
		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Border), 1.0, propertyChanged: StrokeThicknessChanged);

		/// <summary>Bindable property for <see cref="StrokeDashArray"/>.</summary>
		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Border), null,
				defaultValueCreator: bindable => new DoubleCollection());

		/// <summary>Bindable property for <see cref="StrokeDashOffset"/>.</summary>
		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Border), 0.0);

		/// <summary>Bindable property for <see cref="StrokeLineCap"/>.</summary>
		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Border), PenLineCap.Flat);

		/// <summary>Bindable property for <see cref="StrokeLineJoin"/>.</summary>
		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Border), PenLineJoin.Miter);

		/// <summary>Bindable property for <see cref="StrokeMiterLimit"/>.</summary>
		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Border), 10.0);

		[System.ComponentModel.TypeConverter(typeof(StrokeShapeTypeConverter))]
		public IShape? StrokeShape
		{
			set { SetValue(StrokeShapeProperty, value); }
			get { return (IShape?)GetValue(StrokeShapeProperty); }
		}

		public Brush? Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush?)GetValue(StrokeProperty); }
		}

		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		public DoubleCollection? StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection?)GetValue(StrokeDashArrayProperty); }
		}

		public double StrokeDashOffset
		{
			set { SetValue(StrokeDashOffsetProperty, value); }
			get { return (double)GetValue(StrokeDashOffsetProperty); }
		}

		public PenLineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (PenLineCap)GetValue(StrokeLineCapProperty); }
		}

		public PenLineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
		}

		public double StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (double)GetValue(StrokeMiterLimitProperty); }
		}

		IShape? IBorderStroke.Shape => StrokeShape;

		Paint? IStroke.Stroke => Stroke;

		LineCap IStroke.StrokeLineCap =>
			StrokeLineCap switch
			{
				PenLineCap.Flat => LineCap.Butt,
				PenLineCap.Round => LineCap.Round,
				PenLineCap.Square => LineCap.Square,
				_ => LineCap.Butt
			};

		LineJoin IStroke.StrokeLineJoin =>
			StrokeLineJoin switch
			{
				PenLineJoin.Round => LineJoin.Round,
				PenLineJoin.Bevel => LineJoin.Bevel,
				PenLineJoin.Miter => LineJoin.Miter,
				_ => LineJoin.Round
			};

		public float[]? StrokeDashPattern
		{
			get
			{
				if (StrokeDashArray is INotifyCollectionChanged oldCollection)
					oldCollection.CollectionChanged -= OnStrokeDashArrayChanged;

				_strokeDashPattern = StrokeDashArray?.ToFloatArray();

				if (StrokeDashArray is INotifyCollectionChanged newCollection)
					newCollection.CollectionChanged += OnStrokeDashArrayChanged;

				return _strokeDashPattern;
			}
		}

		float IStroke.StrokeDashOffset => (float)StrokeDashOffset;

		float IStroke.StrokeMiterLimit => (float)StrokeMiterLimit;


		object? IContentView.Content => Content;

		IView? IContentView.PresentedContent => Content;

		public Size CrossPlatformArrange(Graphics.Rect bounds)
		{
			var inset = bounds.Inset(StrokeThickness);
			this.ArrangeContent(inset);
			return bounds.Size;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			var inset = Padding + StrokeThickness;
			return this.MeasureContent(inset, widthConstraint, heightConstraint);
		}

		public static void ContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Border border)
			{
				if (oldValue is Element oldElement)
				{
					border.RemoveLogicalChild(oldElement);
				}

				if (newValue is Element newElement)
				{
					border.AddLogicalChild(newElement);
				}
			}

			((IBorderView)bindable).InvalidateMeasure();
		}

		public static void StrokeThicknessChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((IBorderView)bindable).InvalidateMeasure();
		}

		public void OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			(this as IBorderView).InvalidateMeasure();
		}

		public Thickness PaddingDefaultValueCreator()
		{
			return Thickness.Zero;
		}

		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == StrokeThicknessProperty.PropertyName || propertyName == StrokeShapeProperty.PropertyName)
			{
				UpdateStrokeShape();
				Handler?.UpdateValue(nameof(IBorderStroke.Shape));
			}
			else if (propertyName == StrokeDashArrayProperty.PropertyName)
			{
				Handler?.UpdateValue(nameof(IBorderStroke.StrokeDashPattern));
			}
		}

		void OnStrokeDashArrayChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			Handler?.UpdateValue(nameof(IBorderStroke.StrokeDashPattern));
		}

		void UpdateStrokeShape()
		{
			if (StrokeShape is Shape strokeShape && StrokeThickness == 0)
			{
				strokeShape.StrokeThickness = StrokeThickness;
			}
		}

		/// <inheritdoc cref="ISafeAreaPage.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaPage.GetSafeAreaRegionsForEdge(int edge)
		{
			// Use direct property first, then fall back to attached property
			var regionForEdge = SafeAreaIgnore.GetEdge(edge);
			
			if (regionForEdge != SafeAreaRegions.Default)
			{
				return regionForEdge;
			}
			
			// Fall back to attached property if direct property is Default
			var fallbackRegion = SafeAreaElement.GetIgnoreForEdge(this, edge);
			
			// For Border, never return Default - return None instead
			if (fallbackRegion == SafeAreaRegions.Default)
			{
				return SafeAreaRegions.None;
			}
			
			return fallbackRegion;
		}

		/// <inheritdoc cref="ISafeAreaPage.SafeAreaInsets"/>
		Thickness ISafeAreaPage.SafeAreaInsets { set { } } // Default no-op implementation for borders

		/// <inheritdoc cref="ISafeAreaPage.IgnoreSafeAreaForEdge"/>
		bool ISafeAreaPage.IgnoreSafeAreaForEdge(int edge)
		{
			// Use direct property first, then fall back to attached property
			var regionForEdge = SafeAreaIgnore.GetEdge(edge);
			
			// Handle the SafeAreaRegions behavior
			if (regionForEdge.HasFlag(SafeAreaRegions.All))
			{
				return true; // Ignore all insets - content may be positioned anywhere
			}

			if (regionForEdge == SafeAreaRegions.None || regionForEdge == SafeAreaRegions.SoftInput)
			{
				// Content will never display behind anything that could block it
				// Or treat SoftInput as respecting safe area for now
				return false;
			}

			if (regionForEdge == SafeAreaRegions.Default)
			{
				// Check if attached property is set, if not fall back to default behavior
				if (this.SafeAreaIgnore != SafeAreaEdges.Default)
				{
					return SafeAreaElement.ShouldIgnoreSafeAreaForEdge(this, edge);
				}
				
				// Default behavior for Border is to respect safe area
				return false;
			}

			return false;
		}

		SafeAreaEdges ISafeAreaElement.SafeAreaIgnoreDefaultValueCreator()
		{
			return SafeAreaEdges.Default;
		}
	}
}